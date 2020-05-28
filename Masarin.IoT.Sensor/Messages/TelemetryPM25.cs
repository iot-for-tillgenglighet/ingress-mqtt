namespace Masarin.IoT.Sensor
{
    class TelemetryPM25 : IoTHubMessage
    {
        public double PM25 { get; }

        public TelemetryPM25(IoTHubMessageOrigin origin, string timestamp, double pm25) : base(origin, timestamp, "telemetry.pm25")
        {
            PM25 = pm25;
        }
    }
}
