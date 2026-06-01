using Application;
using Application.Common.Abstracts;
using Application.Interfaces;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Infrastructure;
using Infrastructure.Persistance;
using Infrastructure.Queue;
using IntegrationPlatform;
using Microsoft.EntityFrameworkCore;
using OperationProcessor;
using Worker.Handlers;
using Worker.Workers;

var builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = (IConfiguration)builder.Configuration
    .AddJsonFile(
        $"appsettings.{builder.Environment.EnvironmentName}.json",
        optional: false,
        reloadOnChange: true
    ).AddEnvironmentVariables();

builder.Services.Configure<ApplicationConfiguration>(options => 
    configuration.GetSection("Application").Bind(options));

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(configuration);
builder.Services.AddWebApiServices(configuration);

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(container =>
{
    container.RegisterType<OperationProcessorService>().As<IOperationProcessor>();
    container.RegisterType<DataSyncHandler>().As<OperationHandler>();
    container.RegisterType<ReportGenerationHandler>().As<OperationHandler>();
    container.RegisterType<WebhookDispatchHandler>().As<OperationHandler>();
});

builder.Services.AddSingleton<IErrorSimulator, ErrorSimulator>();
builder.Services.AddSingleton<IOperationQueue, InMemoryOperationQueue>();
builder.Services.AddSingleton<IHostedService>(sp =>
{
    var queue = sp.GetRequiredService<IOperationQueue>();
    var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
    var parallelism = builder.Configuration.GetValue<int>("Worker:DegreeOfParallelism", 4);
    var timeoutSec = builder.Configuration.GetValue<int>("Worker:OperationTimeoutSeconds", 100);
    return new OperationWorker(queue, scopeFactory, parallelism, TimeSpan.FromSeconds(timeoutSec));
});

var app = builder.Build();

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        c.RoutePrefix = "api/swagger";
    });
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        // context.Database.EnsureCreated();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Migration failed: {ex.Message}");
        throw;
    }
}

app.Run("http://0.0.0.0:5000");
