IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);


Enumerable.Range(0, 128).ToList().ForEach((int x) =>
{
    builder.AddProject<Projects.RandomAPIApp_一袋米要扛几楼>(x);
});

builder.Build().Run();
