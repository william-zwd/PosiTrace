using System;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace PosiTrace
{
    public class StreetAddress
    {
        public string NormalizedAddress { get; set; }
        public string GeoCoding { get; set; }

        public static string Normalize(string address)
        {
            string[] tokens = address.Split(',');
            var ret = new List<string>();
            foreach (var token in tokens) 
            {
                var addToken = token.Trim();
                addToken = Regex.Replace(addToken, @"^\d+-", "");
                addToken = Regex.Replace(addToken, @"\s*(Apt|Unit|Suite|#).*", "", RegexOptions.IgnoreCase);
                if (!string.IsNullOrEmpty(addToken))
                {
                    ret.Add(addToken);
                }
            }

            return string.Join(", ", ret);
        }

        public static string PostCode(string address)
        {
            return "";
        }
    }
}
