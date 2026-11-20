using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using StudentApi.Data;
using StudentApi.Models;

namespace StudentApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductionOrderController : ControllerBase
    {
        private readonly SapDbConnection _sapDb;
        private readonly ILogger<ProductionOrderController> _logger;

        public ProductionOrderController(SapDbConnection sapDb, ILogger<ProductionOrderController> logger)
        {
            _sapDb = sapDb;
            _logger = logger;
        }

        // GET: /api/ProductionOrder/open
        [HttpGet("open")]
        public async Task<IActionResult> GetOpenProductionOrders(
            [FromQuery] string? itemCode = null,
            [FromQuery] string? status = null)
        {
            try
            {
                _logger.LogInformation("Fetching open production orders from SAP B1.");

                var orders = new List<ProductionOrderResponse>();

                var query = @"
                    SELECT 
                        DocEntry, 
                        DocNum, 
                        ItemCode, 
                        ProdName, 
                        CASE Status 
                            WHEN 'P' THEN 'Planned' 
                            WHEN 'R' THEN 'Released' 
                            WHEN 'L' THEN 'Closed' 
                            WHEN 'C' THEN 'Cancelled' 
                            ELSE Status 
                        END AS Status, 
                        CASE Type 
                            WHEN 'S' THEN 'Standard' 
                            WHEN 'P' THEN 'Special' 
                            WHEN 'D' THEN 'Disassembly' 
                            ELSE Type 
                        END AS Type, 
                        ISNULL(PlannedQty, 0) AS PlannedQty, 
                        ISNULL(CmpltQty, 0) AS CmpltQty, 
                        ISNULL(RjctQty, 0) AS RjctQty, 
                        ISNULL(PlannedQty, 0) - ISNULL(CmpltQty, 0) AS OpenQty, 
                        CONVERT(VARCHAR(10), PostDate, 120) AS PostDate, 
                        CONVERT(VARCHAR(10), DueDate, 120) AS DueDate, 
                        CONVERT(VARCHAR(10), StartDate, 120) AS StartDate, 
                        ISNULL(Warehouse, '') AS Warehouse, 
                        ISNULL(Comments, '') AS Comments 
                    FROM OWOR 
                    WHERE 1 = 1";

                if (string.IsNullOrWhiteSpace(status))
                {
                    query += " AND Status IN ('P', 'R')";
                }
                else
                {
                    query += " AND Status = @Status";
                }

                if (!string.IsNullOrWhiteSpace(itemCode))
                {
                    query += " AND ItemCode = @ItemCode";
                }

                query += " ORDER BY DocEntry DESC";

                await using var connection = _sapDb.GetConnection();
                await connection.OpenAsync();

                await using var command = new SqlCommand(query, connection);

                if (!string.IsNullOrWhiteSpace(status))
                {
                    command.Parameters.AddWithValue("@Status", status.Trim().ToUpperInvariant());
                }

                if (!string.IsNullOrWhiteSpace(itemCode))
                {
                    command.Parameters.AddWithValue("@ItemCode", itemCode.Trim());
                }

                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    orders.Add(new ProductionOrderResponse
                    {
                        DocEntry = Convert.ToInt32(reader["DocEntry"]),
                        DocNum = Convert.ToInt32(reader["DocNum"]),
                        ItemCode = Convert.ToString(reader["ItemCode"]) ?? string.Empty,
                        ProductName = Convert.ToString(reader["ProdName"]) ?? string.Empty,
                        Status = Convert.ToString(reader["Status"]) ?? string.Empty,
                        Type = Convert.ToString(reader["Type"]) ?? string.Empty,
                        PlannedQuantity = Convert.ToDecimal(reader["PlannedQty"]),
                        CompletedQuantity = Convert.ToDecimal(reader["CmpltQty"]),
                        RejectedQuantity = Convert.ToDecimal(reader["RjctQty"]),
                        OpenQuantity = Convert.ToDecimal(reader["OpenQty"]),
                        PostDate = Convert.ToString(reader["PostDate"]) ?? string.Empty,
                        DueDate = Convert.ToString(reader["DueDate"]) ?? string.Empty,
                        StartDate = Convert.ToString(reader["StartDate"]) ?? string.Empty,
                        Warehouse = Convert.ToString(reader["Warehouse"]) ?? string.Empty,
                        Comments = Convert.ToString(reader["Comments"]) ?? string.Empty
                    });
                }

                _logger.LogInformation("Successfully retrieved {Count} open production orders.", orders.Count);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching open production orders from SAP B1.");
                return StatusCode(500, new { message = "Error querying SAP B1", error = ex.Message });
            }
        }

        // GET: /api/ProductionOrder/{docEntry}
        [HttpGet("{docEntry:int}")]
        public async Task<IActionResult> GetProductionOrderById(int docEntry)
        {
            try
            {
                _logger.LogInformation("Fetching production order with DocEntry: {DocEntry}", docEntry);

                const string headerQuery = @"
                    SELECT 
                        DocEntry, 
                        DocNum, 
                        ItemCode, 
                        ProdName, 
                        CASE Status 
                            WHEN 'P' THEN 'Planned' 
                            WHEN 'R' THEN 'Released' 
                            WHEN 'L' THEN 'Closed' 
                            WHEN 'C' THEN 'Cancelled' 
                            ELSE Status 
                        END AS Status, 
                        CASE Type 
                            WHEN 'S' THEN 'Standard' 
                            WHEN 'P' THEN 'Special' 
                            WHEN 'D' THEN 'Disassembly' 
                            ELSE Type 
                        END AS Type, 
                        ISNULL(PlannedQty, 0) AS PlannedQty, 
                        ISNULL(CmpltQty, 0) AS CmpltQty, 
                        ISNULL(RjctQty, 0) AS RjctQty, 
                        ISNULL(PlannedQty, 0) - ISNULL(CmpltQty, 0) AS OpenQty, 
                        CONVERT(VARCHAR(10), PostDate, 120) AS PostDate, 
                        CONVERT(VARCHAR(10), DueDate, 120) AS DueDate, 
                        CONVERT(VARCHAR(10), StartDate, 120) AS StartDate, 
                        ISNULL(Warehouse, '') AS Warehouse, 
                        ISNULL(Comments, '') AS Comments 
                    FROM OWOR 
                    WHERE DocEntry = @DocEntry";

                const string linesQuery = @"
                    SELECT 
                        LineNum, 
                        ItemCode, 
                        ItemName, 
                        ISNULL(BaseQty, 0) AS BaseQty, 
                        ISNULL(PlannedQty, 0) AS PlannedQty, 
                        ISNULL(IssuedQty, 0) AS IssuedQty, 
                        ISNULL(PlannedQty, 0) - ISNULL(IssuedQty, 0) AS OpenQty, 
                        ISNULL(wareHouse, '') AS Warehouse, 
                        CASE IssueType 
                            WHEN 'M' THEN 'Manual' 
                            WHEN 'B' THEN 'Backflush' 
                            ELSE IssueType 
                        END AS IssueType 
                    FROM WOR1 
                    WHERE DocEntry = @DocEntry 
                    ORDER BY LineNum ASC";

                await using var connection = _sapDb.GetConnection();
                await connection.OpenAsync();

                ProductionOrderDetailResponse? order = null;

                // 1. Read Header
                await using (var cmdHeader = new SqlCommand(headerQuery, connection))
                {
                    cmdHeader.Parameters.AddWithValue("@DocEntry", docEntry);
                    await using var reader = await cmdHeader.ExecuteReaderAsync();

                    if (await reader.ReadAsync())
                    {
                        order = new ProductionOrderDetailResponse
                        {
                            DocEntry = Convert.ToInt32(reader["DocEntry"]),
                            DocNum = Convert.ToInt32(reader["DocNum"]),
                            ItemCode = Convert.ToString(reader["ItemCode"]) ?? string.Empty,
                            ProductName = Convert.ToString(reader["ProdName"]) ?? string.Empty,
                            Status = Convert.ToString(reader["Status"]) ?? string.Empty,
                            Type = Convert.ToString(reader["Type"]) ?? string.Empty,
                            PlannedQuantity = Convert.ToDecimal(reader["PlannedQty"]),
                            CompletedQuantity = Convert.ToDecimal(reader["CmpltQty"]),
                            RejectedQuantity = Convert.ToDecimal(reader["RjctQty"]),
                            OpenQuantity = Convert.ToDecimal(reader["OpenQty"]),
                            PostDate = Convert.ToString(reader["PostDate"]) ?? string.Empty,
                            DueDate = Convert.ToString(reader["DueDate"]) ?? string.Empty,
                            StartDate = Convert.ToString(reader["StartDate"]) ?? string.Empty,
                            Warehouse = Convert.ToString(reader["Warehouse"]) ?? string.Empty,
                            Comments = Convert.ToString(reader["Comments"]) ?? string.Empty
                        };
                    }
                }

                if (order == null)
                {
                    _logger.LogWarning("Production Order with DocEntry {DocEntry} not found.", docEntry);
                    return NotFound($"Production Order with DocEntry '{docEntry}' not found.");
                }

                // 2. Read Lines (Components)
                await using (var cmdLines = new SqlCommand(linesQuery, connection))
                {
                    cmdLines.Parameters.AddWithValue("@DocEntry", docEntry);
                    await using var reader = await cmdLines.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        order.Lines.Add(new ProductionOrderLineResponse
                        {
                            LineNum = Convert.ToInt32(reader["LineNum"]),
                            ItemCode = Convert.ToString(reader["ItemCode"]) ?? string.Empty,
                            ItemName = Convert.ToString(reader["ItemName"]) ?? string.Empty,
                            BaseQuantity = Convert.ToDecimal(reader["BaseQty"]),
                            PlannedQuantity = Convert.ToDecimal(reader["PlannedQty"]),
                            IssuedQuantity = Convert.ToDecimal(reader["IssuedQty"]),
                            OpenQuantity = Convert.ToDecimal(reader["OpenQty"]),
                            Warehouse = Convert.ToString(reader["Warehouse"]) ?? string.Empty,
                            IssueType = Convert.ToString(reader["IssueType"]) ?? string.Empty
                        });
                    }
                }

                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching production order with DocEntry: {DocEntry}", docEntry);
                return StatusCode(500, new { message = "Error querying SAP B1", error = ex.Message });
            }
        }
    }
}
