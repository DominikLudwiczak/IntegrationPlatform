using Consumer.Common.Interfaces;
using Consumer.Services;
using Consumer.Services.Rest;

var builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = (IConfiguration)builder.Configuration
    .AddJsonFile(
        $"appsettings.{builder.Environment.EnvironmentName}.json",
        optional: false,
        reloadOnChange: true
    ).AddEnvironmentVariables();

builder.Services.Configure<IntegrationPlatformConfiguration>(options => configuration.GetSection("IntegrationPlatform").Bind(options));

// Add services to the container.
builder.Services.AddIntegrationPlatformHttpClients();

builder.Services.AddScoped<ISyncOperationService, SyncOperationRestService>();
builder.Services.AddScoped<IOperationService, OperationRestService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run("http://0.0.0.0:5001");