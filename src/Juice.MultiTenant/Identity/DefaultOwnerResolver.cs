using System.Security.Claims;

namespace Juice.MultiTenant.Identity
{
    internal class DefaultOwnerResolver : IOwnerResolver
    {
        public Task<string?> GetOwnerAsync(ClaimsPrincipal principal)
            => Task.FromResult(principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
        public Task<string?> GetOwnerAsync(string userInfo) => Task.FromResult((string?)userInfo);
        public Task<string?> GetOwnerNameAsync(string owner) => Task.FromResult((string?)owner);
    }
}
