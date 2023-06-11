using System.ComponentModel.DataAnnotations;

namespace QuizBuilder.ViewModels.Test
{
    public class QuestionCreateViewModel
    {
        public int TestId { get; set; }
        [Required]
        public string Type { get; set; }
        [Required]
        public string Text { get; set; }
    }
}
