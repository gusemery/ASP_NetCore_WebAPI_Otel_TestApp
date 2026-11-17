

using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Net;

namespace ASP_NetCore_WebAPI_Otel_TestApp
{
    using System.Diagnostics;

    public static class Telemetry
    {
        public static readonly ActivitySource Source =
            new("WeatherForecast.Api");
    }
    public class Program
    {
        //  Configure the following variables for your environment
        static string OtelCollectorUrl = "http://192.168.68.95";   //   You are able to use URL or ip address.   Needs to be in the given format.
        static string ServiceNameSpace = "TestCoreApplication";        //   This is the namespace within LM that wants to be built.
        static string ApplicationName = "my-mvc-core-application";      //   This is the actual application name. 
        static string HostName = Dns.GetHostName();

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            builder.Services.AddSwaggerGen();


            //  Format the correct url
            string otelUrl = OtelCollectorUrl + ":4318/v1/traces";

            //Setup the LM needed tags.
            var tags = new Dictionary<string, object>();
            tags.Add("service.namespace", ServiceNameSpace);
            tags.Add("host.name", HostName);


            // Setup automatic OTEL instrumenation
            builder.Services.AddOpenTelemetry().WithTracing(tracing=> tracing
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName: ApplicationName, serviceVersion: "1.0.0").AddAttributes(tags))
                .AddSource("WeatherForecast.Api")
                .AddAspNetCoreInstrumentation()
                .AddOtlpExporter(otlpOptions =>
                {
                    otlpOptions.Endpoint = new Uri(otelUrl);
                    otlpOptions.ExportProcessorType = ExportProcessorType.Batch;
                    otlpOptions.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
                })
                );


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
