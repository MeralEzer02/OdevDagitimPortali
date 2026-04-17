using Microsoft.AspNetCore.Mvc;

namespace HomeworkPortal.UI.Controllers
{
    public class AuthController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SetSessionToken([FromBody] string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                HttpContext.Session.SetString("Token", token);
                return Ok();
            }
            return BadRequest();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("Token");
            return RedirectToAction("Login");
        }
    }
}