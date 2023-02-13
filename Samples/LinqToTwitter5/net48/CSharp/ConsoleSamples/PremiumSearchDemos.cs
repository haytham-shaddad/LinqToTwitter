using System;
using System.Linq;
using System.Threading.Tasks;
using LinqToTwitter;
using System.Collections.Generic;
using System.Diagnostics;

namespace Linq2TwitterDemos_Console
{
    public static class DateTimeExtensions
    {
        public static DateTime UpToMinutes(this DateTime value)
        {
            var result = new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, 0);
            return result;
        }
    }

    public class PremiumSearchDemos
    {
        internal static async Task RunAsync(TwitterContext twitterCtx)
        {
            char key;

            do
            {
                ShowMenu();

                key = Console.ReadKey(true).KeyChar;

                switch (key)
                {
                    case '0':
                        Console.WriteLine("\n\tSearching...\n");
                        await DoSearchAsync(twitterCtx);
                        break;
                    case '1':
                        Console.WriteLine("\n\tSearching...\n");
                        await DoPagedSearchAsync(twitterCtx);
                        break;
                    case 'q':
                    case 'Q':
                        Console.WriteLine("\nReturning...\n");
                        break;
                    default:
                        Console.WriteLine(key + " is unknown");
                        break;
                }

            } while (char.ToUpper(key) != 'Q');
        }

        static void ShowMenu()
        {
            Console.WriteLine("\nSearch Demos - Please select:\n");

            Console.WriteLine("\t 0. Search");
            Console.WriteLine("\t 1. Paged Search");
            Console.WriteLine();
            Console.Write("\t Q. Return to Main menu");
        }
  
        static async Task DoSearchAsync(TwitterContext twitterCtx)
        {
            string environmentName = "TwitterArchive";

            string searchTerm = "\"LINQ to Twitter\" OR Linq2Twitter OR LinqToTwitter OR JoeMayo";
            //searchTerm = "кот (";
            searchTerm = "Amman";

            DateTime instance = DateTime.UtcNow;
            DateTime since = instance.UpToMinutes().AddMonths(-12);
            DateTime until = since.AddMonths(6);

            PremiumSearch searchResponse =
                await
                (from search in twitterCtx.PremiumSearch
                 where search.Type == PremiumSearchType.FullArchive &&
                       search.EnvironmentName == environmentName &&
                       search.Query == searchTerm &&
                       search.Count == 500 &&
                       search.Since == since &&
                       search.Until == until
                 select search)
                .SingleOrDefaultAsync();

            if (searchResponse?.Statuses != null)
                searchResponse.Statuses.ForEach(tweet =>
                    Console.WriteLine(
                        "\n  User: {0} ({1})\n  Tweet: {2}", 
                        tweet.User.ScreenNameResponse,
                        tweet.User.UserIDResponse,
                        tweet.Text ?? tweet.FullText));
            else
                Console.WriteLine("No entries found.");
        }

        static async Task DoPagedSearchAsync(TwitterContext twitterCtx)
        {
            const int MaxSearchEntriesToReturn = 500;
            const int SearchRateLimit = 1000;

            const string environmentName = "TwitterArchive";

            string searchTerm = "\"LINQ to Twitter\" OR Linq2Twitter OR LinqToTwitter OR JoeMayo";
            //searchTerm = "кот (";
            searchTerm = "Amman";

            DateTime instance = DateTime.UtcNow;
            DateTime since = instance.UpToMinutes().AddMinutes(-10);
            DateTime until = since.AddMinutes(9);

            var combinedSearchResults = new List<Status>();

            PremiumSearch searchResponse =
                await
                    (from search in twitterCtx.PremiumSearch
                    where search.Type == PremiumSearchType.FullArchive &&
                          search.EnvironmentName == environmentName &&
                          search.Query == searchTerm &&
                          search.Count == MaxSearchEntriesToReturn &&
                          search.Since == since &&
                          search.Until == until
                    select search)
                    .SingleOrDefaultAsync();

            if (searchResponse != null)
            {
                combinedSearchResults.AddRange(searchResponse.Statuses);
                do
                {
                    string next = searchResponse.NextPage;

                    searchResponse =
                        await
                            (from search in twitterCtx.PremiumSearch
                            where search.Type == PremiumSearchType.FullArchive &&
                                  search.EnvironmentName == environmentName &&
                                  search.Query == searchTerm &&
                                  search.Count == MaxSearchEntriesToReturn &&
                                  search.Since == since &&
                                  search.Until == until &&
                                  search.Page == next
                            select search)
                            .SingleOrDefaultAsync();

                    combinedSearchResults.AddRange(searchResponse.Statuses);
                } while (searchResponse.Statuses.Any() && combinedSearchResults.Count < SearchRateLimit);

                combinedSearchResults.ForEach(tweet =>
                    Console.WriteLine(
                        "\n  User: {0} ({1})\n  Tweet: {2}",
                        tweet.User.ScreenNameResponse,
                        tweet.User.UserIDResponse,
                        tweet.Text ?? tweet.FullText)); 
            }
            else
            {
                Console.WriteLine("No entries found.");
            }
        }
    }
}
