using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuizBuilder.Data.Entities;
using QuizBuilder.Data;
using QuizBuilder.ViewModels.Test;
using Microsoft.EntityFrameworkCore;

namespace QuizBuilder.Controllers
{
    public class TestController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public TestController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: Test/Index/{id}
        public IActionResult Index(int id)
        {
            var test = _dbContext.Tests.FirstOrDefault(t => t.Id == id);
            if (test == null)
            {
                return NotFound();
            }

            var questions = _dbContext.Questions.Where(q => q.TestId == id).ToList();

            ViewData["TestName"] = test.Name;
            ViewData["SubjectId"] = test.SubjectId;
            ViewData["TestId"] = test.Id;

            return View(questions);
        }

        // GET: Test/CreateQuestion/{id}
        public IActionResult CreateQuestion(int id)
        {
            var viewModel = new QuestionCreateViewModel
            {
                TestId = id
            };

            return View(viewModel);
        }

        // POST: Test/CreateQuestion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateQuestion(QuestionCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var question = new Question
                {
                    Text = viewModel.Text,
                    Type = viewModel.Type,
                    TestId = viewModel.TestId
                };

                _dbContext.Questions.Add(question);
                _dbContext.SaveChanges();

                return RedirectToAction("Index", new { id = viewModel.TestId });
            }

            return View(viewModel);
        }

        // GET: Test/Edit
        [HttpGet]
        public IActionResult EditQuestion(int questionId)
        {
            var question = _dbContext.Questions.Include(q => q.Options).FirstOrDefault(q => q.Id == questionId);

            if (question == null)
            {
                return NotFound();
            }

            var viewModel = new QuestionEditViewModel
            {
                QuestionId = question.Id,
                Text = question.Text,
                Type = question.Type,
                Options = question.Options.ToList()
            };

            ViewData["TestId"] = question.TestId;

            return View(viewModel);
        }

        // POST: Test/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditQuestion(QuestionEditViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var question = _dbContext.Questions.Include(q => q.Options).FirstOrDefault(q => q.Id == viewModel.QuestionId);

                if (question == null)
                {
                    return NotFound();
                }

                question.Text = viewModel.Text;
                question.Type = viewModel.Type;

                // Очистити старі варіанти відповіді
                question.Options.Clear();

                // Додати нові варіанти відповіді
                if (viewModel.Type == "SingleChoice")
                {
                    var correctOption = new Option
                    {
                        Text = viewModel.CorrectOptionText,
                        IsCorrect = true
                    };
                    question.Options.Add(correctOption);

                    var wrongOption1 = new Option
                    {
                        Text = viewModel.WrongOption1Text,
                        IsCorrect = false
                    };
                    question.Options.Add(wrongOption1);

                    var wrongOption2 = new Option
                    {
                        Text = viewModel.WrongOption2Text,
                        IsCorrect = false
                    };
                    question.Options.Add(wrongOption2);
                }
                else if (viewModel.Type == "Algorithm")
                {
                    var expectedAnswer1 = new Option
                    {
                        Text = viewModel.ExpectedAnswer1Text,
                        IsCorrect = false
                    };
                    question.Options.Add(expectedAnswer1);

                    var expectedAnswer2 = new Option
                    {
                        Text = viewModel.ExpectedAnswer2Text,
                        IsCorrect = false
                    };
                    question.Options.Add(expectedAnswer2);

                    var expectedAnswer3 = new Option
                    {
                        Text = viewModel.ExpectedAnswer3Text,
                        IsCorrect = false
                    };
                    question.Options.Add(expectedAnswer3);
                }

                await _dbContext.SaveChangesAsync();

                return RedirectToAction("Index", new { id = question.TestId });
            }

            return View(viewModel);
        }
    }
}
