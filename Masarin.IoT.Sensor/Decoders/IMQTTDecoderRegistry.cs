namespace Masarin.IoT.Sensor
{
	interface IMQTTDecoderRegistry
    {
        IMQTTDecoder GetDecoderForNode(string node, string path);
    }
}
