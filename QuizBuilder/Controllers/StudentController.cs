using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuizBuilder.Data.Entities;
using QuizBuilder.Data;
using System.Data;
using QuizBuilder.ViewModels.Student;

namespace QuizBuilder.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentController(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
        }

        // GET: Student/Index
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var subjects = _dbContext.Subjects
                .Where(s => s.SubjectStudents.Any(ss => ss.StudentId == user.Id))
                .ToList();

            return View(subjects);
        }

        // GET: Student/JoinSubject
        public IActionResult JoinSubject()
        {
            return View();
        }

        // POST: Student/JoinSubject
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> JoinSubject(JoinSubjectViewModel model)
        {
            if (ModelState.IsValid)
            {
                var subject = _dbContext.Subjects.FirstOrDefault(s => s.ConnectionId == model.ConnectionId && s.Password == model.Password);
                if (subject == null)
                {
                    ModelState.AddModelError(string.Empty, "Неправильно введено Код доступу чи Код підключення");
                    return View(model);
                }

                var user = await _userManager.GetUserAsync(User);
                var subjectStudent = new SubjectStudent
                {
                    StudentId = user.Id,
                    SubjectId = subject.Id
                };

                _dbContext.SubjectStudents.Add(subjectStudent);
                await _dbContext.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: Student/SubjectTests/5
        public IActionResult SubjectTests()
        {
            // TODO
            return View();
        }
    }
}
