using System.ComponentModel.DataAnnotations;

namespace QuizBuilder.ViewModels.Subject
{
    public class SubjectCreateViewModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
