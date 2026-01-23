using ITIExaminationSystem.Controllers;
using ITIExaminationSystem.Models;
using ITIExaminationSystem.Models.ModelView;

using Microsoft.AspNetCore.Mvc;

namespace ITIExamination.Controllers
{
    public class ExamController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ExaminationSystemContext Context;
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult GenerateExam()
        {
            var questions = Context.Questions
            .OrderBy(e => Guid.NewGuid())
            .Take(10)
            .Select(q => new
            {
                id = q.QuestionId,
                type = q.QuestionType,
                text = q.QuestionText,
                options = q.QuestionType == "TF"
                    ? new[] { "True", "False" }
                    : new[] { "Option A", "Option B", "Option C", "Option D" },
                userAnswer = (int?)null,
                isFlagged = false
            })
            .ToList();

            ViewBag.Questions = questions;

            return View("Exam");
        }


    }
}


