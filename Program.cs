using StudentApi.DAL.Implementations;
using StudentApi.DAL.Interfaces;
using StudentApi.Services.Implementations;
using StudentApi.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database Connection Factory
builder.Services.AddSingleton<MySqlConnectionFactory>();

// DAL
builder.Services.AddScoped<IStudentDataAccess, StudentDataAccess>();

// Services
builder.Services.AddScoped<IStudentService, StudentService>();

var app = builder.Build();

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
