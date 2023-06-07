namespace QuizBuilder.Data.Entities
{
    public class StudentTest
    {
        public int Id { get; set; }
        public string StudentId { get; set; }
        public int TestId { get; set; }
        public int QuestionId { get; set; }
        public ApplicationUser Student { get; set; }
        public Test Test { get; set; }
        public Question Question { get; set; }
    }
}
