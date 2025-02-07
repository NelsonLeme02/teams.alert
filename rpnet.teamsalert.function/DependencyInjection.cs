using System;
using Microsoft.Extensions.DependencyInjection;
using rpnet.teamsalert.function.Handlers;

namespace rpnet.teamsalert.function;

public class DependencyInjection
{
    private readonly IServiceProvider _serviceProvider;

    public DependencyInjection()
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<IRequestHandler, RequestHandler>();
    }

    public T Resolve<T>() where T : notnull
    {
        return _serviceProvider.GetRequiredService<T>();
    }
}
