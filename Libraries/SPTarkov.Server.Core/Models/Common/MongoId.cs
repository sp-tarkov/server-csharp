using SPTarkov.Server.Core.Extensions;

namespace SPTarkov.Server.Core.Models.Common;

public readonly struct MongoId : IEquatable<MongoId>
{
    private readonly string? _stringId;

    public MongoId(string? id)
    {
        // Handle null strings, various id's are null either by BSG or by our own doing with LINQ
        if (string.IsNullOrEmpty(id))
        {
            _stringId = null;

            return;
        }

        if (id.Length != 24)
        {
            // TODO: Items.json root item has an empty parentId property
            Console.WriteLine($"Critical MongoId error: Incorrect length. id: {id}");
        }

        if (!IsValidMongoId(id))
        {
            Console.WriteLine(
                $"Critical MongoId error: Incorrect format. Must be a hexadecimal [a-f0-9] of 24 characters. id: {id}"
            );
        }

        _stringId = string.Intern(id);
    }

    public MongoId()
    {
        _stringId = Generate();
    }

    /// <summary>
    /// Create a 24 character MongoId
    /// </summary>
    /// <returns>24 character objectId</returns>
    private static string Generate()
    {
        Span<byte> objectId = stackalloc byte[12];

        // 4 bytes: current timestamp (big endian)
        var timestamp = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        objectId[0] = (byte)(timestamp >> 24);
        objectId[1] = (byte)(timestamp >> 16);
        objectId[2] = (byte)(timestamp >> 8);
        objectId[3] = (byte)timestamp;

        // 5 bytes: random machine/process identifier
        Random.Shared.NextBytes(objectId.Slice(4, 5));

        // 3 bytes: random counter fallback (no static state)
        var counter = Random.Shared.Next(0, 0xFFFFFF);
        objectId[9] = (byte)(counter >> 16);
        objectId[10] = (byte)(counter >> 8);
        objectId[11] = (byte)counter;

        // Convert to lowercase hex string (24 chars)
        return Convert.ToHexStringLower(objectId);
    }

    public override string ToString()
    {
        return _stringId ?? string.Empty;
    }

    public bool Equals(MongoId? other)
    {
        if (other is null)
        {
            return false;
        }

        return other.ToString().Equals(ToString(), StringComparison.InvariantCultureIgnoreCase);
    }

    public bool Equals(string? other)
    {
        if (other is null)
        {
            return this == null;
        }

        return other.Equals(ToString(), StringComparison.InvariantCultureIgnoreCase);
    }

    public static bool IsValidMongoId(string stringToCheck)
    {
        return stringToCheck.IsValidMongoId();
    }

    public static implicit operator string(MongoId mongoId)
    {
        return mongoId.ToString();
    }

    public static implicit operator MongoId(string mongoId)
    {
        return new MongoId(mongoId);
    }

    public bool Equals(MongoId other)
    {
        return string.Equals(_stringId, other._stringId, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
        return obj is MongoId other && Equals(other);
    }

    public static bool operator ==(MongoId left, MongoId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(MongoId left, MongoId? right)
    {
        return left.Equals(right);
    }

    public static bool operator ==(MongoId left, MongoId? right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(MongoId left, MongoId right)
    {
        return !left.Equals(right);
    }

    public override int GetHashCode()
    {
        return (_stringId ?? string.Empty).GetHashCode();
    }

    public bool IsEmpty()
    {
        if (string.IsNullOrEmpty(_stringId) || _stringId == "000000000000000000000000")
        {
            return true;
        }

        return false;
    }

    public static MongoId Empty()
    {
        return new MongoId("000000000000000000000000");
    }
}
