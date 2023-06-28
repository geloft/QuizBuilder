using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuizBuilder.Data.Entities;
using QuizBuilder.Data;
using System.Data;
using QuizBuilder.ViewModels.Student;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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
                    EndTime = test.EndTime
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
                studentTest.Test.Questions = studentTest.Test.Questions
                    .Where(q => q.Type != "Algorithm")
                    .OrderBy(q => q.Type switch
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

        [HttpPost]
        public async Task<IActionResult> StartQuestions(int id, IFormCollection form)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var studentTest = _dbContext.StudentTests
                .Include(st => st.Test)
                .ThenInclude(t => t.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefault(st => st.StudentId == currentUser.Id && st.TestId == id);

            if (studentTest == null)
            {
                return NotFound();
            }

            var studentAnswers = new List<StudentAnswer>();

            foreach (var question in studentTest.Test.Questions)
            {
                var selectedOptions = form["Question_" + question.Id];

                if (question.Type == "SingleChoice")
                {
                    if (int.TryParse(selectedOptions, out int selectedOptionId))
                    {
                        var studentAnswer = new StudentAnswer
                        {
                            Text = "",
                            OptionId = selectedOptionId
                        };
                        studentAnswers.Add(studentAnswer);
                    }
                }
                else if (question.Type == "MultipleChoice")
                {
                    if (selectedOptions.Count > 0)
                    {
                        foreach (var selectedOption in selectedOptions)
                        {
                            if (int.TryParse(selectedOption, out int selectedOptionId))
                            {
                                var studentAnswer = new StudentAnswer
                                {
                                    Text = "",
                                    OptionId = selectedOptionId
                                };
                                studentAnswers.Add(studentAnswer);
                            }
                        }
                    }
                }
                else if (question.Type == "Matching")
                {
                    for (var i = 0; i < question.Options.Count; i += 2)
                    {
                        var statement = question.Options.ElementAt(i);
                        var selectedOptionId = int.Parse(form["Question_" + question.Id + "_" + statement.Id]);
                        var studentAnswer = new StudentAnswer
                        {
                            OptionId = statement.Id,
                            Text = selectedOptionId.ToString()
                        };
                        studentAnswers.Add(studentAnswer);
                    }
                }
                else if (question.Type == "Open")
                {
                    var text = form["Question_" + question.Id];
                    var questionIdTemp = _dbContext.Options
                    .Where(o => o.QuestionId == question.Id)
                    .Select(o => o.Id)
                    .FirstOrDefault();

                    var studentAnswer = new StudentAnswer
                    {
                        OptionId = questionIdTemp,
                        Text = text
                    };
                    studentAnswers.Add(studentAnswer);
                }
            }
            _dbContext.StudentAnswers.RemoveRange(_dbContext.StudentAnswers.Where(sa => sa.StudentTestId == studentTest.Id));

            // Save student answers to the database
            foreach (var answer in studentAnswers)
            {
                answer.StudentTestId = studentTest.Id;
                _dbContext.StudentAnswers.Add(answer);
            }
            await _dbContext.SaveChangesAsync();

            // Check if the test contains any "Algorithm" questions
            if (studentTest.Test.Questions.Any(q => q.Type == "Algorithm"))
            {
                return RedirectToAction("StartAlgorithm", new { id = studentTest.TestId });
            }
            else
            {
                return RedirectToAction("FinishTest", new { id = studentTest.Id });
            }
        }

        // GET: Student/StartAlgorithm/{id}
        public async Task<IActionResult> StartAlgorithm(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var studentTest = _dbContext.StudentTests
                .Include(st => st.Test)
                .ThenInclude(t => t.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefault(st => st.StudentId == currentUser.Id && st.TestId == id);

            if (studentTest == null)
            {
                return NotFound();
            }

            var algorithmQuestions = studentTest.Test.Questions
                .Where(q => q.Type == "Algorithm")
                .ToList();

            return View("StartAlgorithm", algorithmQuestions);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessCode(Dictionary<string, string> questionAnswers, Dictionary<string, string> questionLanguages)
        {
            var model = new List<TestAlgorithmViewModel>();

            foreach (var questionAnswer in questionAnswers)
            {
                var questionId = questionAnswer.Key.Replace("Question_", "");
                if (int.TryParse(questionId, out int parsedQuestionId))
                {
                    var languageKey = "Language_" + questionId;
                    var language = questionLanguages.ContainsKey(languageKey) ? questionLanguages[languageKey] : "";
                    var code = questionAnswer.Value;
                    var questionModel = new TestAlgorithmViewModel
                    {
                        TestId = 0,
                        QuestionId = parsedQuestionId,
                        code = code,
                        language = language
                    };
                    var testId = _dbContext.Questions
                        .Where(q => q.Id == questionModel.QuestionId)
                        .Select(q => q.TestId)
                        .FirstOrDefault();

                    questionModel.TestId = testId;
                    model.Add(questionModel);
                }
                continue;
            }


            var currentUser = await _userManager.GetUserAsync(User);
            var testsPassed = new List<AlgorithmResultViewModel>();

            foreach (var questionModel in model)
            {
                var options = _dbContext.Options
                    .Where(o => o.QuestionId == questionModel.QuestionId)
                    .ToList();

                var testPassed = new AlgorithmResultViewModel();

                using (var client = new HttpClient()) // Створення нового об'єкта HttpClient
                {
                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        RequestUri = new Uri("https://online-code-compiler.p.rapidapi.com/v1/"),
                        Headers =
                {
                    { "X-RapidAPI-Key", "cdf60e663cmsh4dc5c0e771235dep1d3b40jsnaec7880ece49" },
                    { "X-RapidAPI-Host", "online-code-compiler.p.rapidapi.com" },
                },
                    };
                    var request2 = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        RequestUri = new Uri("https://online-code-compiler.p.rapidapi.com/v1/"),
                        Headers =
                {
                    { "X-RapidAPI-Key", "cdf60e663cmsh4dc5c0e771235dep1d3b40jsnaec7880ece49" },
                    { "X-RapidAPI-Host", "online-code-compiler.p.rapidapi.com" },
                },
                    };
                    var request3 = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        RequestUri = new Uri("https://online-code-compiler.p.rapidapi.com/v1/"),
                        Headers =
                {
                    { "X-RapidAPI-Key", "cdf60e663cmsh4dc5c0e771235dep1d3b40jsnaec7880ece49" },
                    { "X-RapidAPI-Host", "online-code-compiler.p.rapidapi.com" },
                },
                    };

                    // Окремий response для тесту 1
                    var content1 = new StringContent(JsonConvert.SerializeObject(new
                    {
                        language = questionModel.language,
                        version = "latest",
                        code = questionModel.code,
                        input = options.ElementAt(1).Text
                    }), Encoding.UTF8, "application/json");

                    request.Content = content1;

                    using (var response = await client.SendAsync(request))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            throw new HttpRequestException($"The API request was unsuccessful with status code: {response.StatusCode}");
                        }

                        var body = await response.Content.ReadAsStringAsync();

                        var result = JsonConvert.DeserializeObject<CodeProcessingResult>(body);

                        if (result.output == options.ElementAt(0).Text + "\n")
                        {
                            testPassed.testPassed.test1 = "Тест 1. Пройдено";
                        }
                        else
                        {
                            testPassed.testPassed.test1 = "Тест 1. НЕ ПРОЙДЕНО";
                        }
                    }

                    // Окремий response для тесту 2
                    var content2 = new StringContent(JsonConvert.SerializeObject(new
                    {
                        language = questionModel.language,
                        version = "latest",
                        code = questionModel.code,
                        input = options.ElementAt(3).Text
                    }), Encoding.UTF8, "application/json");

                    request2.Content = content2;

                    using (var response = await client.SendAsync(request2))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            throw new HttpRequestException($"The API request was unsuccessful with status code: {response.StatusCode}");
                        }

                        var body = await response.Content.ReadAsStringAsync();

                        var result = JsonConvert.DeserializeObject<CodeProcessingResult>(body);

                        if (result.output == options.ElementAt(2).Text + "\n")
                        {
                            testPassed.testPassed.test2 = "Тест 2. Пройдено";
                        }
                        else
                        {
                            testPassed.testPassed.test2 = "Тест 2. НЕ ПРОЙДЕНО";
                        }
                    }

                    // Окремий response для тесту 3
                    var content3 = new StringContent(JsonConvert.SerializeObject(new
                    {
                        language = questionModel.language,
                        version = "latest",
                        code = questionModel.code,
                        input = options.ElementAt(5).Text
                    }), Encoding.UTF8, "application/json");

                    request3.Content = content3;

                    using (var response = await client.SendAsync(request3))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            throw new HttpRequestException($"The API request was unsuccessful with status code: {response.StatusCode}");
                        }

                        var body = await response.Content.ReadAsStringAsync();

                        var result = JsonConvert.DeserializeObject<CodeProcessingResult>(body);

                        if (result.output == options.ElementAt(4).Text + "\n")
                        {
                            testPassed.testPassed.test3 = "Тест 3. Пройдено";
                        }
                        else
                        {
                            testPassed.testPassed.test3 = "Тест 3. НЕ ПРОЙДЕНО";
                        }
                    }

                    if (testPassed.testPassed.test1 == "Тест 1. Пройдено" && testPassed.testPassed.test2 == "Тест 2. Пройдено" && testPassed.testPassed.test3 == "Тест 3. Пройдено")
                    {
                        var studentTestId = _dbContext.StudentTests
                            .Where(st => st.StudentId == currentUser.Id && st.TestId == questionModel.TestId)
                            .Select(st => st.Id)
                            .FirstOrDefault();

                        var existingAnswers = _dbContext.StudentAnswers
                            .Where(sa => sa.StudentTestId == studentTestId && sa.OptionId == options.First().Id)
                            .ToList();

                        _dbContext.StudentAnswers.RemoveRange(existingAnswers);

                        var studentAnswer = new StudentAnswer
                        {
                            StudentTestId = studentTestId,
                            OptionId = options.First().Id,
                            Text = "Passed"
                        };

                        _dbContext.StudentAnswers.Add(studentAnswer);
                        _dbContext.SaveChanges();
                    }
                }
                var questionText = await _dbContext.Questions
                    .Where(q => q.Id == questionModel.QuestionId)
                    .Select(q => q.Text)
                    .FirstOrDefaultAsync();
                testPassed.StudentTestId = _dbContext.StudentTests
                            .Where(st => st.StudentId == currentUser.Id && st.TestId == questionModel.TestId)
                            .Select(st => st.Id)
                            .FirstOrDefault();
                testPassed.QuestionText = questionText;
                testPassed.TestId = questionModel.TestId;
                testsPassed.Add(testPassed);
            }

            return View("TestAlgorithm", testsPassed);
        }

        public async Task<IActionResult> FinishTest(int TestId, int StudentTestId)
        {

            var test = _dbContext.Tests
                .Include(t => t.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefault(t => t.Id == TestId);

            var studentTest = _dbContext.StudentTests
                .Include(st => st.Student)
                .Include(st => st.Question)
                    .ThenInclude(q => q.Options)
                .FirstOrDefault(st => st.Id == StudentTestId);

            if (test == null || studentTest == null)
            {
                return NotFound();
            }


            float score = 0;

            foreach (var question in test.Questions)
            {
                var studentAnswer = _dbContext.StudentAnswers
                    .FirstOrDefault(sa => sa.StudentTestId == StudentTestId && sa.Option.QuestionId == question.Id);

                if (studentAnswer == null)
                {
                    continue;
                }

                if (question.Type == "SingleChoice" || question.Type == "MultipleChoice")
                {
                    foreach (var option in question.Options)
                    {
                        if (option.IsCorrect && studentAnswer.OptionId == option.Id)
                        {
                            score += question.Score;
                        }
                        else if (!option.IsCorrect && studentAnswer.OptionId == option.Id)
                        {
                            score -= 1;
                        }
                    }
                }
                else if (question.Type == "Matching")
                {
                    int matchingOptionsCount = question.Options.Count;

                    foreach (var option in question.Options)
                    {
                        if (int.TryParse(option.Text, out int number) && number == (option.Id + 1))
                        {
                            score += question.Score / matchingOptionsCount;
                        }
                    }
                }
                else if (question.Type == "Algorithm")
                {
                    if (studentAnswer.Text == "Passed")
                    {
                        score += question.Score;
                    }
                }
            }


            var testResult = new TestResult
            {
                TestId = TestId,
                StudentId = studentTest.StudentId,
                Score = score
            };

            var removeTests = _dbContext.TestResults.Where(x => x.TestId == testResult.TestId);

            _dbContext.TestResults.RemoveRange(removeTests);
            _dbContext.TestResults.Add(testResult);
            _dbContext.SaveChanges();

            return RedirectToAction("Results", "Student");
        }
        public async Task<IActionResult> Results()
        {
            var results = await _dbContext.TestResults
                .Include(tr => tr.Test)
                .ThenInclude(t => t.Subject)
                .ToListAsync();

            return View(results);
        }
    }
}
