using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuizBuilder.Data.Entities;
using QuizBuilder.Data;
using QuizBuilder.ViewModels.Test;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;

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

        // GET: Test/EditQuestion
        public IActionResult EditQuestion(int questionId)
        {
            var question = _dbContext.Questions
                .Include(q => q.Options)
                .FirstOrDefault(q => q.Id == questionId);

            if (question == null)
            {
                return NotFound();
            }

            var viewModel = new QuestionEditViewModel
            {
                QuestionId = question.Id,
                TestId=question.TestId,
                Text = question.Text,
                Type = question.Type
            };

            return View(viewModel);
        }

        // POST: Test/EditQuestion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditQuestion(QuestionEditViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var question = await _dbContext.Questions
                    .Include(q => q.Options)
                    .FirstOrDefaultAsync(q => q.Id == viewModel.QuestionId);

                if (question == null)
                {
                    return NotFound();
                }

                // Update question text
                question.Text = viewModel.Text;

                // Update options based on question type
                switch (viewModel.Type)
                {
                    case "SingleChoice":
                        UpdateSingleChoiceOptions(question, viewModel);
                        break;
                    case "MultipleChoice":
                        UpdateMultipleChoiceOptions(question, viewModel);
                        break;
                    case "Matching":
                        UpdateMatchingOptions(question, viewModel);
                        break;
                    case "Open":
                        UpdateOpenOptions(question, viewModel);
                        break;
                    case "Algorithm":
                        UpdateAlgorithmOptions(question, viewModel);
                        break;
                }

                await _dbContext.SaveChangesAsync();

                return RedirectToAction("Index", new { id = question.TestId });
            }

            return View(viewModel);
        }

        private void UpdateSingleChoiceOptions(Question question, QuestionEditViewModel viewModel)
        {
            // Clear existing options
            question.Options.Clear();

            // Create correct option
            var correctOption = new Option
            {
                Text = viewModel.CorrectOptionText,
                IsCorrect = true
            };
            question.Options.Add(correctOption);

            // Create wrong options
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

        private void UpdateMultipleChoiceOptions(Question question, QuestionEditViewModel viewModel)
        {
            // Clear all options
            question.Options.Clear();

            // Create new options
            var options = new List<Option>();

            if (!string.IsNullOrEmpty(viewModel.OptionText1))
            {
                options.Add(new Option
                {
                    Text = viewModel.OptionText1,
                    IsCorrect = viewModel.CorrectOptions.Contains("OptionText1")
                });
            }

            if (!string.IsNullOrEmpty(viewModel.OptionText2))
            {
                options.Add(new Option
                {
                    Text = viewModel.OptionText2,
                    IsCorrect = viewModel.CorrectOptions.Contains("OptionText2")
                });
            }

            if (!string.IsNullOrEmpty(viewModel.OptionText3))
            {
                options.Add(new Option
                {
                    Text = viewModel.OptionText3,
                    IsCorrect = viewModel.CorrectOptions.Contains("OptionText3")
                });
            }

            if (!string.IsNullOrEmpty(viewModel.OptionText4))
            {
                options.Add(new Option
                {
                    Text = viewModel.OptionText4,
                    IsCorrect = viewModel.CorrectOptions.Contains("OptionText4")
                });
            }

            if (!string.IsNullOrEmpty(viewModel.OptionText5))
            {
                options.Add(new Option
                {
                    Text = viewModel.OptionText5,
                    IsCorrect = viewModel.CorrectOptions.Contains("OptionText5")
                });
            }

            // Add options to the question
            question.Options.AddRange(options);
        }

        private void UpdateMatchingOptions(Question question, QuestionEditViewModel viewModel)
        {
            // Clear existing options
            question.Options.Clear();

            // Create options for matching question
            var statement1 = new Option
            {
                Text = viewModel.Statement1,
                IsCorrect = false
            };
            question.Options.Add(statement1);

            var answer1 = new Option
            {
                Text = viewModel.Answer1,
                IsCorrect = false
            };
            question.Options.Add(answer1);

            var statement2 = new Option
            {
                Text = viewModel.Statement2,
                IsCorrect = false
            };
            question.Options.Add(statement2);

            var answer2 = new Option
            {
                Text = viewModel.Answer2,
                IsCorrect = false
            };
            question.Options.Add(answer2);

            var statement3 = new Option
            {
                Text = viewModel.Statement3,
                IsCorrect = false
            };
            question.Options.Add(statement3);

            var answer3 = new Option
            {
                Text = viewModel.Answer3,
                IsCorrect = false
            };
            question.Options.Add(answer3);

            var statement4 = new Option
            {
                Text = viewModel.Statement4,
                IsCorrect = false
            };
            question.Options.Add(statement4);

            var answer4 = new Option
            {
                Text = viewModel.Answer4,
                IsCorrect = false
            };
            question.Options.Add(answer4);
        }

        private void UpdateOpenOptions(Question question, QuestionEditViewModel viewModel)
        {
            // Clear existing options
            question.Options.Clear();

            // Create option for open question
            var option = new Option
            {
                Text = "",
                IsCorrect = true
            };
            question.Options.Add(option);
        }

        private void UpdateAlgorithmOptions(Question question, QuestionEditViewModel viewModel)
        {
            // Clear existing options
            question.Options.Clear();

            // Create options for algorithm question
            var option1 = new Option
            {
                QuestionId = viewModel.QuestionId,
                Text = viewModel.ExpectedAnswer1Text,
                IsCorrect = true
            };
            question.Options.Add(option1);

            var option2 = new Option
            {
                QuestionId = viewModel.QuestionId,
                Text = viewModel.ExpectedAnswer2Text,
                IsCorrect = true
            };
            question.Options.Add(option2);

            var option3 = new Option
            {
                QuestionId = viewModel.QuestionId,
                Text = viewModel.ExpectedAnswer3Text,
                IsCorrect = true
            };
            question.Options.Add(option3);
        }

    }
}
