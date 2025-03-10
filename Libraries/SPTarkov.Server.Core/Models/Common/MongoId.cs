namespace SPTarkov.Server.Core.Models.Common
{
    public readonly struct MongoId : IEquatable<MongoId>
    {
        private readonly string _stringId;

        public MongoId(string id)
        {
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
            // Allocate a span directly onto the stack, will dispose whenever we finished running
            // Span is recommended to work with stackalloc + we use stackalloc here because we don't do anything with this afterwards
            Span<byte> objectId = stackalloc byte[12];

            // Time stamp (4 bytes)
            var timestamp = (int) DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            // Convert to big-endian
            objectId[0] = (byte) (timestamp >> 24);
            objectId[1] = (byte) (timestamp >> 16);
            objectId[2] = (byte) (timestamp >> 8);
            objectId[3] = (byte) timestamp;

            // Random value (5 bytes)
            var rand = new Random();
            rand.NextBytes(objectId.Slice(4, 5));

            // Incrementing counter (3 bytes)
            // 24-bit counter
            var counter = rand.Next(0, 16777215);
            objectId[9] = (byte) (counter >> 16);
            objectId[10] = (byte) (counter >> 8);
            objectId[11] = (byte) counter;

            return Convert.ToHexStringLower(objectId);
        }

        public override string ToString()
        {
            return this._stringId ?? string.Empty;
        }

        public bool Equals(MongoId? other)
        {
            if (other is null)
            {
                return other == this;
            }

            return other.ToString().Equals(ToString(), StringComparison.InvariantCultureIgnoreCase);
        }

        public bool Equals(string? other)
        {
            if (other is null)
            {
                return other == this;
            }

            return other.Equals(ToString(), StringComparison.InvariantCultureIgnoreCase);
        }

        public static implicit operator string(MongoId mongoId)
        {
            return mongoId.ToString();
        }


        public bool Equals(MongoId other)
        {
            return _stringId == other._stringId;
        }

        public override bool Equals(object? obj)
        {
            return obj is MongoId other && Equals(other);
        }

        public static bool operator ==(MongoId left, MongoId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MongoId left, MongoId right)
        {
            return !left.Equals(right);
        }

        public override int GetHashCode()
        {
            return _stringId.GetHashCode();
        }

        public MongoId Empty()
        {
            return new MongoId("000000000000000000000000");
        }
    }
}
