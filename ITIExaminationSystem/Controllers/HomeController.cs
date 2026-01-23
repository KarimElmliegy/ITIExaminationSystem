using System.Diagnostics;
using ITIExaminationSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITIExaminationSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ExaminationSystemContext _context;

        // ✅ Inject DbContext properly
        public HomeController(
            ILogger<HomeController> logger,
            ExaminationSystemContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult LoginPage()
        {
            return View("Login");
        }

        public IActionResult CheckLoginUser(string Email, string Password)
        {
            // ✅ Use injected DbContext
            var user = _context.Users
                .Include(u => u.BranchManagers).Include(ins=>ins.Instructor)
                .FirstOrDefault(u =>
                    u.UserEmail == Email &&
                    u.UserPassword == Password);

            if (user == null)
            {
                ViewBag.Error = "Invalid Email or Password";
                return View("Login");
            }

            // ========= ROLE REDIRECTION =========

            if (user.Role == "Instructor")
            {
                return RedirectToAction("insDiplayStudents", "Instructor");
            }

            if (user.Role == "Admin")
            {
                return RedirectToAction("DashBoard", "Admin");
            }

            if (user.Role == "Student")
            {
                return RedirectToAction("StudentHome", "Student",new { id = user.UserId});
            }

            if (user.Role == "Branch Manager") // ✅ Correct role name
            {
                var branchId = user.BranchManagers
                    .Select(bm => bm.BranchId)
                    .FirstOrDefault();

                if (branchId == null)
                {
                    return Content("This manager is not assigned to any branch");
                }

                return RedirectToAction(
                    "DashBoard",
                    "BranchManager",
                    new { branchId = branchId }
                );
            }

            // Fallback
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
