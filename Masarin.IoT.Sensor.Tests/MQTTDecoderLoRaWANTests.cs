using System;
using System.Text;
using Xunit;
using Masarin.IoT.Sensor;
using Moq;
using Fiware;

namespace Masarin.IoT.Sensor.Tests
{
    public class MQTTDecoderLoRaWANTests
    {
        [Fact]
        public void DecodeObject()
        {
            var contextBroker = new Mock<IFiwareContextBroker>();
            var decoder = new MQTTDecoderLoRaWAN(contextBroker.Object);
            var payload = "{\"deviceName\":\"sn-elt-livboj-02\",\"devEUI\":\"a81758fffe04d854\",\"data\":\"Bw4yDQA=\",\"object\":{\"present\":true}}";

            decoder.Decode("2020-08-26T07:11:31Z", "iothub", "out", Encoding.UTF8.GetBytes(payload));

            contextBroker.Verify(foo => foo.PostMessage(It.Is<DeviceMessage>(mo => mo.Id == "urn:ngsi-ld:Device:sn-elt-livboj-02")));
        }
    }    
}
