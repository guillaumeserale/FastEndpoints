﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace FastEndpoints;

internal sealed class ServiceResolver : IServiceResolver
{
    private readonly ConcurrentDictionary<Type, ObjectFactory> _factoryCache = new();
    private readonly IServiceProvider _rootServiceProvider;
    private readonly IHttpContextAccessor _ctxAccessor;

    private readonly bool _isUnitTestMode;

    public ServiceResolver(IServiceProvider provider, IHttpContextAccessor ctxAccessor, bool isUnitTestMode = false)
    {
        //this class is instantiated by either the IOC container in normal mode
        //or by Factory.AddTestServices() method in unit testing mode

        _rootServiceProvider = provider;
        _ctxAccessor = ctxAccessor;
        _isUnitTestMode = isUnitTestMode;
    }

    public object CreateInstance(Type type, IServiceProvider? serviceProvider = null)
    {
        var factory = _factoryCache.GetOrAdd(type, (t) => ActivatorUtilities.CreateFactory(t, Type.EmptyTypes));
        return factory(serviceProvider ?? _ctxAccessor?.HttpContext?.RequestServices ?? _rootServiceProvider, null);
    }

    public object CreateSingleton(Type type)
    {
        return ActivatorUtilities.CreateInstance(_rootServiceProvider, type);
    }

    public IServiceScope CreateScope()
        => _isUnitTestMode
            ? _ctxAccessor.HttpContext?.RequestServices.CreateScope() ?? throw new InvalidOperationException("Please follow documentation to configure unit test environment properly!")
            : _rootServiceProvider.CreateScope();

    public TService Resolve<TService>() where TService : class
        => _ctxAccessor.HttpContext?.RequestServices.GetRequiredService<TService>() ??
           _rootServiceProvider.GetRequiredService<TService>();

    public object Resolve(Type typeOfService)
        => _ctxAccessor.HttpContext?.RequestServices.GetRequiredService(typeOfService) ??
           _rootServiceProvider.GetRequiredService(typeOfService);

    public TService? TryResolve<TService>() where TService : class
        => _ctxAccessor.HttpContext?.RequestServices.GetService<TService>() ??
           _rootServiceProvider.GetService<TService>();

    public object? TryResolve(Type typeOfService)
        => _ctxAccessor.HttpContext?.RequestServices.GetService(typeOfService) ??
           _rootServiceProvider.GetService(typeOfService);
}