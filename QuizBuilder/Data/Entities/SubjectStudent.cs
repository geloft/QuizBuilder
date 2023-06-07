namespace QuizBuilder.Data.Entities
{
    public class SubjectStudent
    {
        public int Id { get; set; }
        public int SubjectId { get; set; }
        public string StudentId { get; set; }
        public Subject Subject { get; set; }
        public ApplicationUser Student { get; set; }
    }
}
