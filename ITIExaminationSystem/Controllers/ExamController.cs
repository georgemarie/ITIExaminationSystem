using ITIExaminationSystem.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ITIExaminationSystem.Controllers
{
    /// <summary>
    /// Manages the full exam lifecycle:
    ///   StartExam → TakeExam → SubmitExam → Result
    ///
    /// New features implemented here:
    ///   - Exam timer (duration passed to view, JS handles countdown)
    ///   - Question randomization (done in ExamService)
    ///   - Prevent re-taking (IsSubmitted check before TakeExam)
    ///   - Auto score calculation (SP_CorrectExam called in ExamService)
    /// </summary>
    public class ExamController : Controller
    {
        private readonly IExamService _examService;

        public ExamController(IExamService examService)
        {
            _examService = examService;
        }

        /// <summary>
        /// Generates a new exam for the given course, enrolls the student, then redirects to TakeExam.
        /// </summary>
        public async Task<IActionResult> StartExam(int courseId)
        {
            int? stId = HttpContext.Session.GetInt32("St_ID");
            if (stId == null) return RedirectToAction("Login", "Account");

            // 1. Generate the exam via stored procedure
            int examId = await _examService.GenerateExamAsync(courseId);

            // 2. Enroll student (sets IsSubmitted = 0, records StartedAt)
            await _examService.EnrollStudentAsync(stId.Value, examId);

            return RedirectToAction("TakeExam", new { examId });
        }

        /// <summary>
        /// Loads the exam questions (randomized) and starts the timer.
        /// Redirects to Result if already submitted (prevents re-taking).
        /// </summary>
        public async Task<IActionResult> TakeExam(int examId)
        {
            int? stId = HttpContext.Session.GetInt32("St_ID");
            if (stId == null) return RedirectToAction("Login", "Account");

            // PREVENT RE-TAKING: If already submitted, go to result page
            bool alreadySubmitted = await _examService.HasStudentSubmittedAsync(stId.Value, examId);
            if (alreadySubmitted)
            {
                TempData["Warning"] = "You have already submitted this exam. Results are shown below.";
                return RedirectToAction("Result", new { examId });
            }

            var viewModel = await _examService.GetExamForStudentAsync(examId);
            return View(viewModel);
        }

        /// <summary>
        /// Receives submitted answers, saves them, corrects the exam, and redirects to Result.
        /// POST-Redirect-GET pattern prevents double submission.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitExam(int examId, IFormCollection form)
        {
            int? stId = HttpContext.Session.GetInt32("St_ID");
            if (stId == null) return RedirectToAction("Login", "Account");

            // Guard: Don't process if already submitted
            bool alreadySubmitted = await _examService.HasStudentSubmittedAsync(stId.Value, examId);
            if (alreadySubmitted)
                return RedirectToAction("Result", new { examId });

            // Save each answer (keys are like "ans_42")
            foreach (var key in form.Keys.Where(k => k.StartsWith("ans_")))
            {
                if (int.TryParse(key.Replace("ans_", ""), out int qId))
                {
                    string answer = form[key]!;
                    await _examService.SaveAnswerAsync(stId.Value, examId, qId, answer);
                }
            }

            // Correct exam, mark submitted, and store result in TempData for redirect
            var result = await _examService.CorrectExamAsync(stId.Value, examId);

            // Store result in TempData so Result page can display it without another DB call
            TempData["Grade"] = result.Grade.ToString();
            TempData["TotalDegree"] = result.TotalDegree.ToString();
            TempData["CourseName"] = result.CourseName;

            return RedirectToAction("Result", new { examId });
        }

        /// <summary>
        /// Displays the final exam result.
        /// Reads from TempData (set by SubmitExam) or re-fetches from DB.
        /// </summary>
        public async Task<IActionResult> Result(int examId)
        {
            int? stId = HttpContext.Session.GetInt32("St_ID");
            if (stId == null) return RedirectToAction("Login", "Account");

            // Try to use TempData first (from post-redirect)
            if (TempData["Grade"] != null &&
                decimal.TryParse(TempData["Grade"]?.ToString(), out decimal grade) &&
                decimal.TryParse(TempData["TotalDegree"]?.ToString(), out decimal total))
            {
                var vm = new ITIExaminationSystem.ViewModels.ExamResultViewModel
                {
                    ExamId = examId,
                    Grade = grade,
                    TotalDegree = total,
                    CourseName = TempData["CourseName"]?.ToString() ?? ""
                };
                return View(vm);
            }

            // Fallback: re-fetch from DB (e.g., direct URL visit after submission)
            var result = await _examService.CorrectExamAsync(stId.Value, examId);
            return View(result);
        }
    }
}
