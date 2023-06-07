namespace QuizBuilder.Data.Entities
{
    public class StudentAnswer
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int StudentTestId { get; set; }
        public int OptionId { get; set; }
        public StudentTest StudentTest { get; set; }
        public Option Option { get; set; }
    }
}
