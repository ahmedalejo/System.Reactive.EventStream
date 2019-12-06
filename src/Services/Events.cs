using System;

namespace System.Reactive.EventStream
{
    public class ServiceRegisteredEvent : StreamEvent
    {
        public ServiceRegisteredEvent(string serviceName) => this.ServiceName = serviceName;
        public string ServiceName { get; set; }
        public override string ToString() =>
            $"{Timestamp:HH:mm:ss fffffff} - {ID}: '{ServiceName}' Registered";
    }

    public class ServiceStartedEvent : StreamEvent
    {
        public ServiceStartedEvent(string serviceName) => this.ServiceName = serviceName;
        public string ServiceName { get; set; }
        public override string ToString() =>
             $"{Timestamp:HH:mm:ss fffffff} - {ID}: '{ServiceName}' Started";
    }

    public class ServiceStoppedEvent : StreamEvent
    {
        public ServiceStoppedEvent(string serviceName) => this.ServiceName = serviceName;
        public string ServiceName { get; set; }
        public override string ToString() =>
            $"{Timestamp:HH:mm:ss fffffff} - {ID}: '{ServiceName}' Stopped";
    }
}
