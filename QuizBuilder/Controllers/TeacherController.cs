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
using QuizBuilder.ViewModels.Test;

namespace QuizBuilder.Controllers
{
    [Authorize(Roles = "Admin, Teacher")]
    public class TeacherController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public TeacherController(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext)
        {
            _userManager = userManager;
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
                .Include(s => s.SubjectStudents)
                .ThenInclude(ss => ss.Student)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (subject == null)
            {
                return NotFound();
            }

            return View(subject);
        }

        // GET: Teacher/RemoveStudent
        public async Task<IActionResult> RemoveStudent(int subjectId, string studentId)
        {
            var subject = await _dbContext.Subjects
                .Include(s => s.SubjectStudents)
                .FirstOrDefaultAsync(s => s.Id == subjectId);

            if (subject == null)
            {
                return NotFound();
            }

            var subjectStudent = subject.SubjectStudents.FirstOrDefault(ss => ss.StudentId == studentId);
            if (subjectStudent == null)
            {
                return NotFound();
            }

            _dbContext.SubjectStudents.Remove(subjectStudent);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Details", new { id = subjectId });
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

                // Перевірка на наявність ConnectionId в БД
                var existingSubject = await _dbContext.Subjects.FirstOrDefaultAsync(s => s.ConnectionId == subjectModel.ConnectionId);
                if (existingSubject != null)
                {
                    ModelState.AddModelError("ConnectionId", "Такий код підключення вже існує. Використайте інше значення.");
                    return View(subjectModel);
                }

                var subject = new Subject
                {
                    Name = subjectModel.Name,
                    ConnectionId = subjectModel.ConnectionId,
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
                ConnectionId = subject.ConnectionId,
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

                    var existingSubject = await _dbContext.Subjects.FirstOrDefaultAsync(s => s.ConnectionId == subjectModel.ConnectionId && s.Id != id);
                    if (existingSubject != null)
                    {
                        ModelState.AddModelError("ConnectionId", "Такий код підключення вже існує. Використайте інше значення.");
                        return View(subjectModel);
                    }

                    subject.Name = subjectModel.Name;
                    subject.ConnectionId = subjectModel.ConnectionId;
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

        // GET: Teacher/Tests/5
        public async Task<IActionResult> Tests(int id)
        {
            var subject = await _dbContext.Subjects.FindAsync(id);

            if (subject == null)
            {
                return NotFound();
            }

            var tests = await _dbContext.Tests
                .Where(t => t.SubjectId == id)
                .ToListAsync();

            ViewData["SubjectId"] = id;

            return View(tests);
        }

        // GET: Teacher/CreateTest
        public IActionResult CreateTest(int subjectId)
        {
            var viewModel = new TestCreateViewModel
            {
                SubjectId = subjectId
            };

            return View(viewModel);
        }

        // POST: Teacher/CreateTest
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTest(TestCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var test = new Test
                {
                    Name = viewModel.Name,
                    StartTime = viewModel.StartTime,
                    EndTime = viewModel.EndTime,
                    SubjectId = viewModel.SubjectId
                };

                _dbContext.Tests.Add(test);
                await _dbContext.SaveChangesAsync();

                return RedirectToAction("Tests", new { id = viewModel.SubjectId });
            }

            return View(viewModel);
        }

        // GET: Teacher/GenerateQuestions/{id}
        public IActionResult GenerateQuestions(int id)
        {
            var test = _dbContext.Tests.FirstOrDefault(t => t.Id == id);
            if (test == null)
            {
                return NotFound();
            }

            var subject = _dbContext.Subjects
                .Include(s => s.SubjectStudents)
                .FirstOrDefault(s => s.Id == test.SubjectId);
            if (subject == null)
            {
                return NotFound();
            }

            var students = subject.SubjectStudents.Select(s => s.StudentId).ToList();

            var questions = _dbContext.Questions.Where(q => q.TestId == id).ToList();

            // Remove previous StudentTest entries
            var previousStudentTests = _dbContext.StudentTests
                .Where(st => st.TestId == id)
                .ToList();
            _dbContext.StudentTests.RemoveRange(previousStudentTests);

            foreach (var studentId in students)
            {
                foreach (var question in questions)
                {
                    var studentTest = new StudentTest
                    {
                        StudentId = studentId,
                        TestId = test.Id,
                        QuestionId = question.Id
                    };

                    _dbContext.StudentTests.Add(studentTest);
                }
            }

            _dbContext.SaveChanges();

            return RedirectToAction("Tests", "Teacher", new { Id = test.SubjectId });
        }

        public async Task<IActionResult> GetTests()
        {
            var subjects = _dbContext.Subjects.Include(s => s.Tests).ToList();
            return View(subjects);
        }
    }
}
