using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace LinqToTwitter
{
    /// <summary>
    /// for performing Twitter searches
    /// </summary>
    [XmlType(Namespace = "LinqToTwitter")]
    public class PremiumSearch
    {
        //
        // Input parameters
        //

        /// <summary>
        /// type of search, included for compatibility
        /// with other APIs
        /// </summary>
        public PremiumSearchType Type { get; set; }

        /// <summary>
        /// Case-sensitive environment name
        /// </summary>
        public string EnvironmentName { get; set; }

        /// <summary>
        /// search query
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// number of results for each page
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Return tweets on or after this date
        /// </summary>
        public DateTime Since { get; set; }

        /// <summary>
        /// Return tweets before this date
        /// </summary>
        public DateTime Until { get; set; }

        /// <summary>
        /// Current Page
        /// </summary>
        public string Page { get; set; }

        //
        // Output results
        //

        /// <summary>
        /// Tweet data returned from the search
        /// </summary>
        public List<Status> Statuses { get; set; }

        /// <summary>
        /// Tweet metadata returned from search
        /// </summary>
        public PremiumSearchMetaData SearchMetaData { get; set; }

        /// <summary>
        /// Next page
        /// </summary>
        public string NextPage { get; set; }
    }
}
