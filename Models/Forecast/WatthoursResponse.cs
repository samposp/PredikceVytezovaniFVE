using System.Xml.Serialization;

namespace PredikceVytěžováníFVE.Models.Forecast {
    // using System.Xml.Serialization;
    // XmlSerializer serializer = new XmlSerializer(typeof(Response));
    // using (StringReader reader = new StringReader(xml))
    // {
    //    var test = (Response)serializer.Deserialize(reader);
    // }

    [XmlRoot(ElementName = "data")]
    public class Data {

        [XmlElement(ElementName = "key")]
        public DateTime Key { get; set; }

        [XmlElement(ElementName = "value")]
        public decimal Value { get; set; }
    }

    [XmlRoot(ElementName = "result")]
    public class Result {

        [XmlElement(ElementName = "data")]
        public List<Data> Data { get; set; }
    }

    [XmlRoot(ElementName = "info")]
    public class Info {

        [XmlElement(ElementName = "latitude")]
        public double Latitude { get; set; }

        [XmlElement(ElementName = "longitude")]
        public double Longitude { get; set; }

        [XmlElement(ElementName = "distance")]
        public double Distance { get; set; }

        [XmlElement(ElementName = "place")]
        public string Place { get; set; }

        [XmlElement(ElementName = "timezone")]
        public string Timezone { get; set; }

        [XmlElement(ElementName = "time")]
        public DateTime Time { get; set; }

        [XmlElement(ElementName = "time_utc")]
        public DateTime TimeUtc { get; set; }
    }

    [XmlRoot(ElementName = "ratelimit")]
    public class Ratelimit {

        [XmlElement(ElementName = "zone")]
        public string Zone { get; set; }

        [XmlElement(ElementName = "period")]
        public int Period { get; set; }

        [XmlElement(ElementName = "limit")]
        public int Limit { get; set; }

        [XmlElement(ElementName = "remaining")]
        public int Remaining { get; set; }
    }

    [XmlRoot(ElementName = "message")]
    public class Message {

        [XmlElement(ElementName = "code")]
        public int Code { get; set; }

        [XmlElement(ElementName = "type")]
        public string Type { get; set; }

        [XmlElement(ElementName = "text")]
        public object Text { get; set; }

        [XmlElement(ElementName = "pid")]
        public string Pid { get; set; }

        [XmlElement(ElementName = "info")]
        public Info Info { get; set; }

        [XmlElement(ElementName = "ratelimit")]
        public Ratelimit Ratelimit { get; set; }
    }

    [XmlRoot(ElementName = "response")]
    public class WatthourResponse {

        [XmlElement(ElementName = "result")]
        public Result Result { get; set; }

        [XmlElement(ElementName = "message")]
        public Message Message { get; set; }
    }


}
