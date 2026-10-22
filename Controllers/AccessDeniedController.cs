using Microsoft.AspNetCore.Mvc;

namespace shipping_perf.Controllers
{
    public class AccessDeniedController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
