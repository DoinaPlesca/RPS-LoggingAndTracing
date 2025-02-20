// See https://aka.ms/new-console-template for more information

using Monolith;
using OpenTelemetry;
using OpenTelemetry.Trace;
using Serilog;

public class Program
{
    public static void Main()
    {
        //Setup Serilog
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.Seq("http://localhost:5341")
            .CreateLogger();
        
        Log.Information("Starting Rock-Paper-Scissors");
        
        
        // Setup OpenTelemetry for tracing
        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSource("Monolith")
            .AddZipkinExporter(o =>
            {
                o.Endpoint = new Uri("http://localhost:9411/api/v2/spans"); // Zipkin Endpoint
            })
            .Build();

       
        
        
        var game = new Game();
        for (int i = 0; i < 1000; i++)
        {
            game.Start();
        }
        Log.Information("All games finished.");
        Log.CloseAndFlush();
        
    }
}