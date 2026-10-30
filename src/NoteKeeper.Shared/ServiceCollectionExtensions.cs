using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NoteKeeper.Shared.Cqrs;
using NoteKeeper.Shared.Cqrs.Commands;
using NoteKeeper.Shared.Cqrs.Queries;

namespace NoteKeeper.Shared;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddCqrs(params Assembly[] assemblies) =>
            services
                .Scan(scan => scan
                    .FromAssemblies(assemblies.Append(Assembly.GetExecutingAssembly()))
                    .AddClasses(
                        classes => classes.AssignableTo(typeof(IQueryHandler<,>)),
                        publicOnly: false)
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
                    .AddClasses(
                        classes => classes.AssignableTo(typeof(ICommandHandler<,>)),
                        publicOnly: false)
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
                    .AddClasses(
                        classes => classes.AssignableTo<IMediator>(),
                        publicOnly: false)
                    .AsImplementedInterfaces()
                    .WithScopedLifetime());
    }
}