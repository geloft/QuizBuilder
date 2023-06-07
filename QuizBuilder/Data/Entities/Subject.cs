namespace QuizBuilder.Data.Entities
{
    public class Subject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string TeacherId { get; set; }
        public ApplicationUser Teacher { get; set; }
        public ICollection<SubjectStudent> SubjectStudents { get; set; }
        public ICollection<Test> Tests { get; set; }
    }
}
