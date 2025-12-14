using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ServiceDefaults;

public static class ServiceDefaultsHostBuilderExtensions
{
    private const string ServiceNameKey = "ServiceName";

    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks();

        var serviceName = builder.Configuration.GetValue<string>(ServiceNameKey) ?? builder.Environment.ApplicationName;

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(rb => rb.AddService(serviceName ?? builder.Environment.ApplicationName))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporterIfConfigured(builder.Configuration))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporterIfConfigured(builder.Configuration));

        builder.Services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
        });

        return builder;
    }

    private static TracerProviderBuilder AddOtlpExporterIfConfigured(this TracerProviderBuilder builder, IConfiguration configuration)
    {
        var endpoint = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
        if (!string.IsNullOrWhiteSpace(endpoint))
        {
            builder.AddOtlpExporter(options => options.Endpoint = new Uri(endpoint));
        }

        return builder;
    }

    private static MeterProviderBuilder AddOtlpExporterIfConfigured(this MeterProviderBuilder builder, IConfiguration configuration)
    {
        var endpoint = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
        if (!string.IsNullOrWhiteSpace(endpoint))
        {
            builder.AddOtlpExporter(options => options.Endpoint = new Uri(endpoint));
        }

        return builder;
    }
}
