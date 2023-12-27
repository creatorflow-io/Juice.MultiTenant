using Juice.MultiTenant.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Juice.MultiTenant.SelfHost.Controllers
{
    public class ContentController : Controller
    {
        public async Task<IActionResult> IndexAsync([FromServices] TenantContentDbContext context)
        {
            var tenant = HttpContext.RequestServices.GetService<ITenant>();
            var count = await context.TenantContents.CountAsync();
            return Json(new { Message = $"Hello World! {tenant?.Identifier ?? "root"} {count}" });
        }
    }
}
