namespace SPTarkov.Server.Core.Utils;

/// <summary>
///     Deterministic XORShift PRNG that mirrors the client's EFT.PseudoRandom.RandomXORShift, used to replay hideout workout QTE rolls
/// </summary>
public sealed class QteRandomUtil(int x, int y, int z, int w)
{
    /// <summary>
    ///     Decode a Hideout.Seed hex string into the RNG
    /// </summary>
    public static QteRandomUtil FromSeedHex(string? seedHex)
    {
        if (string.IsNullOrEmpty(seedHex))
        {
            return new QteRandomUtil(0, 0, 0, 0);
        }

        var bytes = Convert.FromHexString(seedHex);

        var a = BitConverter.ToInt32(bytes, 0);
        var b = BitConverter.ToInt32(bytes, 4);
        var c = BitConverter.ToInt32(bytes, 8);
        var d = BitConverter.ToInt32(bytes, 12);

        return new QteRandomUtil(d, c, b, a);
    }

    public string ToSeedHex()
    {
        var bytes = new byte[16];

        BitConverter.GetBytes(w).CopyTo(bytes, 0);
        BitConverter.GetBytes(z).CopyTo(bytes, 4);
        BitConverter.GetBytes(y).CopyTo(bytes, 8);
        BitConverter.GetBytes(x).CopyTo(bytes, 12);

        return Convert.ToHexStringLower(bytes);
    }

    private int XorShift()
    {
        var num = (x ^ (x << 11)) & int.MaxValue;
        x = y;
        y = z;
        z = w;
        w = w ^ (w >> 19) ^ (num ^ (num >> 8));

        return w;
    }

    public int Next(int min, int max)
    {
        return XorShift() % (max - min) + min;
    }
}
