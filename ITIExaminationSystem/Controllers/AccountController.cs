using ITIExaminationSystem.Interfaces;
using ITIExaminationSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace ITIExaminationSystem.Controllers
{
    /// <summary>
    /// Handles login and logout for students.
    /// All business logic is in StudentService — controller stays thin.
    /// </summary>
    public class AccountController : Controller
    {
        private readonly IStudentService _studentService;

        public AccountController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // If already logged in, redirect to dashboard
            if (HttpContext.Session.GetInt32("St_ID") != null)
                return RedirectToAction("Index", "Student");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(User user)
        {
            if (!ModelState.IsValid)
                return View(user);

            int? studentId = await _studentService.AuthenticateAsync(user.Email, user.Password);

            if (studentId == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password. Please try again.");
                return View(user);
            }

            // Store student ID in session
            HttpContext.Session.SetInt32("St_ID", studentId.Value);

            return RedirectToAction("Index", "Student");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
