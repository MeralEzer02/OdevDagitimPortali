using Microsoft.AspNetCore.Mvc;

namespace HomeworkPortal.UI.Controllers
{
    public class ProfileController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}