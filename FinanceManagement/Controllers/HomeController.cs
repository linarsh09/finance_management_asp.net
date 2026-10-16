////using FinanceManagement.Data;
////using FinanceManagement.Models;
////using Microsoft.AspNetCore.Mvc;
////using Microsoft.EntityFrameworkCore;
////using System.Diagnostics;

////namespace FinanceManagement.Controllers
////{
////    public class HomeController : Controller
////    {
////        public IActionResult Index()
////        {
////            var allUsers = _context.Users.ToList();
////            return View();
////        }

////        public IActionResult Privacy()
////        {
////            return View();
////        }

////        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
////        public IActionResult Error()
////        {
////            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
////        }
////    }
////}
//using FinanceManagement.Data;
//using FinanceManagement.Models;
//using Microsoft.AspNetCore.Mvc;
//using System.Diagnostics;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;

//namespace FinanceManagement.Controllers
//{
//    public class HomeController : Controller
//    {
//        private readonly FinanceDbContext _context;
//        private readonly UserManager<IdentityUser> _userManager;

//        public HomeController(
//            FinanceDbContext context,
//            UserManager<IdentityUser> userManager)
//        {
//            _context = context;
//            _userManager = userManager;
//        }
//        private readonly FinanceDbContext _context;


//        [Authorize]
//        public async Task<IActionResult> Index()
//        {
//            var userId = _userManager.GetUserId(User);

//            var accounts = await _context.Accounts
//                .Where(a => a.UserId == userId)
//                .ToListAsync();

//            ViewBag.AccountCount = accounts.Count;
//            return View();
//        }

//        public IActionResult Privacy()
//        {
//            return View();
//        }

//        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
//        public IActionResult Error()
//        {
//            return View(new ErrorViewModel
//            {
//                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
//            });
//        }
//    }
//}
using FinanceManagement.Data;
using FinanceManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace FinanceManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly FinanceDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(
            FinanceDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var accounts = await _context.Accounts
                .Where(a => a.UserId == userId)
                .ToListAsync();

            ViewBag.AccountCount = accounts.Count;
            return View();
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