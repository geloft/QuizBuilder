using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuizBuilder.Data.Entities;

namespace QuizBuilder.Controllers
{
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: /User/AssignRole
        public IActionResult AssignRole()
        {
            var users = _userManager.Users.ToList();
            ViewBag.Roles = _roleManager.Roles.ToList();
            return View(users);
        }

        // POST: /User/AssignRole
        [HttpPost]
        public async Task<IActionResult> AssignRole(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                // Remove all existing roles
                var existingRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, existingRoles);

                // Assign the new role
                await _userManager.AddToRoleAsync(user, roleName);
            }

            return RedirectToAction("AssignRole");
        }

        // GET: /User/Search
        public IActionResult Search()
        {
            ViewBag.Roles = _roleManager.Roles.ToList();
            return View();
        }

        // POST: /User/Search
        [HttpPost]
        public async Task<IActionResult> Search(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            ViewBag.Roles = _roleManager.Roles.ToList();
            return View("SearchResult", user);
        }
    }
}
