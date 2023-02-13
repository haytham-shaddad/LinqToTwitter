using System;
using System.Globalization;
using LinqToTwitter.Common;
using LitJson;
using System.Xml.Serialization;

namespace LinqToTwitter
{
    [XmlType(Namespace = "LinqToTwitter")]
    public class PremiumSearchMetaData
    {
        public PremiumSearchMetaData() { }
        public PremiumSearchMetaData(JsonData metaData)
        {
            MaxResults = metaData.GetValue<ulong>("maxResults");
            Since = DateTime.ParseExact(metaData.GetValue<string>("fromDate"), "yyyyMMddHHmm", CultureInfo.InvariantCulture);
            Until = DateTime.ParseExact(metaData.GetValue<string>("toDate"), "yyyyMMddHHmm", CultureInfo.InvariantCulture);
        }

        public ulong MaxResults { get; set; }
        public DateTime Since { get; set; }
        public DateTime Until { get; set; }
    }
}
