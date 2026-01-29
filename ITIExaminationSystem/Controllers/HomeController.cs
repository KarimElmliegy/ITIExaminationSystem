using System.Diagnostics;
using ITIExaminationSystem.Models;
using ITIExaminationSystem.Models.Login;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ITIExaminationSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ExaminationSystemContext _context;

        public HomeController(
            ILogger<HomeController> logger,
            ExaminationSystemContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index() => View();

        public IActionResult LoginPage() => View("Login");

        public IActionResult CheckLoginUser(string Email, string Password)
        {
            var user = _context.LoginUserDtos
                .FromSqlRaw(
                    "EXEC sp_LoginUser @Email,@Password",
                    new SqlParameter("@Email", Email),
                    new SqlParameter("@Password", Password)
                )
                .AsEnumerable()
                .FirstOrDefault();

            if (user == null)
            {
                ViewBag.Error = "Invalid Email or Password";
                return View("Login");
            }

            // ================= ROLE REDIRECTION =================

            return user.Role switch
            {
                "Instructor" => RedirectToAction("insDiplayStudents", "Instructor"),

                "Admin" => RedirectToAction("DashBoard", "Admin"),

                "Student" => RedirectToAction(
                    "StudentHome",
                    "Student",
                    new { id = user.User_Id }
                ),

                "Branch Manager" when user.Branch_Id.HasValue =>
                    RedirectToAction(
                        "DashBoard",
                        "BranchManager",
                        new { branchId = user.Branch_Id }
                    ),

                "Branch Manager" =>
                    Content("This manager is not assigned to any branch"),

                _ => RedirectToAction("Index", "Home")
            };
        }

        public IActionResult Privacy() => View();

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
