using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using StudentApi.Data;
using StudentApi.Models;

namespace StudentApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseOrderController : ControllerBase
    {
        private readonly SapDbConnection _sapDb;
        private readonly ILogger<PurchaseOrderController> _logger;

        public PurchaseOrderController(SapDbConnection sapDb, ILogger<PurchaseOrderController> logger)
        {
            _sapDb = sapDb;
            _logger = logger;
        }

        // GET: /api/PurchaseOrder/open
        [HttpGet("open")]
        public async Task<IActionResult> GetOpenPurchaseOrders([FromQuery] string? vendorCode = null)
        {
            try
            {
                _logger.LogInformation("Fetching open purchase orders from SAP B1.");

                var orders = new List<PurchaseOrderResponse>();

                string query = @"
                    SELECT 
                        DocEntry, 
                        DocNum, 
                        CardCode, 
                        CardName, 
                        CONVERT(VARCHAR(10), DocDate, 120) AS DocDate, 
                        CONVERT(VARCHAR(10), DocDueDate, 120) AS DocDueDate, 
                        ISNULL(DocTotal, 0) AS DocTotal, 
                        DocCur, 
                        DocStatus, 
                        Comments 
                    FROM OPOR 
                    WHERE DocStatus = 'O' AND Canceled = 'N'";

                if (!string.IsNullOrWhiteSpace(vendorCode))
                {
                    query += " AND CardCode = @VendorCode";
                }

                query += " ORDER BY DocEntry DESC";

                await using var connection = _sapDb.GetConnection();
                await connection.OpenAsync();

                await using var command = new SqlCommand(query, connection);

                if (!string.IsNullOrWhiteSpace(vendorCode))
                {
                    command.Parameters.AddWithValue("@VendorCode", vendorCode.Trim());
                }

                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    orders.Add(new PurchaseOrderResponse
                    {
                        DocEntry = Convert.ToInt32(reader["DocEntry"]),
                        DocNum = Convert.ToInt32(reader["DocNum"]),
                        VendorCode = Convert.ToString(reader["CardCode"]) ?? string.Empty,
                        VendorName = Convert.ToString(reader["CardName"]) ?? string.Empty,
                        DocDate = Convert.ToString(reader["DocDate"]) ?? string.Empty,
                        DocDueDate = Convert.ToString(reader["DocDueDate"]) ?? string.Empty,
                        DocTotal = Convert.ToDecimal(reader["DocTotal"]),
                        Currency = Convert.ToString(reader["DocCur"]) ?? string.Empty,
                        DocStatus = "Open",
                        Comments = Convert.ToString(reader["Comments"]) ?? string.Empty
                    });
                }

                _logger.LogInformation("Successfully retrieved {Count} open purchase orders.", orders.Count);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching open purchase orders from SAP B1.");
                return StatusCode(500, new { message = "Error querying SAP B1", error = ex.Message });
            }
        }

        // GET: /api/PurchaseOrder/{docEntry}
        [HttpGet("{docEntry:int}")]
        public async Task<IActionResult> GetPurchaseOrderById(int docEntry)
        {
            try
            {
                _logger.LogInformation("Fetching purchase order with DocEntry: {DocEntry}", docEntry);

                const string headerQuery = @"
                    SELECT 
                        DocEntry, 
                        DocNum, 
                        CardCode, 
                        CardName, 
                        CONVERT(VARCHAR(10), DocDate, 120) AS DocDate, 
                        CONVERT(VARCHAR(10), DocDueDate, 120) AS DocDueDate, 
                        ISNULL(DocTotal, 0) AS DocTotal, 
                        DocCur, 
                        DocStatus, 
                        Comments 
                    FROM OPOR 
                    WHERE DocEntry = @DocEntry";

                const string linesQuery = @"
                    SELECT 
                        LineNum, 
                        ItemCode, 
                        Dscription, 
                        ISNULL(Quantity, 0) AS Quantity, 
                        ISNULL(OpenQty, 0) AS OpenQty, 
                        ISNULL(Price, 0) AS Price, 
                        ISNULL(LineTotal, 0) AS LineTotal, 
                        WhsCode 
                    FROM POR1 
                    WHERE DocEntry = @DocEntry 
                    ORDER BY LineNum ASC";

                await using var connection = _sapDb.GetConnection();
                await connection.OpenAsync();

                PurchaseOrderDetailResponse? order = null;

                // 1. Read Header
                await using (var cmdHeader = new SqlCommand(headerQuery, connection))
                {
                    cmdHeader.Parameters.AddWithValue("@DocEntry", docEntry);
                    await using var reader = await cmdHeader.ExecuteReaderAsync();

                    if (await reader.ReadAsync())
                    {
                        order = new PurchaseOrderDetailResponse
                        {
                            DocEntry = Convert.ToInt32(reader["DocEntry"]),
                            DocNum = Convert.ToInt32(reader["DocNum"]),
                            VendorCode = Convert.ToString(reader["CardCode"]) ?? string.Empty,
                            VendorName = Convert.ToString(reader["CardName"]) ?? string.Empty,
                            DocDate = Convert.ToString(reader["DocDate"]) ?? string.Empty,
                            DocDueDate = Convert.ToString(reader["DocDueDate"]) ?? string.Empty,
                            DocTotal = Convert.ToDecimal(reader["DocTotal"]),
                            Currency = Convert.ToString(reader["DocCur"]) ?? string.Empty,
                            DocStatus = Convert.ToString(reader["DocStatus"]) == "O" ? "Open" : "Closed",
                            Comments = Convert.ToString(reader["Comments"]) ?? string.Empty
                        };
                    }
                }

                if (order == null)
                {
                    _logger.LogWarning("Purchase Order with DocEntry {DocEntry} not found.", docEntry);
                    return NotFound($"Purchase Order with DocEntry '{docEntry}' not found.");
                }

                // 2. Read Lines
                await using (var cmdLines = new SqlCommand(linesQuery, connection))
                {
                    cmdLines.Parameters.AddWithValue("@DocEntry", docEntry);
                    await using var reader = await cmdLines.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        order.Lines.Add(new PurchaseOrderLineResponse
                        {
                            LineNum = Convert.ToInt32(reader["LineNum"]),
                            ItemCode = Convert.ToString(reader["ItemCode"]) ?? string.Empty,
                            ItemDescription = Convert.ToString(reader["Dscription"]) ?? string.Empty,
                            Quantity = Convert.ToDecimal(reader["Quantity"]),
                            OpenQuantity = Convert.ToDecimal(reader["OpenQty"]),
                            Price = Convert.ToDecimal(reader["Price"]),
                            LineTotal = Convert.ToDecimal(reader["LineTotal"]),
                            WarehouseCode = Convert.ToString(reader["WhsCode"]) ?? string.Empty
                        });
                    }
                }

                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching purchase order with DocEntry: {DocEntry}", docEntry);
                return StatusCode(500, new { message = "Error querying SAP B1", error = ex.Message });
            }
        }
    }
}
