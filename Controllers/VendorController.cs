using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using StudentApi.Data;
using StudentApi.Models;

namespace StudentApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VendorController : ControllerBase
    {
        private readonly SapDbConnection _sapDb;
        private readonly ILogger<VendorController> _logger;

        public VendorController(SapDbConnection sapDb, ILogger<VendorController> logger)
        {
            _sapDb = sapDb;
            _logger = logger;
        }

        // GET: /api/Vendor
        [HttpGet]
        public async Task<IActionResult> GetAllVendors()
        {
            try
            {
                _logger.LogInformation("Fetching all vendors from SAP B1.");

                var vendors = new List<VendorResponse>();
                const string query = "SELECT CardCode, CardName, Address, City, Country, ISNULL(Balance, 0) AS Balance, FrozenFor FROM OCRD WHERE CardType = 'S'";

                await using var connection = _sapDb.GetConnection();
                await connection.OpenAsync();

                await using var command = new SqlCommand(query, connection);
                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    vendors.Add(new VendorResponse
                    {
                        VendorCode = Convert.ToString(reader["CardCode"]) ?? string.Empty,
                        VendorName = Convert.ToString(reader["CardName"]) ?? string.Empty,
                        Address = Convert.ToString(reader["Address"]) ?? string.Empty,
                        City = Convert.ToString(reader["City"]) ?? string.Empty,
                        Country = Convert.ToString(reader["Country"]) ?? string.Empty,
                        Balance = Convert.ToDecimal(reader["Balance"]),
                        IsActive = Convert.ToString(reader["FrozenFor"]) == "N"
                    });
                }

                _logger.LogInformation("Successfully retrieved {Count} vendors.", vendors.Count);
                return Ok(vendors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all vendors from SAP B1.");
                return StatusCode(500, new { message = "Error querying SAP B1", error = ex.Message });
            }
        }

        // GET: /api/Vendor/{code}
        [HttpGet("{code}")]
        public async Task<IActionResult> GetVendorByCode(string code)
        {
            try
            {
                _logger.LogInformation("Fetching vendor with code: {Code}", code);

                const string query = "SELECT CardCode, CardName, Address, City, Country, ISNULL(Balance, 0) AS Balance, FrozenFor FROM OCRD WHERE CardType = 'S' AND CardCode = @Code";

                await using var connection = _sapDb.GetConnection();
                await connection.OpenAsync();

                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Code", code);

                await using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var vendor = new VendorResponse
                    {
                        VendorCode = Convert.ToString(reader["CardCode"]) ?? string.Empty,
                        VendorName = Convert.ToString(reader["CardName"]) ?? string.Empty,
                        Address = Convert.ToString(reader["Address"]) ?? string.Empty,
                        City = Convert.ToString(reader["City"]) ?? string.Empty,
                        Country = Convert.ToString(reader["Country"]) ?? string.Empty,
                        Balance = Convert.ToDecimal(reader["Balance"]),
                        IsActive = Convert.ToString(reader["FrozenFor"]) == "N"
                    };

                    _logger.LogInformation("Found vendor with code: {Code}", code);
                    return Ok(vendor);
                }

                _logger.LogWarning("Vendor with code {Code} not found.", code);
                return NotFound($"Vendor '{code}' not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching vendor with code: {Code}", code);
                return StatusCode(500, new { message = "Error querying SAP B1", error = ex.Message });
            }
        }
    }
}
