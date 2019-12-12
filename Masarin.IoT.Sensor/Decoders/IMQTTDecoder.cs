namespace Masarin.IoT.Sensor
{
	interface IMQTTDecoder
    {
        void Decode(string timestamp, string device, string topic, byte[] payload);
    }
}
