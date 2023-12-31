﻿using DozorBot.DAL.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DozorBot.DAL.UnitOfWork;

public static class UnitOfWorkServiceCollectionExtensions
{
    public static IServiceCollection AddUnitOfWork<TContext>(this IServiceCollection services)
        where TContext : DbContext
    {
        services.AddScoped<IRepositoryFactory, UnitOfWork<TContext>>();
        services.AddScoped<IUnitOfWork, UnitOfWork<TContext>>();
        services.AddScoped<IUnitOfWork<TContext>, UnitOfWork<TContext>>();

        return services;
    }
}