using ITIExaminationSystem.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ITIExaminationSystem.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Redirect to student dashboard if logged in, otherwise to login
            if (HttpContext.Session.GetInt32("St_ID") != null)
                return RedirectToAction("Index", "Student");

            return RedirectToAction("Login", "Account");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
