using System;
using System.IO;
using System.Linq;
using CommandLine;
using Newtonsoft.Json;
using RedditSharp;

namespace RedditCommands
{
    public class Program
    {
        private static Options options;

        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(o =>
            {
                options = o;
                if (!string.IsNullOrEmpty(o.SettingsFile))
                {
                    var settings = GetSettings(o.SettingsFile);
                    SetOptionsFromSettings(settings);
                }
            });

            if (options == null)
            {
                Environment.Exit(0);
            }

            switch (options.Action)
            {
                case "unsubscribe":
                    UnsubscribeAll(options.PrimaryUser, options.PrimaryPassword);
                    return;
                case "copy":
                    CopySubreddits();
                    return;
                case "count":
                    CountSubreddits();
                    return;
                case "list":
                    ListSubreddits();
                    return;
                default:
                    Console.WriteLine($"Unrecognized action in {options.SettingsFile} file. Values should be 'unsubscribe' or 'copy'");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey(true);
                    return;
            }

        }

        private static void ListSubreddits()
        {
            var account = LogIn(options.PrimaryUser, options.PrimaryPassword);
            Console.WriteLine("Subscribed subreddits are: ");
            var subreddits = account.GetSubscribedSubreddits()
                .Select(s => new { s.Name, s.Subscribers })
                .ToListAsync()
                .Result;
            foreach (var subreddit in subreddits)
            {
                Console.WriteLine($"{subreddit.Name} \t\t\t subscriber count: {subreddit.Subscribers}");
            }
        }

        private static RedditSharp.Things.AuthenticatedUser LogIn(string primaryUser, string primaryPassword)
        {
            var webAgentPool = new RefreshTokenWebAgentPool(primaryUser, primaryPassword, null);
            var t = webAgentPool.GetWebAgentAsync(primaryUser).Result;
            var reddit = new Reddit();
            //reddit .Log .LogIn(primaryUser, primaryPassword);
            return reddit.User;
        }

        private static void CountSubreddits()
        {
            var account = LogIn(options.PrimaryUser, options.PrimaryPassword);
            //Console.WriteLine($"Total subscribed subreddits for {options.PrimaryUser} are {account.SubscribedSubreddits.Count()}");
        }

        private static void CopySubreddits()
        {
            var importReddit = new Reddit();
            //importReddit.LogIn(options.PrimaryUser, options.PrimaryPassword);
            var importAccount = importReddit.User;

            var exportReddit = new Reddit();
            //exportReddit.LogIn(options.CopyUser, options.CopyPassword);

            int totalSubscriptions = importAccount.GetSubscribedSubreddits().CountAsync().Result;
            int currentSubscription = 0;

            foreach (string subreddit in importAccount.GetSubscribedSubreddits().ToListAsync().Result.Select(s => s.Name))
            {
                currentSubscription++;
                try
                {
                    exportReddit.GetSubredditAsync(subreddit).Result.SubscribeAsync();
                    Console.WriteLine($"Subscribed {exportReddit.User} to {subreddit} ({currentSubscription} of {totalSubscriptions})");
                }
                catch (Exception ex)
                {

                    Console.WriteLine($"Exception encountered subscribing to {subreddit}. Exception: {ex.Message}");
                    Console.WriteLine("Now continuing...");
                }
            }
        }

        private static void SetOptionsFromSettings(Settings settings)
        {
            options.Action = settings.Action;
            options.PrimaryUser = settings.OriginAccount.User;
            options.PrimaryPassword = settings.OriginAccount.Password;
            options.CopyUser = settings.NewAccount.User;
            options.CopyPassword = settings.NewAccount.Password;
        }

        private static void UnsubscribeAll(string primaryUser, string primaryPassword)
        {
            var unsubscribeReddit = new Reddit();
            //unsubscribeReddit.LogIn(primaryUser, primaryPassword);
            var unsub = unsubscribeReddit.User;
            var total = unsub.GetSubscribedSubreddits().CountAsync().Result;
            int currentCount = 0;
            var unsubscribeAll = false;
            Console.WriteLine($"Logged in as {primaryUser}...");
            foreach (string subreddit in unsub.GetSubscribedSubreddits().ToListAsync().Result.Select(s => s.Name))
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
                }
                else
                {
                    UnsubSubreddit(unsubscribeReddit, total, currentCount, subreddit);
                }
            }
        }

        private static void UnsubSubreddit(Reddit unsubscribeReddit, int total, int currentCount, string subreddit)
        {
            try
            {
                unsubscribeReddit.GetSubredditAsync(subreddit).Result.UnsubscribeAsync();
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
