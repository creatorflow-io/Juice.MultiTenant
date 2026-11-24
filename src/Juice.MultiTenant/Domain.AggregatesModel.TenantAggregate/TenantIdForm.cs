namespace Juice.MultiTenant.Domain.AggregatesModel.TenantAggregate
{
    /// <summary>
    /// A tenant's Id is a GUID in its standard form (<c>0193f0aa-0000-7000-8000-0000000000a1</c>), the tenant pattern decision record. Before
    /// it, the REST API issued Ids in a 26-character base32 form (<c>StringIdGenerator.GenerateUniqueId</c>): the same 16 bytes, read here so
    /// they can be rewritten. This is the reading of Juice's <c>TenantIds</c> (Juice.Contracts 10.15), kept here while this service is on an
    /// older Juice.
    /// </summary>
    public static class TenantIdForm
    {
        private const string Base32Chars = "0123456789abcdefghjkmnpqrstvwxyz";

        /// <summary>A new tenant's Id.</summary>
        public static string New() => Guid.NewGuid().ToString("D");

        /// <summary>The standard form of an Id given in any GUID form or in the base32 form; null when it is neither (or the empty GUID).</summary>
        public static string? Standard(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }
            value = value.Trim();
            if (Guid.TryParse(value, out var id) || TryParseBase32(value, out id))
            {
                return id == Guid.Empty ? null : id.ToString("D");
            }
            return null;
        }

        private static bool TryParseBase32(string value, out Guid id)
        {
            id = Guid.Empty;
            if (value.Length != 26)
            {
                return false;
            }
            var bytes = new byte[16];
            var bit = 0;
            foreach (var c in value)
            {
                var digit = Base32Chars.IndexOf(char.ToLowerInvariant(c));
                if (digit < 0)
                {
                    return false;
                }
                for (var i = 4; i >= 0; i--, bit++)
                {
                    var set = (digit >> i & 1) == 1;
                    if (bit >= 128)
                    {
                        if (set)
                        {
                            return false;
                        }
                        continue;
                    }
                    if (set)
                    {
                        bytes[bit / 8] |= (byte)(0x80 >> (bit % 8));
                    }
                }
            }
            id = new Guid(bytes);
            return true;
        }
    }
}
