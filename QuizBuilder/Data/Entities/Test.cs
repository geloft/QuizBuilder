namespace QuizBuilder.Data.Entities
{
    public class Test
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int SubjectId { get; set; }
        public Subject Subject { get; set; }
        public ICollection<Question> Questions { get; set; }
    }
}
