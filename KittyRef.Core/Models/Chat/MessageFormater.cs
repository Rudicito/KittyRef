// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace KittyRef.Core.Models.Chat
{
    /// <remarks>
    /// Based on https://github.com/ppy/osu/blob/b7774fe8d16a96690bef65b4f9562e3df393d5e4/osu.Game/Online/Chat/MessageFormatter.cs
    /// </remarks>
    public static class MessageFormatter
    {
        // IMPORTANT: this has to be defined and initialised to a non-zero value BEFORE all usages below.
        // failing to do so will cause the `Regex` constructor invocations in initialisers below to throw.
        private static readonly TimeSpan regex_timeout = TimeSpan.FromMilliseconds(5);

        // [[Performance Points]] -> wiki:Performance Points (https://osu.ppy.sh/wiki/Performance_Points)
        private static readonly Regex wiki_regex = new Regex(@"\[\[(?<text>[^\]]+)\]\]", RegexOptions.None, regex_timeout);

        // (test)[https://osu.ppy.sh/b/1234] -> test (https://osu.ppy.sh/b/1234)
        private static readonly Regex old_link_regex =
            new Regex(@"\((?<text>(((?<=\\)[\(\)])|[^\(\)])*(((?<open>\()(((?<=\\)[\(\)])|[^\(\)])*)+((?<close-open>\))(((?<=\\)[\(\)])|[^\(\)])*)+)*(?(open)(?!)))\)\[(?<url>[a-z]+://[^ ]+)\]",
                RegexOptions.None, regex_timeout);

        // [https://osu.ppy.sh/b/1234 Beatmap [Hard] (poop)] -> Beatmap [hard] (poop) (https://osu.ppy.sh/b/1234)
        private static readonly Regex new_link_regex =
            new Regex(@"\[(?<url>[a-z]+://[^ ]+) (?<text>(((?<=\\)[\[\]])|[^\[\]])*(((?<open>\[)(((?<=\\)[\[\]])|[^\[\]])*)+((?<close-open>\])(((?<=\\)[\[\]])|[^\[\]])*)+)*(?(open)(?!)))\]",
                RegexOptions.None, regex_timeout);

        // [test](https://osu.ppy.sh/b/1234) -> test (https://osu.ppy.sh/b/1234) aka correct markdown format
        private static readonly Regex markdown_link_regex =
            new Regex(
                @"\[(?<text>(((?<=\\)[\[\]])|[^\[\]])*(((?<open>\[)(((?<=\\)[\[\]])|[^\[\]])*)+((?<close-open>\])(((?<=\\)[\[\]])|[^\[\]])*)+)*(?(open)(?!)))\]\((?<url>[a-z]+://[^ ]+)(\s+(?<title>""([^""]|(?<=\\)"")*""))?\)",
                RegexOptions.None, regex_timeout);

        // advanced, RFC-compatible regular expression that matches any possible URL, *but* allows certain invalid characters that are widely used
        // This is in the format (<required>, [optional]):
        //      http[s]://<domain>.<tld>[:port][/path][?query][#fragment]
        private static readonly Regex advanced_link_regex = new Regex(
            // protocol
            @"(?<link>(https?|osu(mp)?):\/\/" +
            // domain + tld
            @"(?<domain>(?:[a-z0-9]\.|[a-z0-9][a-z0-9-]*[a-z0-9]\.)*[a-z0-9-]*[a-z0-9]" +
            // port (optional)
            @"(?::\d+)?)" +
            // path (optional)
            @"(?<path>(?:(?:\/+(?:[a-z0-9$_\.\+!\*\',;:\(\)@&~=-]|%[0-9a-f]{2})*)*" +
            // query (optional)
            @"(?:\?(?:[a-z0-9$_\+!\*\',;:\(\)@&=\/~-]|%[0-9a-f]{2})*)?)?" +
            // fragment (optional)
            @"(?:#(?:[a-z0-9$_\+!\*\',;:\(\)@&=\/~-]|%[0-9a-f]{2})*)?)?)",
            RegexOptions.IgnoreCase, regex_timeout);

        // #osu
        private static readonly Regex channel_regex = new Regex(@"(#[a-zA-Z]+[a-zA-Z0-9]+)", RegexOptions.None, regex_timeout);

        // Unicode emojis
        // private static readonly Regex emoji_regex = new Regex(@"(\uD83D[\uDC00-\uDE4F])", RegexOptions.None, regex_timeout);

        /// <summary>
        /// Used for parsing in contexts where we don't want e.g. normal times of day to be parsed as timestamps (e.g. chat)
        /// Original osu-web regex:
        /// https://github.com/ppy/osu-web/blob/3b1698639244cfdaf0b41c68bfd651ea729ec2e3/resources/js/utils/beatmapset-discussion-helper.ts#L78
        /// </summary>
        /// <example>
        /// 00:00:000 (...) - test
        /// </example>
        public static readonly Regex TIME_REGEX_STRICT = new Regex(@"\b(((?<minutes>\d{2,}):(?<seconds>[0-5]\d)[:.](?<milliseconds>\d{3}))(?<selection>\s\([^)]+\))?)", RegexOptions.Compiled);


        /// <summary>
        /// The root URL for the website, used for chat link matching.
        /// </summary>
        public static string WebsiteRootUrl
        {
            get => websiteRootUrl;
            set => websiteRootUrl = value
                                    .Trim('/') // trim potential trailing slash/
                                    .Split('/').Last(); // only keep domain name, ignoring protocol.
        }

        private static string websiteRootUrl = "osu.ppy.sh";

        public const string OSU_PROTOCOL = "osu://";

        private static void handleMatches(Regex regex, string display, string link, MessageFormatterResult result, int startIndex = 0,
                                          char[]? escapeChars = null)
        {
            int captureOffset = 0;

            MatchCollection matches = regex.Matches(result.Text, startIndex);

            foreach (Match m in matches)
            {
                int index = m.Index - captureOffset;

                string displayText = string.Format(display,
                    m.Groups[0],
                    m.Groups["text"].Value,
                    m.Groups["url"].Value).Trim();

                string linkText = string.Format(link,
                    m.Groups[0],
                    m.Groups["text"].Value,
                    m.Groups["url"].Value).Trim();

                if (displayText.Length == 0 || linkText.Length == 0) continue;

                // Remove backslash escapes in front of the characters provided in escapeChars
                if (escapeChars != null)
                    displayText = escapeChars.Aggregate(displayText, (current, c) => current.Replace($"\\{c}", c.ToString()));

                // Check for overlapping links
                if (!result.Links.Exists(l => l.Overlaps(index, m.Length)))
                {
                    result.Text = result.Text.Remove(index, m.Length).Insert(index, displayText);

                    // since we just changed the line display text, offset any already processed links.
                    result.Links.ForEach(l => l.Index -= l.Index > index ? m.Length - displayText.Length : 0);

                    result.Links.Add(new Link(linkText, index, displayText.Length));

                    // adjust the offset for processing the current matches group.
                    captureOffset += m.Length - displayText.Length;
                }
            }
        }

        private static void handleAdvanced(Regex regex, MessageFormatterResult result, int startIndex = 0)
        {
            foreach (Match m in regex.Matches(result.Text, startIndex))
            {
                int index = m.Index;
                string linkText = m.Groups["link"].Value;
                int indexLength = linkText.Length;

                var link = new Link(linkText, index, indexLength);

                // sometimes an already-processed formatted link can reduce to a simple URL, too
                // (example: [mean example - https://osu.ppy.sh](https://osu.ppy.sh))
                // therefore we need to check if any of the pre-existing links contains the raw one we found
                if (result.Links.All(existingLink => !existingLink.Overlaps(link)))
                    result.Links.Add(link);
            }
        }

        private static MessageFormatterResult format(string toFormat, int startIndex = 0)
        {
            var result = new MessageFormatterResult(toFormat);

            // handle the [link display] format
            handleMatches(new_link_regex, "{1}", "{2}", result, startIndex, escapeChars: new[] { '[', ']' });

            // handle the standard markdown []() format
            handleMatches(markdown_link_regex, "{1}", "{2}", result, startIndex, escapeChars: new[] { '[', ']' });

            // handle the ()[] link format
            handleMatches(old_link_regex, "{1}", "{2}", result, startIndex, escapeChars: new[] { '(', ')' });

            // handle wiki links
            handleMatches(wiki_regex, "{1}", $"https://{WebsiteRootUrl}/wiki/{{1}}", result, startIndex);

            // handle bare links
            handleAdvanced(advanced_link_regex, result, startIndex);

            // handle editor times
            handleMatches(TIME_REGEX_STRICT, "{0}", $@"{OSU_PROTOCOL}edit/{{0}}", result, startIndex);

            // handle channels
            handleMatches(channel_regex, "{0}", $@"{OSU_PROTOCOL}chan/{{0}}", result, startIndex);

            // see: https://github.com/ppy/osu/pull/24190
            // result.Text = Regex.Replace(result.Text, emoji_regex.ToString(), "[emoji]");

            return result;
        }

        public static Message FormatMessage(Message inputMessage)
        {
            var result = format(inputMessage.Content);

            inputMessage.DisplayContent = result.Text;

            // Sometimes, regex matches are not in order
            result.Links.Sort();
            inputMessage.Links = result.Links;
            return inputMessage;
        }

        public static MessageFormatterResult FormatText(string text)
        {
            var result = format(text);

            result.Links.Sort();

            return result;
        }

        public class MessageFormatterResult
        {
            public List<Link> Links = new List<Link>();
            public string Text;
            public string OriginalText;

            public MessageFormatterResult(string text)
            {
                OriginalText = Text = text;
            }
        }
    }

    public class Link : IComparable<Link>
    {
        public string Url;
        public int Index;
        public int Length;

        public Link(string url, int startIndex, int length)
        {
            Url = url;
            Index = startIndex;
            Length = length;
        }

        public bool Overlaps(Link otherLink) => Overlaps(otherLink.Index, otherLink.Length);

        public bool Overlaps(int otherIndex, int otherLength) => Index < otherIndex + otherLength && otherIndex < Index + Length;

        public int CompareTo(Link? otherLink) => Index > otherLink?.Index ? 1 : -1;
    }
}
