using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using SPTarkov.Server.Core.Extensions;

namespace SPTarkov.Server.Core.Models.Common;

/// <summary>
/// Represents a 12-<see cref="byte"/> MongoDB-style ObjectId, consisting of:
/// <list type="bullet">
///   <item><description>4-byte timestamp (seconds since Unix epoch, big-endian)</description></item>
///   <item><description>3-byte machine identifier</description></item>
///   <item><description>2-byte process identifier (big-endian)</description></item>
///   <item><description>3-byte incrementing counter (big-endian)</description></item>
/// </list>
/// </summary>
/// <remarks>
/// <para>
/// This struct stores the ObjectId in two packed fields for efficient memory usage
/// and comparison:
/// <list type="bullet">
///   <item><see cref="_timestampAndMachine"/><description>: First 8 bytes (timestamp + machine ID)</description></item>
///   <item><see cref="_pidAndIncrement"/><description>: Last 4 bytes (process ID + counter)</description></item>
/// </list>
/// </para>
/// <para>
/// The struct is immutable and implements <see cref="IEquatable{MongoId}"/> for fast comparisons.
/// </para>
/// </remarks>
public readonly struct MongoId : IEquatable<MongoId>, IComparable<MongoId>
{
    /// <summary>
    /// The first 8 bytes: 4-byte timestamp + 3-byte machine ID + 1 byte of PID.
    /// </summary>
    private readonly long _timestampAndMachine;

    /// <summary>
    /// The last 4 bytes: remaining 1 byte of PID + 3-byte counter.
    /// </summary>
    private readonly int _pidAndIncrement;

    private static readonly int _machine = BitConverter.ToInt32(RandomNumberGenerator.GetBytes(4), 0) & 0xFFFFFF;
    private static readonly short _pid = (short) Environment.ProcessId;
    private static int _increment = RandomNumberGenerator.GetInt32(0, 0xFFFFFF);

    public bool IsEmpty
    {
        get { return _timestampAndMachine == 0 && _pidAndIncrement == 0; }
    }

    /// <summary>
    /// Initializes a new <see cref="MongoId"/> with a generated value
    /// based on the current time, machine ID, process ID, and counter.
    /// </summary>
    public MongoId()
    {
        var timestamp = (int) DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Span<byte> bytes = stackalloc byte[12];

        // timestamp (4 bytes, big-endian)
        BinaryPrimitives.WriteInt32BigEndian(bytes, timestamp);

        // machine ID (3 bytes)
        bytes[4] = (byte) (_machine >> 16);
        bytes[5] = (byte) (_machine >> 8);
        bytes[6] = (byte) _machine;

        // PID (2 bytes)
        BinaryPrimitives.WriteInt16BigEndian(bytes[7..9], _pid);

        // increment (3 bytes, big-endian)
        var inc = Interlocked.Increment(ref _increment) & 0xFFFFFF;
        bytes[9] = (byte) (inc >> 16);
        bytes[10] = (byte) (inc >> 8);
        bytes[11] = (byte) inc;

        // pack into fields (avoids array allocations later)
        _timestampAndMachine = BitConverter.ToInt64(bytes);
        _pidAndIncrement = BitConverter.ToInt32(bytes[8..]);
    }

    public MongoId(ReadOnlySpan<char> hex)
    {
        if (hex.IsEmpty)
        {
            this = default;
            return;
        }

        if (hex.Length != 24)
        {
            throw new ArgumentException(
                $"ObjectId must be a 24-character hex string, but got \"{hex}\" (length {hex.Length}).",
                nameof(hex)
            );
        }

        Span<byte> bytes = stackalloc byte[12];
        for (var i = 0; i < 12; i++)
        {
            var hi = HexCharToValue(hex[2 * i]);
            var lo = HexCharToValue(hex[(2 * i) + 1]);

            if (hi == -1 || lo == -1)
            {
                throw new FormatException("ObjectId contains invalid hex characters.");
            }

            bytes[i] = (byte) ((hi << 4) | lo);
        }

        _timestampAndMachine = BitConverter.ToInt64(bytes);
        _pidAndIncrement = BitConverter.ToInt32(bytes[8..]);
    }

    public MongoId(ReadOnlySpan<byte> hex)
    {
        if (hex.IsEmpty)
        {
            this = default;
            return;
        }

        if (hex.Length != 24)
        {
            throw new ArgumentException(
                $"ObjectId must be a 24-character hex string, but got \"{Encoding.UTF8.GetString(hex)}\" (length {hex.Length}).",
                nameof(hex)
            );
        }

        Span<byte> bytes = stackalloc byte[12];
        for (var i = 0; i < 12; i++)
        {
            var hi = HexCharToValue((char)hex[2 * i]);
            var lo = HexCharToValue((char)hex[(2 * i) + 1]);

            if (hi == -1 || lo == -1)
            {
                throw new FormatException("ObjectId contains invalid hex characters.");
            }

            bytes[i] = (byte)((hi << 4) | lo);
        }

        _timestampAndMachine = BitConverter.ToInt64(bytes);
        _pidAndIncrement = BitConverter.ToInt32(bytes[8..]);
    }

    public MongoId(string? hex)
        : this(hex.AsSpan()) { }

    /// <summary>
    /// Converts a hexadecimal character into its corresponding integer nibble value.
    /// </summary>
    /// <param name="c">The hex character to evaluate (0-9, a-f, A-F).</param>
    /// <returns>An integer value from 0 to 15 if the character is valid hex; otherwise, -1.</returns>
    private static int HexCharToValue(char c)
    {
        return c >= '0' && c <= '9' ? c - '0'
            : c >= 'a' && c <= 'f' ? c - 'a' + 10
            : c >= 'A' && c <= 'F' ? c - 'A' + 10
            : -1;
    }

    /// <summary>
    /// Converts an integer nibble value into its corresponding lowercase hexadecimal character representation.
    /// </summary>
    /// <param name="value">The nibble value to convert (0-15).</param>
    /// <returns>A lowercase hexadecimal character representing the specified value.</returns>
    private static char HexValueToChar(int value)
    {
        return (char) (value < 10 ? value + '0' : value - 10 + 'a');
    }

    /// <summary>
    /// Returns the MongoId as a 24-character lowercase hexadecimal string.
    /// </summary>
    public override string ToString()
    {
        if (IsEmpty)
        {
            return string.Empty;
        }

        return string.Create(24, this, static (chars, state) =>
        {
            Span<byte> bytes = stackalloc byte[12];
            BitConverter.TryWriteBytes(bytes, state._timestampAndMachine);
            BitConverter.TryWriteBytes(bytes[8..], state._pidAndIncrement);

            for (var i = 0; i < 12; i++)
            {
                var b = bytes[i];
                chars[i * 2] = HexValueToChar(b >> 4);
                chars[(i * 2) + 1] = HexValueToChar(b & 0x0F);
            }
        });
    }

    /// <summary>
    /// Tries to format the current <see cref="MongoId"/> instance into the provided character span.
    /// </summary>
    /// <param name="destination">The destination span. Must be at least 24 characters long.</param>
    /// <param name="charsWritten">When this method returns, contains the number of characters written.</param>
    /// <returns><see langword="true"/> if the formatting was successful; otherwise, <see langword="false"/>.</returns>
    public bool TryFormat(Span<char> destination, out int charsWritten)
    {
        if (destination.Length < 24)
        {
            charsWritten = 0;
            return false;
        }

        if (IsEmpty)
        {
            charsWritten = 0;
            return true;
        }

        Span<byte> bytes = stackalloc byte[12];
        BitConverter.TryWriteBytes(bytes, _timestampAndMachine);
        BitConverter.TryWriteBytes(bytes[8..], _pidAndIncrement);

        for (var i = 0; i < 12; i++)
        {
            var b = bytes[i];
            destination[i * 2] = HexValueToChar(b >> 4);
            destination[(i * 2) + 1] = HexValueToChar(b & 0x0F);
        }

        charsWritten = 24;
        return true;
    }

    /// <inheritdoc/>
    public bool Equals(MongoId other)
    {
        return _timestampAndMachine == other._timestampAndMachine && _pidAndIncrement == other._pidAndIncrement;
    }

    public bool Equals(string? other)
    {
        if (other == null || other.Length != 24)
        {
            return false;
        }

        Span<byte> bytes = stackalloc byte[12];
        for (var i = 0; i < 12; i++)
        {
            var hi = HexCharToValue(other[2 * i]);
            var lo = HexCharToValue(other[2 * i + 1]);
            if (hi == -1 || lo == -1)
            {
                return false;
            }

            bytes[i] = (byte) ((hi << 4) | lo);
        }

        var a = BitConverter.ToInt64(bytes);
        var b = BitConverter.ToInt32(bytes[8..]);

        return _timestampAndMachine == a && _pidAndIncrement == b;
    }

    /// <summary>
    /// Validates whether the specified string represents a valid MongoDB ObjectId format.
    /// </summary>
    /// <param name="stringToCheck">The string representation of the identifier to validate.</param>
    /// <returns><see langword="true"/> if the string satisfies format constraints; otherwise, <see langword="false"/>.</returns>
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

    public int CompareTo(MongoId other)
    {
        var compare = _timestampAndMachine.CompareTo(other._timestampAndMachine);
        return compare != 0 ? compare : _pidAndIncrement.CompareTo(other._pidAndIncrement);
    }

    /// <inheritdoc/>
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
        return !left.Equals(right);
    }

    public static bool operator ==(MongoId left, MongoId? right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(MongoId left, MongoId right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(_timestampAndMachine, _pidAndIncrement);
    }

    /// <summary>
    /// Returns an empty <see cref="MongoId"/> instance with all internal bits initialized to zero.
    /// </summary>
    /// <returns>A default initialized <see cref="MongoId"/>.</returns>
    public static MongoId Empty()
    {
        return default;
    }
}
