namespace QuizBuilder.Data.Entities
{
    public class TestResult
    {
        public int Id { get; set; }
        public int TestId { get; set; }
        public string StudentId { get; set; }
        public float Score { get; set; }
        public Test Test { get; set; }
        public ApplicationUser Student { get; set; }
    }
}
