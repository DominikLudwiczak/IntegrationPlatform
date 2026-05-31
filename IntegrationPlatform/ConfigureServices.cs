using FluentValidation;
using FluentValidation.AspNetCore;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace IntegrationPlatform;

public static class ConfigureServices
{
    public static void AddWebApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers().ConfigureApiBehaviorOptions(x =>
        {
            x.SuppressMapClientErrors = true;
            x.SuppressInferBindingSourcesForParameters = true;
        }).AddJsonOptions(options =>
        {
        });

        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddDistributedMemoryCache();
        services.AddProblemDetails();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Integration Platform API",
                Version = "v1"
            });
            options.EnableAnnotations();
            options.DescribeAllParametersInCamelCase();
            options.MapType<FileContentResult>(() => new OpenApiSchema
            {
                Type = "file",
                Format = "binary",
            });
        });
    }
}