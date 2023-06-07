using Microsoft.Build.Framework;

namespace QuizBuilder.ViewModels
{
    public class SubjectCreateViewModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
