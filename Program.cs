using StudentApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register single Database Connection helper
builder.Services.AddSingleton<DbConnection>();

var app = builder.Build();

var db = app.Services.GetRequiredService<DbConnection>();
await db.CheckConnection();

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
