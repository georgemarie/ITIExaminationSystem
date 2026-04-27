using ITIExaminationSystem.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ITIExaminationSystem.Controllers
{
    /// <summary>
    /// Serves the student dashboard and profile pages.
    /// </summary>
    public class StudentController : Controller
    {
        private readonly IStudentService _studentService;

        public StudentController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        /// <summary>Student dashboard: available courses + exam history.</summary>
        public async Task<IActionResult> Index()
        {
            int? stId = HttpContext.Session.GetInt32("St_ID");
            if (stId == null) return RedirectToAction("Login", "Account");

            var viewModel = await _studentService.GetDashboardAsync(stId.Value);
            return View(viewModel);
        }
    }
}
