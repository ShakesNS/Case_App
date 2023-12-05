using Case_Service;
using Case_Service.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext,services) =>
    {
        var configuration = hostContext.Configuration;
        
        services.AddDbContext<ServiceDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("sqlConnection")),ServiceLifetime.Singleton);
        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();
