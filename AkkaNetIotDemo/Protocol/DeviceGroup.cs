using System.Collections.Generic;

namespace AkkaNetIotDemo.Protocol
{
    public sealed class RequestAllTemperatures
    {
        public RequestAllTemperatures(long requestId)
        {
            RequestId = requestId;
        }

        public long RequestId { get; }
    }

    public sealed class RespondAllTemperatures
    {
        public RespondAllTemperatures(long requestId, Dictionary<string, ITemperatureReading> temperatures)
        {
            RequestId = requestId;
            Temperatures = temperatures;
        }

        public long RequestId { get; }
        public Dictionary<string, ITemperatureReading> Temperatures { get; }
    }

    public sealed class CollectionTimeout
    {
        public static CollectionTimeout Instance { get; } = new CollectionTimeout();

        private CollectionTimeout()
        {
        }
    }

    public interface ITemperatureReading
    {
    }

    public sealed class Temperature : ITemperatureReading
    {
        public Temperature(double value)
        {
            Value = value;
        }

        public double Value { get; }
    }

    public sealed class TemperatureNotAvailable : ITemperatureReading
    {
        public static TemperatureNotAvailable Instance { get; } = new TemperatureNotAvailable();

        private TemperatureNotAvailable()
        {
        }
    }

    public sealed class DeviceNotAvailable : ITemperatureReading
    {
        public static DeviceNotAvailable Instance { get; } = new DeviceNotAvailable();

        private DeviceNotAvailable()
        {
        }
    }

    public sealed class DeviceTimeOut : ITemperatureReading
    {
        public static DeviceTimeOut Instance { get; } = new DeviceTimeOut();

        private DeviceTimeOut()
        {
        }
    }
}