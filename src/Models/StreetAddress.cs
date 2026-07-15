using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace PosiTrace.Models
{
    /// <summary>
    /// Represents a street address record stored in the application's database.
    /// </summary>
    public class StreetAddress
    {
        [Key] // Defines the field as the primary key
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Disables auto-generation
        public string Address { get; set; }

        public string GeoCoding { get; set; }

        /// <summary>
        /// Removes apartment/unit/suite identifiers and simple unit ranges from a comma-separated street address.
        /// </summary>
        /// <param name="address">
        /// The full street address string containing comma-separated components (for example:
        /// "123 Main St, Apt 4B, Toronto, ON"). The method expects a non-null string.
        /// </param>
        /// <returns>
        /// A normalized address string with components that contained apartment/unit/suite markers removed.
        /// Remaining components are trimmed and rejoined with ", ". If no components remain, an empty string is returned.
        /// </returns>
        /// <remarks>
        /// - The input is split on commas into tokens. Each token is trimmed.
        /// - Tokens are processed with two regular-expression replacements:
        ///   1) Removes any occurrence of a hyphen followed by digits (pattern "-\d+"), e.g. "123-12 Main St" -> "123 Main St".
        ///   2) Removes any occurrence of "Apt", "Unit", "Suite", or "#" and everything that follows on that token
        ///      (case-insensitive), e.g. "Apt 4B" -> "".
        /// - Empty tokens (after trimming and replacements) are discarded.
        /// - The method preserves the original order of remaining tokens and rejoins them with ", ".
        /// - The method does not validate postal codes, provinces, or other address semantics.
        /// - Passing a null <paramref name="address"/> will result in an exception; callers should validate input if needed.
        /// </remarks>
        /// <example>
        /// Input:  "123 Example St, Apt 4B, Suite 2, Toronto, ON"
        /// Output: "123 Example St, Toronto, ON"
        /// </example>
        public static string RemoveAUS(string address)
        {
            string[] tokens = address.Split(',');
            var ret = new List<string>();
            foreach (var token in tokens)
            {
                var addToken = token.Trim();
                addToken = Regex.Replace(addToken, @"-\d+", "");
                addToken = Regex.Replace(addToken, @"\s*(Apt|Unit|Suite|#).*", "", RegexOptions.IgnoreCase);
                if (!string.IsNullOrEmpty(addToken))
                {
                    ret.Add(addToken);
                }
            }

            return string.Join(", ", ret);
        }

        /// <summary>
        /// Attempts to extract a Canadian postal code from a comma-separated address string.
        /// </summary>
        /// <param name="address">The address string to search for a postal code.</param>
        /// <returns>
        /// The first matched postal code in the address using the Canadian postal code pattern (e.g. "Y1Y 2Z2").
        /// Returns an empty string if no postal code is found.
        /// </returns>
        /// <remarks>
        /// - The method splits the input on commas and examines each token independently.
        /// - Matching uses a regular expression that covers the canonical Canadian postal code form,
        ///   accepting an optional space or hyphen between the third and fourth characters.
        /// - The returned value preserves the matched formatting from the token (case and separator).
        /// - The method returns the first match found.
        /// </remarks>
        public static string PostalCode(string address)
        {
            string pattern = @"[ABCEGHJKLMNPRSTVXY]\d[ABCEGHJKLMNPRSTVWXYZ][ -]?\d[ABCEGHJKLMNPRSTVWXYZ]\d";

            string[] tokens = address.Split(',');
            foreach (var token in tokens)
            {
                MatchCollection matches = Regex.Matches(token.Trim().ToUpper(), pattern, RegexOptions.IgnoreCase);
                if (matches.Count > 0)
                {
                    return matches[0].Value;
                }
            }

            return "";
        }
    }
}