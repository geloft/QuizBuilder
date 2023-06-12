using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuizBuilder.Data.Entities;
using QuizBuilder.Data;
using System.Data;
using QuizBuilder.ViewModels.Student;
using Microsoft.EntityFrameworkCore;

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

        // GET: Student/SubjectTests/{id}
        public async Task<IActionResult> SubjectTests(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var subject = _dbContext.Subjects.FirstOrDefault(s => s.Id == id);
            if (subject == null)
            {
                return NotFound();
            }

            var studentTests = _dbContext.StudentTests
                .Include(st => st.Test)
                .Where(st => st.StudentId == currentUser.Id && st.Test.SubjectId == id)
                .Select(st => st.Test) // Вибираємо тільки об'єкт тесту
                .Distinct() // Відбираємо унікальні значення тестів
                .ToList();

            var viewModel = new List<StudentTestViewModel>();

            foreach (var test in studentTests)
            {
                var testViewModel = new StudentTestViewModel
                {
                    TestId = test.Id,
                    TestName = test.Name,
                    StartTime = test.StartTime,
                    EndTime = test.EndTime,
                    Duration = test.Duration
                };

                viewModel.Add(testViewModel);
            }

            return View("SubjectTests", viewModel);
        }

    }
}
