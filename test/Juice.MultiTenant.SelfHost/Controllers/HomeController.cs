using Microsoft.AspNetCore.Mvc;

namespace Juice.MultiTenant.SelfHost.Controllers
{
    //[Authorize("home")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var tenant = HttpContext.RequestServices.GetService<ITenant>();
            return Json(new { Message = $"Hello World! {User?.Identity?.Name ?? ""} {tenant?.Identifier ?? "root"}" });
        }
    }
}
