using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Karambolo.Common;

namespace AspNetSkeleton.Common.Cli
{
    public static class ConsoleUtils
    {
        // https://gist.github.com/huobazi/1039424
        public static string ReadLineMasked()
        {
            var sb = new StringBuilder();
            while (true)
            {
                var keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }

                if (keyInfo.Key == ConsoleKey.Backspace)                    
                {
                    if (sb.Length > 0)
                    {
                        Console.Write("\b\0\b");
                        sb.Length--;
                    }
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    Console.Write('*');
                    sb.Append(keyInfo.KeyChar);
                }
            }

            return sb.ToString();
        }

        // https://stackoverflow.com/questions/298830/split-string-containing-command-line-parameters-into-string-in-c-sharp
        public static IEnumerable<string> SplitCommandLine(string value)
        {
            var withinQuotes = false;
            return value.Split(c =>
            {
                if (c == '"')
                    withinQuotes = !withinQuotes;

                return !withinQuotes && char.IsWhiteSpace(c);
            }, StringSplitOptions.RemoveEmptyEntries)
            .Select(arg => RemoveQuotes(arg, '"').Unescape('"', '"'));

            string RemoveQuotes(string s, char q)
            {
                return
                    s.Length >= 2 && s[0] == q && s[s.Length - 1] == q ?
                    s.Substring(1, s.Length - 2) :
                    s;
            }
        }
    }
}
