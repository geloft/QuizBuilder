using System.ComponentModel.DataAnnotations;

namespace QuizBuilder.ViewModels.Subject
{
    public class SubjectEditViewModel
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
