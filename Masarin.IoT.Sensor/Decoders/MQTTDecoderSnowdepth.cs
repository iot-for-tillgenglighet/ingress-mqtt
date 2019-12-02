
using Masarin.IoT.Sensor.Messages;
using System;

namespace Masarin.IoT.Sensor
{
	class MQTTDecoderSnowdepth : MQTTDecoder
    {
        private const string UplinkPort1Topic = "1/payload";

        private readonly IMessageQueue _messageQueue = null;

        public MQTTDecoderSnowdepth(IMessageQueue messageQueue)
        {
            _messageQueue = messageQueue;
        }

        public override void Decode(string timestamp, string device, string topic, byte[] payload)
        {
            if (topic == UplinkPort1Topic)
            {
                ReadOnlySpan<byte> span = payload;
                /*
                    uint8_t   Battery [%]
                    uint16_t  raw Distance [mm]
                    uint16_t  Angle [deg]
                    unit16_t  Vertical distance [mm] 
                    unit16_t  Snow depth [mm]
                    unit16_t  Laser signal strength [0-400] (lower is better) 
                    uint8_t   Laser sensor status
                    int16_t   Temperature [°C]F
                    unit8_t   Humidity [%]
                    unit32_t  Pressure [Pa]

                    DEVEUI:s
                    1199411787624306471 Stöde 62.4081681,16.5687632
                    1199411787624306472 Matfors 62.348364,17.016056
                    1199411787624306473 Njurunda 62.310744,17.3533887
                    1199411787624306480 Sundsvall 62.392035,17.2843186
                    1199411787624306481 Alnö 62.423001,17.4263873
                    1199411787624306482 Sidsjö 62.37479,17.2680887
                    1199411787624306483 Granloholm 62.4104911,17.264459
                    1199411787624306484 Kovland 62.467477,17.1440723
                    1199411787624306485 Fagerdalsparken 62.381802,17.2817077
                    1199411787624306486 Finsta 62.462363,17.3451197
                */
                double latitude = 1.348364;
                double longitude = 1.016056;

               

  
                if (device == "1199411787624306471")
                {
                    latitude = 62.4081681;
                    longitude = 16.5687632;
                }
                else if (device == "1199411787624306472")
                {
                    latitude = 62.348364;
                    longitude = 17.016056;
                }
                else if (device == "1199411787624306473")
                {
                    latitude = 62.310744;
                    longitude = 17.3533887;
                }
                else if (device == "1199411787624306480")
                {
                    latitude = 62.392035;
                    longitude = 17.2843186;
                }
                else if (device == "1199411787624306481")
                {
                    latitude = 62.423001;
                    longitude = 17.4263873;
                }
                else if (device == "1199411787624306483")
                {
                    latitude = 62.4104911;
                    longitude = 17.264459;
                }
                else if (device == "1199411787624306482")
                {
                    latitude = 62.37479;
                    longitude = 17.2680887;
                }
                else if (device == "1199411787624306484")
                {
                    latitude = 62.467477;
                    longitude = 17.1440723;
                }
                else if (device == "1199411787624306485")
                {
                    latitude = 62.381802;
                    longitude = 17.2817077;
                }
                else if (device == "1199411787624306486")
                {
                    latitude = 62.462363;
                    longitude = 17.3451197;
                }
                


                IoTHubMessageOrigin origin = new IoTHubMessageOrigin(device, latitude, longitude);

                int battery = payload[0];  
                IoTHubMessageOrigin originWoPosition = new IoTHubMessageOrigin(device);
                _messageQueue.PostMessage(new SensorStatusMessage(originWoPosition, timestamp, 1100+battery*5));
                
                int snowdepth = payload[7] << 8 | payload[8];
                IoTHubMessageOrigin originWoDevice = new IoTHubMessageOrigin(latitude, longitude);
                _messageQueue.PostMessage(new TelemetrySnowdepth(originWoDevice, timestamp, snowdepth));

                int temperature = ((payload[12] << 8 | payload[13]) - 100) / 10;
                originWoDevice = new IoTHubMessageOrigin(latitude, longitude);
                _messageQueue.PostMessage(new TelemetryTemperature(originWoDevice, timestamp, temperature));

                int humidity = payload[14];
                originWoDevice = new IoTHubMessageOrigin(latitude, longitude);
                _messageQueue.PostMessage(new TelemetryHumidity(originWoDevice, timestamp, humidity));

                int pressure = (payload[15] << 24 | payload[16] << 16 | payload[17] << 8 | payload[18]);
                originWoDevice = new IoTHubMessageOrigin(latitude, longitude);
                _messageQueue.PostMessage(new TelemetryPressure(originWoDevice, timestamp, pressure));
            }
        }
    }
}
