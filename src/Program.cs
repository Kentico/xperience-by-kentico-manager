using System.Reflection;

using Autofac;

using Xperience.Manager;
using Xperience.Manager.Commands;
using Xperience.Manager.Repositories;
using Xperience.Manager.Services;
using Xperience.Manager.Wizards;

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
