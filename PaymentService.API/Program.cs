using PaymentService.Application;
using PaymentService.Application.Middleware;
using PaymentService.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Payment Service API",
        Version = "v1",
        Description = "A microservice for processing travel insurance payments",
    });
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowPolicyService", builder =>
    {
        builder.WithOrigins("http://localhost:5001", "https://localhost:7001")
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Payment Service API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseCors("AllowPolicyService");
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

try
{
    await app.Services.InitializeDatabaseAsync();
    Log.Information("Database initialized successfully");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Failed to initialize database");
    throw;
}

Log.Information("Payment Service starting up...");

app.Run();