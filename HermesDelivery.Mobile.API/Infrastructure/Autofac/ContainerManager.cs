using Autofac;
using Autofac.Integration.WebApi;
using AutoMapper;
using CourierAPI.Infrastructure.AutoMapper;
using Serilog;
using System;
using System.Reflection;
using CourierAPI.Infrastructure.Extensions;
using CourierAPI.Infrastructure.Serilog;

namespace CourierAPI.Infrastructure.Autofac
{
    /// <summary>
    /// 
    /// </summary>
    public class ContainerManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IContainer BuildContainer()
        {
            //
            var assembly = Assembly.GetExecutingAssembly();

            //
            var builder = new ContainerBuilder();

            // Scan an assembly for services.
            builder.RegisterAssemblyTypes(assembly).Where(t => t.Name.EndsWith("Service")); 
             
            // Register Serilog. 
            builder.Register<ILogger>((c, p) => SerilogConfigurationManager.CreateLogger()).SingleInstance();

            // Register AutoMapper.
            builder.Register<IMapper>(c => AutoMapperConfigurationManager.CreateMapper()).SingleInstance();

            //
            builder.RegisterApiControllers(assembly);

            //
            var container = builder.Build();
            return container;
        }
    }
}