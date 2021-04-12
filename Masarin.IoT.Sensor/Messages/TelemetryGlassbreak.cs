namespace Masarin.IoT.Sensor
{
    class TelemetryGlassBreak : IoTHubMessage
    {
        public double GlassBreak { get; }

        public TelemetryGlassBreak(IoTHubMessageOrigin origin, string timestamp, int glassBreak) : base(origin, timestamp, "telemetry.GlassBreak")
        {
            GlassBreak = glassBreak;
        }
    }
}
