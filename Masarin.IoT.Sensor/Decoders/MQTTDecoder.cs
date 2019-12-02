using System.Text;

namespace Masarin.IoT.Sensor
{
	abstract class MQTTDecoder : IMQTTDecoder
    {

        public MQTTDecoder()
        {
        }

        public static string PayloadToHex(byte[] payload)
        {
            StringBuilder hex = new StringBuilder(payload.Length * 2);
            foreach (byte b in payload)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        abstract public void Decode(string timestamp, string device, string topic, byte[] payload);
    }
}
