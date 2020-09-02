    namespace Fiware
    {
        public class ValueProperty 
        {
            public string Type { get; }
            public string Value { get; }

            public ValueProperty(string value)
            {
                Type = "Property";
                Value = value;
            }
        }
        public class DeviceMessage
        {
            public string Id { get; }
            public string Type { get; }
            public ValueProperty Value { get; }

            public DeviceMessage(string id, string value)
            {
                Id = "urn:ngsi-ld:Device:" + id;
                Type = "Device";
                Value = new ValueProperty(value);
            }
        }
    }
    
