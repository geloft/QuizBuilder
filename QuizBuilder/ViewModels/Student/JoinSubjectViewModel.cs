using System.ComponentModel.DataAnnotations;

namespace QuizBuilder.ViewModels.Student
{
    public class JoinSubjectViewModel
    {
        [Required]
        public string ConnectionId { get; set; }

        [Required]
        public string Password { get; set; }
    }

}
