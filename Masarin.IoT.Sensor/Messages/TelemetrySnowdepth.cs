namespace Masarin.IoT.Sensor
{
	public class TelemetrySnowdepth : IoTHubMessage
    {
        public double Depth { get; }

        public TelemetrySnowdepth(IoTHubMessageOrigin origin, string timestamp, double snowdepth) : base(origin, timestamp, "telemetry.snowdepth")
        {
            Depth = snowdepth;
        }
    }
}
