using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsyncRegexTester.Classes
{
    public class RegexResult
    {
        public int Number { get; }
        public string Line { get; }
        public string[] Groups { get; }

        public RegexResult(int number, string line, IEnumerable<string> groups)
        {
            Number = number;
            Line = line;

            //Groups has always 5 items (column count in view)
            int count = 5 - groups.Count();
            var emptyGroups = Enumerable.Repeat(string.Empty, count >= 1 ? count : 0);
            Groups = groups.Take(5).Concat(emptyGroups).ToArray();
        }
    }
}
