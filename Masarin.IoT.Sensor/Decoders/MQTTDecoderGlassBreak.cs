
using Masarin.IoT.Sensor.Messages;
using Newtonsoft.Json;
using System;
using System.Buffers.Binary;
using System.Text;

namespace Masarin.IoT.Sensor
{
    public class MQTTDecoderGlassBreak : MQTTDecoder
    {
        class GlassBreakPayloadData
        {
            public string Data { get; set; }
        }

        private readonly IMessageQueue _messageQueue = null;

        public MQTTDecoderGlassBreak(IMessageQueue messageQueue)
        {
            _messageQueue = messageQueue;
        }

        public override void Decode(string timestamp, string device, string topic, byte[] payload)
        {
            string json = Encoding.UTF8.GetString(payload);
            GlassBreakPayloadData data = JsonConvert.DeserializeObject<GlassBreakPayloadData>(json);
            payload = System.Convert.FromBase64String(data.Data);

            /*2020-11-19T14:08:40Z    application     5/device/10a52aaa84c35740/event/up      7b226170706c69636174696f6e4944223a2235222c226170706c69636174696f6e4e616d65223a226578656d70656c2d617070222c226465766963654e616d65223a224d694c6f576173746557617465724233222c22646576455549223a2231306135326161613834633335373430222c227478496e666f223a7b226672657175656e6379223a3836373530303030302c226472223a307d2c22616472223a66616c73652c2266436e74223a32322c2266506f7274223a312c2264617461223a225631773d222c226f626a656374223a7b227061796c6f6164223a225631773d227d7d
{"glassBreak":87.0,"origin":{"device":"GlassBreak_10a52aaa84c35740","latitude":0.348364,"longitude":0.016056},"timestamp":"2020-11-19T14:08:40Z"}
2*/

            string deviceInHex = device;
            device = Int64.Parse(device, System.Globalization.NumberStyles.HexNumber).ToString();

            ReadOnlySpan<byte> span = payload;
            /*
                uint8_t   Status, 0=Heartbeat, >0 = number of sensors reporting break.
                
                DEVEUI:s
                10a52aaa84c35741 
                
            */
            double latitude = 0.348364;
            double longitude = 0.016056;

            
            // TODO: We need to decide on the device names. Should we use the name from the LoRa app server?
            device = "glass_" + deviceInHex;

            IoTHubMessageOrigin origin = new IoTHubMessageOrigin(device, latitude, longitude);

            int glassBreak = payload[0];
            if (glassBreak >= 0) {
                _messageQueue.PostMessage(new TelemetryGlassBreak(origin, timestamp, glassBreak));
            }
        }
    }
}
