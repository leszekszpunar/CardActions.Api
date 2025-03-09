using System.Reflection;
using CardActions.Application.Common.Behaviors;
using CardActions.Domain;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CardActions.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(cfg => { cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()); });
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // Rejestracja walidatorów z assembly aplikacji
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddDomain();

        return services;
    }
}