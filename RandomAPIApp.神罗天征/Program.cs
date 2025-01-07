
//TODO
//set ports
//run on process and threads
//Docker
using System.Collections.Concurrent;
using System.Data;

Console.Out.WriteLine(args);

CancellationTokenSource? cancellationTokenSource = new CancellationTokenSource();
try
{
    ConcurrentQueue<Task>? completedTasks = new ConcurrentQueue<Task>();

    //schedule tasks on the thread pool
    //Port 0 is reserved for ?? 
    Task[]? asapTasks = Enumerable.Range(0, 2 ^ 16).Select(i => RandomAPIApp.神罗天征.Program.RunAsapAsync(args,$"https://localhost:{i}")).ToArray();

    try
    {
        await Parallel.ForEachAsync(Task.WhenEach(asapTasks), cancellationTokenSource.Token, (task, cancellationToken) =>
        {
            if (cancellationToken.IsCancellationRequested) return ValueTask.FromCanceled(cancellationToken);
            completedTasks.Enqueue(task);
            if (cancellationToken.IsCancellationRequested) return ValueTask.FromCanceled(cancellationToken);
            return ValueTask.CompletedTask;
        });

        completedTasks.SelectMany(task =>
        {
            return task.Exception?.InnerExceptions ?? Enumerable.Empty<Exception>();
        }).ToList().ForEach(exception =>
        {
            Console.Out.WriteLine(exception?.Message);
        });
    }
    catch (TaskCanceledException e)
    {
        Console.Out.WriteLine(e.Message);
    }

}
catch (Exception e)
{
    Console.Out.WriteLine(e.Message);
}
finally
{
    cancellationTokenSource.Cancel();
    cancellationTokenSource.Dispose();
}
//static class Program
//{
    
//}

namespace RandomAPIApp.神罗天征
{
    public static partial class Program
    {
        /// <summary>
        /// Run the application as soon as possible
        /// </summary>
        /// <param name="args"></param>
        /// <param name="url"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task RunAsapAsync(string[] args, string url, CancellationToken cancellationToken = default)
        {
            try
            {
                WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);
                builder.WebHost.UseUrls(url);

                // Add services to the container.
                // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
                builder.Services.AddOpenApi();

                WebApplication? app = builder.Build();

                string?[]? summaries =
                [
                    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
                ];

                const string weatherforecast = "weatherforecast";

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.MapOpenApi();

                    //Natural Language Query


                    //order of magnitude of the temperature of a supernova
                    const double supernovaTemperature = 100_000_000_000d; //Kelvin
                    const double absoluteZero = double.Epsilon; //Kelvin
                    const string superWeatherForecast = "superWeatherForecast";

                    //Report me to NASA
                    //TODO order by temperature using NLinq
                    summaries.Order().ToList().ForEach(summary =>
                    {
                        app.MapGet($"/{superWeatherForecast}/{summary}", () =>
                        {
                            return new SuperWeatherForecast
                            (
                                DateOnly.FromDateTime(DateTime.Now),
                                absoluteZero + Random.Shared.NextDouble() * (supernovaTemperature - absoluteZero),
                                summary
                            );
                        })
                        .WithName(nameof(SuperWeatherForecast));
                    });

                    //Report me to Microsoft to The World Meteorological Organization (MTWMO)
                    WeatherForecast?[]? randomWeatherRandomForecasts = Enumerable.Range(1, Random.Shared.Next(0, summaries.Length)).Select(index =>
                        new WeatherForecast
                        (
                            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                            Random.Shared.Next(-20, 55), //blizzard to heatwave
                            summaries[Random.Shared.Next(summaries.Length)] //random summary
                        ))
                        .ToArray();

                    for (int i = 0; i < randomWeatherRandomForecasts.Length; i++)
                    {
                        app.MapGet($"/{weatherforecast}/{i}", () =>
                        {
                            return summaries;
                        })
                        .WithName($"Get{nameof(randomWeatherRandomForecasts)}{i}");
                    }
                }

                //Report Microsoft to The World Meteorological Organization (WMO)
                WeatherForecast?[]? forecast = Enumerable.Range(1, 5).Select(index =>
                    new WeatherForecast
                    (
                        DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                        Random.Shared.Next(-20, 55),
                        summaries[Random.Shared.Next(summaries.Length)]
                    ))
                    .ToArray();

                for (int i = 0; i < summaries.Length; i++)
                {
                    const string weatherForecast = "weatherForecast";
                    app.MapGet($"/{weatherforecast}/{i}", () =>
                    {
                        return forecast[i];
                    })
                    .WithName($"Get{weatherForecast}{i}");
                }

                app.UseHttpsRedirection();

                return app.RunAsync(cancellationToken);

            }
            catch (AggregateException e)
            {
                return Task.FromException(e);
            }
            catch (Exception e)
            {
                return Task.FromException(e);

            }
        }
    }
    /// <summary>
    /// Template code for a weather forecast
    /// </summary>
    /// <param name="Date"></param>
    /// <param name="TemperatureC"></param>
    /// <param name="Summary"></param>
    internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
    {
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
        public int TemperatureK => TemperatureC + 273;
    }

    /// <summary>
    /// A super weather forecast that includes the temperature in Kelvin
    /// </summary>
    /// <param name="Date"></param>
    /// <param name="Temperature"></param>
    /// <param name="Summary"></param>
    public record SuperWeatherForecast(DateOnly Date, double Temperature, string? Summary)
    {
        public double TemperatureF => 32 + (int)(TemperatureC / 0.5556);
        public double TemperatureC => Temperature + 273.15;
    }
}