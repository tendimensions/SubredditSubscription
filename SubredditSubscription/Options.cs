using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace SubredditSubscription
{
    public class Options
    {
        [Option('a', "action", Required = true, HelpText = "Action required. 'count', 'copy', 'unsubscribe', 'list'")]
        public string Action { get; set; }

        [Option('u', "user", Required = true, HelpText = "Primary username account to use")]
        public string PrimaryUser { get; set; }

        [Option('p', "password", Required = true, HelpText = "Password for the primary user")]
        public string PrimaryPassword { get; set; }

        [Option('t', "copy_user", Required = false, HelpText = "Username to copy subreddits to")]
        public string CopyUser { get; set; }

        [Option('y', "copy_password", Required = false, HelpText = "Password of username copying subreddits to")]
        public string CopyPassword { get; set; }

        [Option('f', "filename", Required = false, HelpText = "Optional settings json file")]
        public string SettingsFile { get; set; }
    }
}
