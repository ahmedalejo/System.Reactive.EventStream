using System;
using System.Threading;
using System.Threading.Tasks;

namespace System.Reactive.EventStream
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;

    public class ServiceManager : ServiceRunnerBase, IServiceManager
    {
        List<Type> ServicesTypes { get; set; } = new List<Type>();
        List<IService> Services { get; set; } = new List<IService>();
        bool HasRegisteredServices => this.Services.Any() || this.ServicesTypes.Any();

        public ServiceManager() { }

        public override Task Start(CancellationToken token = default)
        {
            if (this.HasRegisteredServices == false)
                return Task.Delay(0);

            if (this.IsRunning)
                return Task.Delay(0);

            this.IsRunning = true;

            this.Services.AddRange(
                from serviceType in this.ServicesTypes
                let service = this.InstantiateService(serviceType)
                where service != null
                select service);

            var initializationTasks =
                from service in this.Services.ToArray()
                select this.StartService(service, token);

            return Task.WhenAny(initializationTasks);
        }

        public override Task Stop()
        {
            if (this.HasRegisteredServices == false)
                return Task.Delay(0);

            var shutdownTasks =
                from service in this.Services.ToArray()
                select this.StopService(service);

            this.IsRunning = false;

            return Task.WhenAny(shutdownTasks);
        }

        public IServiceManager Add<TService>()
            where TService : IService
        {
            this.ServicesTypes.Add(typeof(TService));
            return this;
        }

        public IServiceManager Add(IService service)
        {
            if (service == null)
                return this;

            this.Services.Add(service);
            return this;
        }

        async Task StartService(IService service, CancellationToken token)
        {
            try
            {
                await service.Start(token);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //ex.Log(description: $"Failed Starting {service.Name}");
            }
        }

        IService InstantiateService(Type serviceType)
        {
            try
            {
                var service = Activator.CreateInstance(serviceType) as IService;

                return service;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //ex.Log(description: $"Failed Starting Service of type '{serviceType.Name}'");
                return null;
            }
        }

        async Task StopService(IService service)
        {
            try
            {
                await service.Stop();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //ex.Log(description: $"Failed Stopping {service.Name}");
            }
        }


        public Task Start(string serviceName, CancellationToken token = default)
        {
            return GetService(serviceName)?.Start(token);
        }

        public Task Stop(string serviceName)
        {
            return GetService(serviceName)?.Stop();
        }

        public IService GetService(string serviceName)
        {
            return this.Services.FirstOrDefault(_ => _.Name == serviceName);
        }
    }
}

