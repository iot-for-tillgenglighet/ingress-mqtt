namespace Masarin.IoT.Sensor
{
	class TelemetryTemperature : IoTHubMessage
    {
        public double Temp { get; }

        public TelemetryTemperature(IoTHubMessageOrigin origin, string timestamp, double temperature) : base(origin, timestamp, "telemetry.temperature")
        {
            Temp = temperature;
        }
    }
}
