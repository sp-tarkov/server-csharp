using System.Collections.ObjectModel;

namespace MongoIdTplGenerator.Utils;

public class QuestOverrides
{
    public static readonly ReadOnlyDictionary<string, string> NameOverridesDictionary = new(
        new Dictionary<string, string>
        {
            // Bear duplicates
            { "5e381b0286f77420e3417a74", "TEXTILE_PART_1_BEAR" },
            { "5e4d4ac186f774264f758336", "TEXTILE_PART_2_BEAR" },
            { "6613f3007f6666d56807c929", "DRIP_OUT_PART_1_BEAR" },
            { "6613f307fca4f2f386029409", "DRIP_OUT_PART_2_BEAR" },
            // Usec duplicates
            { "5e383a6386f77465910ce1f3", "TEXTILE_PART_1_USEC" },
            { "5e4d515e86f77438b2195244", "TEXTILE_PART_2_USEC" },
            { "66151401efb0539ae10875ae", "DRIP_OUT_PART_1_USEC" },
            { "6615141bfda04449120269a7", "DRIP_OUT_PART_2_USEC" },
            // Generic duplicates
            { "6658a15615cbb1b2c6014d5b", "HUSTLE_2" },
            { "6744a9dfef61d56e020b5c4a", "BATTERY_CHANGE_2" },
            { "6745cbee909d2013670a4a55", "THE_PRICE_OF_INDEPENDENCE_2" },
        }
    );
}
