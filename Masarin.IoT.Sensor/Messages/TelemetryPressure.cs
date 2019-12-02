namespace Masarin.IoT.Sensor
{
	class TelemetryPressure : IoTHubMessage
    {
        public long Pressure { get; }

        public TelemetryPressure(IoTHubMessageOrigin origin, string timestamp, long pressure) : base(origin, timestamp, "telemetry.pressure")
        {
            Pressure = pressure;
        }
    }
}
