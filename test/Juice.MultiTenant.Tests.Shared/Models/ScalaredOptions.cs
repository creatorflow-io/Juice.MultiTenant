
namespace Juice.MultiTenant.Tests.Models
{
    public class ScalaredOptions
    {
#if NET8_0_OR_GREATER
        public required IDictionary<string, object> Dict { get; set; }
#else
        public IDictionary<string, object> Dict { get; set; } = new Dictionary<string, object>();
#endif
    }
}
