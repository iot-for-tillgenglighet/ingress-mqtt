namespace Masarin.IoT.Sensor
{
	class TelemetrySnowdepth : IoTHubMessage
    {
        public double Depth { get; }

        public TelemetrySnowdepth(IoTHubMessageOrigin origin, string timestamp, double snowdepth) : base(origin, timestamp, "telemetry.snowdepth")
        {
            Depth = snowdepth;
        }
    }
}
