using System;

namespace Masarin.IoT.Sensor
{
	class TelemetryLight : IoTHubMessage
    {
        public double Lux { get; }
        public double UvIndex { get; }

        public TelemetryLight(IoTHubMessageOrigin origin, string timestamp, double lux, double uvIndex) : base(origin, timestamp, "telemetry.light")
        {
            Lux = Math.Round(lux, 2);
            UvIndex = Math.Round(uvIndex, 2);
        }
    }
}
