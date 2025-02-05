using SptCommon.Annotations;

namespace Core.Utils;

[Injectable]
public class CompareUtil
{
    private readonly HashSet<string> typesToCheckAgainst =
    [
        "string",
        "number",
        "boolean",
        "bigint",
        "symbol",
        "undefined",
        "null"
    ];

    /**
     * This function does an object comparison, equivalent to applying reflections
     * and scanning for all possible properties including arrays.
     * @param v1 value 1 to compare
     * @param v2 value 2 to compare
     * @returns true if equal, false if not
     */
    public bool RecursiveCompare(object v1, object v2)
    {
        // TODO: Fix this shit.
        // const typeOfv1 = typeof v1;
        // const typeOfv2 = typeof v2;
        // if (CompareUtil.typesToCheckAgainst.has(typeOfv1)) {
        //     return v1 === v2;
        // }
        // if (typeOfv1 === "object" && typeOfv2 === "object") {
        //     if (Array.isArray(v1)) {
        //         if (!Array.isArray(v2)) {
        //             return false;
        //         }
        //         const arr1 = v1 as Array<any>;
        //         const arr2 = v2 as Array<any>;
        //         if (arr1.length !== arr2.length) {
        //             return false;
        //         }
        //         return arr1.every((vOf1) => arr2.find((vOf2) => this.recursiveCompare(vOf1, vOf2)));
        //     }
        //     for (const propOf1 in v1) {
        //         if (v2[propOf1] === undefined) {
        //             return false;
        //         }
        //         return this.recursiveCompare(v1[propOf1], v2[propOf1]);
        //     }
        // }
        // if (typeOfv1 === typeOfv2) {
        //     return v1 === v2;
        // }
        //
        // return false;

        throw new NotImplementedException();
    }
}
