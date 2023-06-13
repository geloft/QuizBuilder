namespace QuizBuilder.ViewModels.Student
{
    public class AlgorithmResultViewModel
    {
        public string QuestionText { get; set; }
        public int TestId { get; set; }
        public AlgorithmTestPassed testPassed { get; set; } = new AlgorithmTestPassed();
    }
    public class AlgorithmTestPassed
    {
        public string test1 { get; set; }
        public string test2 { get; set; }
        public string test3 { get; set; }

    }
}
