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

        // GET: Student/StartTest/{id}
        public async Task<IActionResult> StartTest(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var test = _dbContext.Tests.FirstOrDefault(t => t.Id == id);
            if (test == null)
            {
                return NotFound();
            }

            // Перевірка доступності тестування за поточний час
            var now = DateTime.Now;
            if (now < test.StartTime || now > test.EndTime)
            {
                ViewBag.TestAvailability = "Тестування недоступне";
            }
            else
            {
                ViewBag.TestAvailability = "Розпочати тестування";
            }

            return View("StartTest", test);
        }

        // GET: Student/StartQuestions/{id}
        public async Task<IActionResult> StartQuestions(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var studentTest = _dbContext.StudentTests
                .Include(st => st.Test)
                .ThenInclude(t => t.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefault(st => st.StudentId == currentUser.Id && st.TestId == id);

            if (studentTest != null)
            {
                studentTest.Test.Questions = studentTest.Test.Questions.OrderBy(q => q.Type switch
                {
                    "SingleChoice" => 1,
                    "MultipleChoice" => 2,
                    "Matching" => 3,
                    "Open" => 4,
                    _ => 5
                }).ToList();
            }

            if (studentTest == null)
            {
                return NotFound();
            }

            return View("StartQuestions", studentTest);
        }


        // POST: Student/PostStartQuestions/{id}
        [HttpPost]
        public async Task<IActionResult> StartQuestions(int id, [FromForm] Dictionary<int, string> answers)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var studentTest = _dbContext.StudentTests
                .Include(st => st.Test)
                .Include(st => st.Question)
                .ThenInclude(q => q.Options)
                .FirstOrDefault(st => st.StudentId == currentUser.Id && st.TestId == id);

            if (studentTest == null)
            {
                return NotFound();
            }

            foreach (var question in studentTest.Test.Questions)
            {
                if (question.Type == "SingleChoice" || question.Type == "MultipleChoice" || question.Type == "Matching")
                {
                    var selectedOptions = new List<int>();
                    foreach (var option in question.Options)
                    {
                        if (answers.ContainsKey(option.Id) && answers[option.Id] == "on")
                        {
                            selectedOptions.Add(option.Id);
                        }
                    }

                    // Збереження відповідей MultipleChoice та Matching
                    foreach (var optionId in selectedOptions)
                    {
                        var studentAnswer = new StudentAnswer
                        {
                            Text = null,
                            StudentTestId = studentTest.Id,
                            OptionId = optionId
                        };
                        _dbContext.StudentAnswers.Add(studentAnswer);
                    }
                }
                else if (question.Type == "Open")
                {
                    var studentAnswer = new StudentAnswer
                    {
                        Text = answers.ContainsKey(question.Id) ? answers[question.Id] : null,
                        StudentTestId = studentTest.Id,
                        OptionId = 0
                    };
                    _dbContext.StudentAnswers.Add(studentAnswer);
                }
            }

            _dbContext.SaveChanges();

            return RedirectToAction("FinishTest", new { id = studentTest.Id });
        }

    }
}
