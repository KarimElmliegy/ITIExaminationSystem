using ITIExaminationSystem.Models.ModelView;
using ITIExaminationSystem.Controllers;
using ITIExaminationSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace ITIExaminationSystem.Controllers
{
    public class BranchManagerController : Controller
    {

        private readonly ILogger<HomeController> _logger;
        private readonly ExaminationSystemContext context;
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult DashBoard(int BranchManager_BranchID)
        {
            ViewBag.Students = context.Students.Include(s => s.User).Where(s => s.BranchId == BranchManager_BranchID);
            ViewBag.StudentCount = context.Students.Count(s => s.BranchId == BranchManager_BranchID);
            return View("DashBoard");
        }
    }
}
