using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using StudentApi.Data;
using StudentApi.Models;

namespace StudentApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GrpoController : ControllerBase
    {
        private readonly SapDbConnection _sapDb;
        private readonly ILogger<GrpoController> _logger;

        public GrpoController(SapDbConnection sapDb, ILogger<GrpoController> logger)
        {
            _sapDb = sapDb;
            _logger = logger;
        }

        // GET: /api/Grpo/open
        [HttpGet("open")]
        public async Task<IActionResult> GetOpenGrpo([FromQuery] string? vendorCode = null)
        {
            return await FetchGrpoList(onlyOpen: true, vendorCode);
        }

        // GET: /api/Grpo
        [HttpGet]
        public async Task<IActionResult> GetAllGrpo([FromQuery] string? vendorCode = null)
        {
            return await FetchGrpoList(onlyOpen: false, vendorCode);
        }

        // GET: /api/Grpo/{docEntry}
        [HttpGet("{docEntry:int}")]
        public async Task<IActionResult> GetGrpoById(int docEntry)
        {
            try
            {
                _logger.LogInformation("Fetching GRPO with DocEntry: {DocEntry}", docEntry);

                const string headerQuery = @"
                    SELECT 
                        DocEntry, 
                        DocNum, 
                        CardCode, 
                        CardName, 
                        ISNULL(NumAtCard, '') AS NumAtCard, 
                        CONVERT(VARCHAR(10), DocDate, 120) AS DocDate, 
                        CONVERT(VARCHAR(10), DocDueDate, 120) AS DocDueDate, 
                        ISNULL(DocTotal, 0) AS DocTotal, 
                        DocCur, 
                        DocStatus, 
                        ISNULL(Comments, '') AS Comments 
                    FROM OPDN 
                    WHERE DocEntry = @DocEntry";

                const string linesQuery = @"
                    SELECT 
                        LineNum, 
                        BaseEntry, 
                        BaseLine, 
                        ItemCode, 
                        Dscription, 
                        ISNULL(Quantity, 0) AS Quantity, 
                        ISNULL(OpenQty, 0) AS OpenQty, 
                        ISNULL(Price, 0) AS Price, 
                        ISNULL(LineTotal, 0) AS LineTotal, 
                        ISNULL(WhsCode, '') AS WhsCode 
                    FROM PDN1 
                    WHERE DocEntry = @DocEntry 
                    ORDER BY LineNum ASC";

                await using var connection = _sapDb.GetConnection();
                await connection.OpenAsync();

                GrpoDetailResponse? grpo = null;

                // 1. Read Header
                await using (var cmdHeader = new SqlCommand(headerQuery, connection))
                {
                    cmdHeader.Parameters.AddWithValue("@DocEntry", docEntry);
                    await using var reader = await cmdHeader.ExecuteReaderAsync();

                    if (await reader.ReadAsync())
                    {
                        grpo = new GrpoDetailResponse
                        {
                            DocEntry = Convert.ToInt32(reader["DocEntry"]),
                            DocNum = Convert.ToInt32(reader["DocNum"]),
                            VendorCode = Convert.ToString(reader["CardCode"]) ?? string.Empty,
                            VendorName = Convert.ToString(reader["CardName"]) ?? string.Empty,
                            VendorRefNo = Convert.ToString(reader["NumAtCard"]) ?? string.Empty,
                            DocDate = Convert.ToString(reader["DocDate"]) ?? string.Empty,
                            DocDueDate = Convert.ToString(reader["DocDueDate"]) ?? string.Empty,
                            DocTotal = Convert.ToDecimal(reader["DocTotal"]),
                            Currency = Convert.ToString(reader["DocCur"]) ?? string.Empty,
                            DocStatus = Convert.ToString(reader["DocStatus"]) == "O" ? "Open" : "Closed",
                            Comments = Convert.ToString(reader["Comments"]) ?? string.Empty
                        };
                    }
                }

                if (grpo == null)
                {
                    _logger.LogWarning("GRPO with DocEntry {DocEntry} not found.", docEntry);
                    return NotFound($"GRPO with DocEntry '{docEntry}' not found.");
                }

                // 2. Read Lines
                await using (var cmdLines = new SqlCommand(linesQuery, connection))
                {
                    cmdLines.Parameters.AddWithValue("@DocEntry", docEntry);
                    await using var reader = await cmdLines.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        grpo.Lines.Add(new GrpoLineResponse
                        {
                            LineNum = Convert.ToInt32(reader["LineNum"]),
                            BaseDocEntry = reader["BaseEntry"] != DBNull.Value ? Convert.ToInt32(reader["BaseEntry"]) : null,
                            BaseLineNum = reader["BaseLine"] != DBNull.Value ? Convert.ToInt32(reader["BaseLine"]) : null,
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

                return Ok(grpo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching GRPO with DocEntry: {DocEntry}", docEntry);
                return StatusCode(500, new { message = "Error querying SAP B1", error = ex.Message });
            }
        }

        private async Task<IActionResult> FetchGrpoList(bool onlyOpen, string? vendorCode)
        {
            try
            {
                _logger.LogInformation("Fetching GRPO list (onlyOpen={OnlyOpen}) from SAP B1.", onlyOpen);

                var list = new List<GrpoResponse>();

                var query = @"
                    SELECT 
                        DocEntry, 
                        DocNum, 
                        CardCode, 
                        CardName, 
                        ISNULL(NumAtCard, '') AS NumAtCard, 
                        CONVERT(VARCHAR(10), DocDate, 120) AS DocDate, 
                        CONVERT(VARCHAR(10), DocDueDate, 120) AS DocDueDate, 
                        ISNULL(DocTotal, 0) AS DocTotal, 
                        DocCur, 
                        DocStatus, 
                        ISNULL(Comments, '') AS Comments 
                    FROM OPDN 
                    WHERE Canceled = 'N'";

                if (onlyOpen)
                {
                    query += " AND DocStatus = 'O'";
                }

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
                    list.Add(new GrpoResponse
                    {
                        DocEntry = Convert.ToInt32(reader["DocEntry"]),
                        DocNum = Convert.ToInt32(reader["DocNum"]),
                        VendorCode = Convert.ToString(reader["CardCode"]) ?? string.Empty,
                        VendorName = Convert.ToString(reader["CardName"]) ?? string.Empty,
                        VendorRefNo = Convert.ToString(reader["NumAtCard"]) ?? string.Empty,
                        DocDate = Convert.ToString(reader["DocDate"]) ?? string.Empty,
                        DocDueDate = Convert.ToString(reader["DocDueDate"]) ?? string.Empty,
                        DocTotal = Convert.ToDecimal(reader["DocTotal"]),
                        Currency = Convert.ToString(reader["DocCur"]) ?? string.Empty,
                        DocStatus = Convert.ToString(reader["DocStatus"]) == "O" ? "Open" : "Closed",
                        Comments = Convert.ToString(reader["Comments"]) ?? string.Empty
                    });
                }

                _logger.LogInformation("Successfully retrieved {Count} GRPO records.", list.Count);
                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching GRPO list from SAP B1.");
                return StatusCode(500, new { message = "Error querying SAP B1", error = ex.Message });
            }
        }
    }
}
