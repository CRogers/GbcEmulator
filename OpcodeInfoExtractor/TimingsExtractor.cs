using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace OpcodeInfoExtractor
{
    class TimingsExtractor
    {
        public static void ExtractTimings(string opMap, string cbOpMap, string operations)
        {
            // BUG: Doesn't work, because of errors in jsGB

            Regex mapper = new Regex(@"[ \t]*Z80\._ops\.(.*?)[, ]");
            Regex comment = new Regex(@"// [\dA-F]{2,4}");
            int xcount = 0;
            Func<string, Dictionary<string, ushort>> map = s => comment.Replace(mapper.Replace(s, @"$1,"), "")
                                                   .Replace("\n", "").Replace("\r", "").Replace(" ", "")
                                                   .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                   .Select((t, i) => new { t, i })
                                                   .ToDictionary(t => t.t == "XX" ? "XX" + xcount++ : t.t, t => (ushort)t.i);

            var opMapped = map(opMap);
            var cbOpMapped = map(cbOpMap);

            foreach (var b in cbOpMapped)
                opMapped.Add(b.Key, (ushort)(b.Value | 0xCB00));

            var ops = Extract(opMapped, operations);
        }

        private static Dictionary<ushort, int> Extract(Dictionary<string, ushort> map, string operations)
        {
            Regex timeExtractor = new Regex(@"(\w+):.*?Z80._r.m=(\d)");
            return timeExtractor.Matches(operations).Cast<Match>().ToDictionary(m => map[m.Groups[1].Value], m => int.Parse(m.Groups[2].Value));
        }
    }
}
