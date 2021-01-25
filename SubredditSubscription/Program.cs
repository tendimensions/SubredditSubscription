using Newtonsoft.Json;
using RedditSharp;
using System;
using System.IO;
using System.Linq;

namespace SubredditSubscription
{
    public class Program
    {
        public static string SettingsFile = "settings.json";
        public static void Main(string[] args)
        {
            var settings = GetSettings(SettingsFile);
            if ("unsubscribe".Equals(settings.Action))
            {
                UnsubscribeAll(settings.UnsubscribeAllAccount);
                return;
            }
            if (!"copy".Equals(settings.Action))
            {
                Console.WriteLine($"Unrecognized action in {SettingsFile} file. Values should be 'unsubscribe' or 'copy'");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey(true);
                return;
            }
            var importReddit = new Reddit();
            importReddit.LogIn(settings.OriginAccount.User, settings.OriginAccount.Password);
            var importAccount = importReddit.User;

            var exportReddit = new Reddit();
            exportReddit.LogIn(settings.NewAccount.User, settings.NewAccount.Password);

            int totalSubscriptions = importAccount.SubscribedSubreddits.Count();
            int currentSubscription = 0;

            foreach (string subreddit in importAccount.SubscribedSubreddits.Select(s => s.Name))
            {
                currentSubscription++;
                try
                {
                    exportReddit.GetSubreddit(subreddit).Subscribe();
                    Console.WriteLine($"Subscribed {exportReddit.User} to {subreddit} ({currentSubscription} of {totalSubscriptions})");
                }
                catch (Exception ex)
                {

                    Console.WriteLine($"Exception encountered subscribing to {subreddit}. Exception: {ex.Message}");
                    Console.WriteLine("Now continuing...");
                }
            }
        }

        private static void UnsubscribeAll(Credentials creds)
        {
            var unsubscribeReddit = new Reddit();
            unsubscribeReddit.LogIn(creds.User, creds.Password);
            var unsub = unsubscribeReddit.User;
            var total = unsub.SubscribedSubreddits.Count();
            int currentCount = 0;
            var unsubscribeAll = false;
            Console.WriteLine($"Logged in as {creds.User}...");
            foreach (string subreddit in unsub.SubscribedSubreddits.Select(s => s.Name))
            {
                currentCount++;
                if (!unsubscribeAll)
                {
                    Console.WriteLine($"Unsubscribe from {subreddit}? y/n/a");
                    var key = Console.ReadKey(true);
                    switch (key.Key)
                    {
                        case ConsoleKey.Y:
                            UnsubSubreddit(unsubscribeReddit, total, currentCount, subreddit);
                            break;
                        case ConsoleKey.N:
                            break;
                        case ConsoleKey.A:
                            unsubscribeAll = true;
                            break;
                        default:
                            Console.WriteLine("Unrecognized key. Will not unsubscribe and continuing...");
                            continue;
                    }
                } else
                {
                    UnsubSubreddit(unsubscribeReddit, total, currentCount, subreddit);
                }
            }
        }

        private static void UnsubSubreddit(Reddit unsubscribeReddit, int total, int currentCount, string subreddit)
        {
            try
            {
                unsubscribeReddit.GetSubreddit(subreddit).Unsubscribe();
                Console.WriteLine($"Unsubscribed from subreddit {subreddit}. {currentCount} of {total}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception encountered unsubscribing from {subreddit}. Exception: {ex.Message}");
                Console.WriteLine("Continuing...");
            }
        }

        private static Settings GetSettings(string filename)
        {
            using (StreamReader r = new StreamReader(filename))
            {
                string json = r.ReadToEnd();
                return JsonConvert.DeserializeObject<Settings>(json);
            }
        }
    }

    public class Settings
    {
        public string Action { get; set; }
        public Credentials OriginAccount { get; set; }
        public Credentials NewAccount { get; set; }
        public Credentials UnsubscribeAllAccount { get; set; }
    }

    public class Credentials
    {
        public string User { get; set; }
        public string Password { get; set; }
    }
}
