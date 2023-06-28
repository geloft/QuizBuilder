namespace QuizBuilder.ViewModels.Student
{
    public class StudentTestViewModel
    {
        public int TestId { get; set; }
        public string TestName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get; set; }
    }

}
