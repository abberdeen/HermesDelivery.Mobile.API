using Autofac;
using Autofac.Integration.WebApi;
using AutoMapper;
using CourierAPI.Infrastructure.AutoMapper;
using Serilog;
using System;
using System.Reflection;
using CourierAPI.Infrastructure.Extensions;

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
            builder.RegisterAssemblyTypes(assembly)
                .Where(t => t.Name.EndsWith("Service"));
              //  .WithParameter("courierId", new ApiControllerExtension().GetCourierId());
             
            // Register Serilog.
            var dataDirectoryPath = AppDomain.CurrentDomain.GetData("DataDirectory").ToString();
            builder.Register<ILogger>((c, p) => new LoggerConfiguration()
                .WriteTo.RollingFile(
                    dataDirectoryPath + "/Logs/Log-{Date}.txt")
                .CreateLogger()).SingleInstance();

            // Register AutoMapper.
            builder.Register<IMapper>(c => ConfigurationManager.CreateConfiguration().CreateMapper()).SingleInstance();

            //
            builder.RegisterApiControllers(assembly);

            //
            var container = builder.Build();
            return container;
        }
    }
}