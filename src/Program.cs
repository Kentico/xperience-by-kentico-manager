using System.Reflection;

using Autofac;

using Xperience.Xman;
using Xperience.Xman.Commands;
using Xperience.Xman.Repositories;
using Xperience.Xman.Services;
using Xperience.Xman.Wizards;

var builder = new ContainerBuilder();
var assemblies = Assembly.GetExecutingAssembly();

builder.RegisterType<App>();
builder.RegisterAssemblyTypes(assemblies)
    .Where(t => t.IsClass
        && !t.IsAbstract
        && typeof(IRepository).IsAssignableFrom(t))
    .AsImplementedInterfaces()
    .InstancePerLifetimeScope();
builder.RegisterAssemblyTypes(assemblies)
    .Where(t => t.IsClass
        && !t.IsAbstract
        && typeof(ICommand).IsAssignableFrom(t))
    .AsImplementedInterfaces()
    .InstancePerLifetimeScope();
builder.RegisterAssemblyTypes(assemblies)
    .Where(t => t.IsClass
        && !t.IsAbstract
        && typeof(IService).IsAssignableFrom(t))
    .AsImplementedInterfaces()
    .InstancePerLifetimeScope();
builder.RegisterAssemblyTypes(assemblies)
    .AsClosedTypesOf(typeof(IWizard<>));

await builder.Build().Resolve<App>().Run(args);
