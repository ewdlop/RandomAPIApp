using Aspire.Hosting;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.RandomAPIApp>("Projects.RandomAPIApp");

Enumerable.Range(0, 128).ToList().ForEach((int x) =>
{
    builder.AddProject<Projects.RandomAPIApp_一袋米要扛几楼>("Projects.一袋米要扛几楼")
        .WithArgs(x.ToString());
});

builder.Build().Run();
