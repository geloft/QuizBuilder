using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuizBuilder.Data.Entities;
using System.Data;

namespace QuizBuilder.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: /Admin/Search
        public IActionResult Search()
        {
            var users = _userManager.Users.ToList();
            var roles = _roleManager.Roles.ToList();

            ViewBag.Users = users;
            ViewBag.Roles = roles;
            ViewBag.UserManager = _userManager;

            return View();
        }


        // POST: /Admin/Search
        [HttpPost]
        public async Task<IActionResult> Search(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                return RedirectToAction("AssignRole", new { userId = user.Id });
            }

            return View("UserNotFound");
        }

        // GET: /Admin/AssignRole
        public async Task<IActionResult> AssignRole(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                ViewBag.Roles = _roleManager.Roles.ToList();
                ViewBag.CurrentRole = userRoles.FirstOrDefault();
                return View(user);
            }

            return View("UserNotFound");
        }

        // POST: /Admin/AssignRole
        [HttpPost]
        public async Task<IActionResult> AssignRole(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var existingRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, existingRoles);
                await _userManager.AddToRoleAsync(user, roleName);
            }

            return RedirectToAction("Search");
        }
    }
}
