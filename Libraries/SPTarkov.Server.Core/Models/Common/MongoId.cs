namespace SPTarkov.Server.Core.Models.Common
{
    public readonly struct MongoId : IEquatable<MongoId>
    {
        private readonly string _stringId;

        public MongoId(string id)
        {
            _stringId = string.Intern(id);
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
