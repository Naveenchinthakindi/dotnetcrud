using StudentApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Database Connection helpers
builder.Services.AddSingleton<DbConnection>();       // MySQL (Student DB)
builder.Services.AddSingleton<SapDbConnection>();    // SAP B1 SQL Server

var app = builder.Build();

// Verify connections on startup
var db = app.Services.GetRequiredService<DbConnection>();
await db.CheckConnection();

var sapDb = app.Services.GetRequiredService<SapDbConnection>();
await sapDb.CheckConnectionAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Map Controller Routes
app.MapControllers();

app.Run();
