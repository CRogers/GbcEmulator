using System;
using System.Linq;
using System.Text.RegularExpressions;
using RomTools;

namespace Assembler
{
    public static partial class Assembler
    {
        private class SectionMetadata
        {
            public static RomInfo GetRomInfo(string[] metadataSection)
            {
                const string rbool = @"\s*=\s*(true|false)";
                const string rbyte = @"\s*=\s*([\dA-F]{1,2})";
                const string rushort = @"\s*=\s*([\dA-F]{1,4})"; // No, I'm quite tall

                var dataRegexs = new[]
                                 {
                                     @"name\s*=\s*([ -~]+)", @"cartType" + rbyte,
                                     @"color" + rbool, @"licenseeCode" + rushort,
                                     @"superGb" + rbool, @"romSize" + rbyte,
                                     @"ramSize" + rbyte, @"japanese" + rbool,
                                     @"oldLicenseeCode" + rbyte
                                 }.ToDictionary(s => s.Split('\\')[0], s => new Regex(s));

                var data = dataRegexs.ToDictionary(kvp => kvp.Key, kvp => "");

                foreach (string s in metadataSection)
                    foreach (var kvp in dataRegexs)
                        if (data[kvp.Key] == "" && kvp.Value.IsMatch(s))
                            data[kvp.Key] = kvp.Value.Match(s).Groups[1].Value;

                var ri = new RomInfo();

                foreach (var kvp in data.Where(kvp => kvp.Value != ""))
                    switch (kvp.Key)
                    {
                        case "name":
                            ri.RomName = kvp.Value;
                            break;

                        case "cartType":
                            ri.CartridgeInfo = new CartridgeInfo(Convert.ToByte(kvp.Value, 16));
                            break;

                        case "color":
                            ri.IsColor = Convert.ToBoolean(kvp.Value);
                            break;

                        case "licenseeCode":
                            ri.LicenseeCode = Convert.ToUInt16(kvp.Value, 16);
                            break;

                        case "superGb":
                            ri.IsSuperGb = Convert.ToBoolean(kvp.Value);
                            break;

                        case "romSize":
                            ri.RomSize = new RomSize(Convert.ToByte(kvp.Value, 16));
                            break;

                        case "ramSize":
                            ri.RamSize = new RamSize(Convert.ToByte(kvp.Value, 16));
                            break;

                        case "japanese":
                            ri.Japanese = Convert.ToBoolean(kvp.Value);
                            break;

                        case "oldLicenseeCode":
                            ri.OldLincenseeCode = Convert.ToByte(kvp.Value);
                            break;
                    }

                return ri;
            }
        }
    }
}
