using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using LinqToTwitter.Common;
using LitJson;

namespace LinqToTwitter
{
    /// <summary>
    /// processes search queries
    /// </summary>
    class PremiumSearchRequestProcessor<T> : IRequestProcessor<T>, IRequestProcessorWantsJson
    {
        /// <summary>
        /// base url for request
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// Case-sensitive environment name
        /// </summary>
        internal string EnvironmentName { get; set; }

        /// <summary>
        /// type of search, included for compatibility
        /// with other APIs
        /// </summary>
        internal PremiumSearchType Type { get; set; }

        /// <summary>
        /// search query
        /// </summary>
        internal string Query { get; set; }

        /// <summary>
        /// number of results for each page
        /// </summary>
        internal int Count { get; set; }

        /// <summary>
        /// Return tweets on or after this date
        /// </summary>
        internal DateTime Since { get; set; }

        /// <summary>
        /// Return tweets before this date
        /// </summary>
        internal DateTime Until { get; set; }

        /// <summary>
        /// for getting a specific page
        /// </summary>
        internal string Page { get; set; }

        /// <summary>
        /// extracts parameters from lambda
        /// </summary>
        /// <param name="lambdaExpression">lambda expression with where clause</param>
        /// <returns>dictionary of parameter name/value pairs</returns>
        public Dictionary<string, string> GetParameters(System.Linq.Expressions.LambdaExpression lambdaExpression)
        {
            var paramFinder =
               new ParameterFinder<PremiumSearch>(
                   lambdaExpression.Body,
                   new List<string> { 
                       "Type",
                       "EnvironmentName",
                       "Query",
                       "Count",
                       "Since",
                       "Until",
                       "Page"
                   });

            return paramFinder.Parameters;
        }

        /// <summary>
        /// builds url based on input parameters
        /// </summary>
        /// <param name="parameters">criteria for url segments and parameters</param>
        /// <returns>URL conforming to Twitter API</returns>
        public Request BuildUrl(Dictionary<string, string> parameters)
        {
            if (parameters.ContainsKey("Type"))
                Type = RequestProcessorHelper.ParseEnum<PremiumSearchType>(parameters["Type"]);
            else
                throw new ArgumentException("Type is required", "Type");

            if (parameters.ContainsKey("EnvironmentName") && !string.IsNullOrWhiteSpace(parameters["EnvironmentName"]))
                EnvironmentName = parameters["EnvironmentName"];
            else
                throw new ArgumentException("EnvironmentName is required", "EnvironmentName");

            return BuildSearchUrlParameters(parameters, $"tweets/search/{(Type == PremiumSearchType.ThirtyDays ? "30day" : "fullarchive")}/{EnvironmentName}.json");
        }

        /// <summary>
        /// appends parameters for Search request
        /// </summary>
        /// <param name="parameters">list of parameters from expression tree</param>
        /// <param name="url">base url</param>
        /// <returns>base url + parameters</returns>
        private Request BuildSearchUrlParameters(Dictionary<string, string> parameters, string url)
        {
            var req = new Request(BaseUrl + url);
            var urlParams = req.RequestParameters;

            if (parameters.ContainsKey("Query") && !string.IsNullOrWhiteSpace(parameters["Query"]))
            {
                Query = parameters["Query"];
                urlParams.Add(new QueryParameter("query", Query));
            }
            else
            {
                throw new ArgumentNullException("Query", "Query filter in where clause is required.");
            }

            if (parameters.ContainsKey("Count"))
            {
                Count = int.Parse(parameters["Count"]);
                urlParams.Add(new QueryParameter("maxResults", Count.ToString(CultureInfo.InvariantCulture)));
            }

            if (parameters.ContainsKey("Since"))
            {
                Since = DateTime.Parse(parameters["Since"]);
                urlParams.Add(new QueryParameter("fromDate", Since.ToString("yyyyMMddHHmm", CultureInfo.InvariantCulture)));
            }

            if (parameters.ContainsKey("Until"))
            {
                Until = DateTime.Parse(parameters["Until"]);
                urlParams.Add(new QueryParameter("toDate",  Until.ToString("yyyyMMddHHmm", CultureInfo.InvariantCulture)));
            }

            if (parameters.ContainsKey("Page"))
            {
                Page = parameters["Page"];
                urlParams.Add(new QueryParameter("next", Page));
            }

            return req;
        }

        /// <summary>
        /// Transforms response from Twitter into List of Search
        /// </summary>
        /// <param name="responseJson">Json response from Twitter</param>
        /// <returns>List of Search</returns>
        public virtual List<T> ProcessResults(string responseJson)
        {
            IEnumerable<PremiumSearch> search;

            if (string.IsNullOrWhiteSpace(responseJson))
            {
                search = new List<PremiumSearch> { new PremiumSearch() };
            }
            else
            {
                var searchResult = JsonSerialize(responseJson);

                search = new List<PremiumSearch> { searchResult };
            }

            return search.OfType<T>().ToList();
        }

        PremiumSearch JsonSerialize(string responseJson)
        {
            JsonData search = JsonMapper.ToObject(responseJson);

            var searchResult = new PremiumSearch
            {
                Type = Type,
                EnvironmentName = EnvironmentName,
                Query = Query,
                Count = Count,
                Since = Since,
                Until = Until,
                Page = Page
            };

            searchResult.Statuses =
                (from JsonData result in search["results"] 
                select new Status(result))
                .ToList();

            searchResult.SearchMetaData = new PremiumSearchMetaData(search.GetValue<JsonData>("requestParameters"));

            searchResult.NextPage = search.GetValue<string>("next");

            return searchResult;
        }
    }
}