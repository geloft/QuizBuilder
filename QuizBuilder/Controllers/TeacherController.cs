using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuizBuilder.Data;
using QuizBuilder.Data.Entities;
using QuizBuilder.ViewModels;
using QuizBuilder.ViewModels.Subject;

namespace QuizBuilder.Controllers
{
    [Authorize(Roles = "Admin, Teacher")]
    public class TeacherController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public TeacherController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _dbContext = dbContext;
        }

        // GET: Teacher
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var teacherId = currentUser.Id;

            var subjects = await _dbContext.Subjects
                .Include(s => s.Teacher)
                .Where(s => s.TeacherId == teacherId)
                .ToListAsync();

            return View(subjects);
        }


        // GET: Teacher/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _dbContext.Subjects == null)
            {
                return NotFound();
            }

            var subject = await _dbContext.Subjects
                .Include(s => s.Teacher)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (subject == null)
            {
                return NotFound();
            }

            return View(subject);
        }

        // GET: Teacher/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Teacher/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubjectCreateViewModel subjectModel)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var subject = new Subject
                {
                    Name = subjectModel.Name,
                    Password = subjectModel.Password,
                    TeacherId = user.Id
                };
                _dbContext.Add(subject);
                await _dbContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View("Index");
        }

        // GET: Teacher/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _dbContext.Subjects == null)
            {
                return NotFound();
            }

            var subject = await _dbContext.Subjects.FindAsync(id);
            if (subject == null)
            {
                return NotFound();
            }

            var subjectModel = new SubjectEditViewModel
            {
                Id = subject.Id,
                Name = subject.Name,
                Password = subject.Password
            };
            return View(subjectModel);
        }

        // POST: Teacher/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SubjectEditViewModel subjectModel)
        {
            if (id != subjectModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var subject = await _dbContext.Subjects.FindAsync(id);
                    if (subject == null)
                    {
                        return NotFound();
                    }

                    subject.Name = subjectModel.Name;
                    subject.Password = subjectModel.Password;

                    _dbContext.Update(subject);
                    await _dbContext.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SubjectExists(subjectModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(subjectModel);
        }



        // GET: Teacher/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _dbContext.Subjects == null)
            {
                return NotFound();
            }

            var subject = await _dbContext.Subjects
                .Include(s => s.Teacher)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (subject == null)
            {
                return NotFound();
            }

            return View(subject);
        }

        // POST: Teacher/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_dbContext.Subjects == null)
            {
                return Problem("Entity set 'ApplicationDbdbContext.Subjects'  is null.");
            }
            var subject = await _dbContext.Subjects.FindAsync(id);
            if (subject != null)
            {
                _dbContext.Subjects.Remove(subject);
            }

            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SubjectExists(int id)
        {
            return (_dbContext.Subjects?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
