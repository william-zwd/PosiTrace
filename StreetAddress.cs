using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace PosiTrace
{
    public class StreetAddress
    {
        [Key] // Defines the field as the primary key
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Disables auto-generation
        public string Address { get; set; }
        public string GeoCoding { get; set; }

        public static string RemoveAUS(string address)
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
