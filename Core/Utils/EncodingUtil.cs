using System.Text;
using Core.Annotations;

namespace Core.Utils;

[Injectable(InjectionType.Singleton)]
public class EncodingUtil
{
    public string Encode(string value, EncodeType encode)
    {
        return encode switch
        {
            EncodeType.BASE64 => Convert.ToBase64String(Encoding.Default.GetBytes(value)),
            EncodeType.HEX => Convert.ToHexString(Encoding.Default.GetBytes(value)),
            EncodeType.ASCII => Encoding.ASCII.GetString(Encoding.Default.GetBytes(value)),
            EncodeType.UTF8 => Encoding.UTF8.GetString(Encoding.Default.GetBytes(value)),
            _ => throw new ArgumentOutOfRangeException(nameof(encode), encode, null)
        };
    }

    public string Decode(string value, EncodeType encode)
    {
        switch (encode)
        {
            case EncodeType.BASE64:
                return Encoding.UTF8.GetString(Convert.FromBase64String(value));
            case EncodeType.HEX:
                return Encoding.UTF8.GetString(Convert.FromHexString(value));
            case EncodeType.ASCII:
                return Encoding.ASCII.GetString(Encoding.Default.GetBytes(value));
            case EncodeType.UTF8:
                return Encoding.UTF8.GetString(Encoding.Default.GetBytes(value));
            default:
                throw new ArgumentOutOfRangeException(nameof(encode), encode, null);
        }
    }

    public string FromBase64(string value)
    {
        return Decode(value, EncodeType.BASE64);
    }

    public string ToBase64(string value)
    {
        return Encode(value, EncodeType.BASE64);
    }

    public string FromHex(string value)
    {
        return Decode(value, EncodeType.HEX);
    }

    public string ToHex(string value)
    {
        return Encode(value, EncodeType.HEX);
    }
}

public enum EncodeType {
    BASE64,
    HEX,
    ASCII,
    UTF8
}