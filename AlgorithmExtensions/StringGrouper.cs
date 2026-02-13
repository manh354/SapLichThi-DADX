using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgorithmExtensions
{
    public class StringGrouper
    {
        /// <summary>
        /// Groups strings based on whether a string starts with another smaller string.
        /// </summary>
        /// <param name="strings">List of strings to group.</param>
        /// <returns>A list of groups where each group is a list of strings.</returns>
        public static List<List<string>> GroupStringsStart(IEnumerable<string> strings)
        {
            // Sorting the strings by length ensures smaller strings come first
            var sortedStrings = strings.OrderBy(s => s.Length).ToList();

            // List to store groups
            List<List<string>> groups = new List<List<string>>();

            foreach (var str in sortedStrings)
            {
                // Find if the current string belongs to an existing group
                var group = groups.FirstOrDefault(g => str.StartsWith(g[0]));

                if (group != null)
                {
                    // Add to existing group
                    group.Add(str);
                }
                else
                {
                    // Create a new group
                    groups.Add(new List<string> { str });
                }
            }

            return groups;
        }
    }

}
