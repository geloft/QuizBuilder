using QuizBuilder.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace QuizBuilder.ViewModels.Test
{
    public class QuestionEditViewModel
    {
        public int QuestionId { get; set; }
        public int TestId { get; set; }

        [Required(ErrorMessage = "The Text field is required.")]
        public string Text { get; set; }

        public string Type { get; set; }

        // Properties for SingleChoice
        public string CorrectOptionText { get; set; }
        public string WrongOption1Text { get; set; }
        public string WrongOption2Text { get; set; }

        // Properties for MultipleChoice
        public string OptionText1 { get; set; }
        public string OptionText2 { get; set; }
        public string OptionText3 { get; set; }
        public string OptionText4 { get; set; }
        public string OptionText5 { get; set; }
        public List<string> CorrectOptions { get; set; }

        // Properties for Matching
        public string Statement1 { get; set; }
        public string Answer1 { get; set; }
        public string Statement2 { get; set; }
        public string Answer2 { get; set; }
        public string Statement3 { get; set; }
        public string Answer3 { get; set; }
        public string Statement4 { get; set; }
        public string Answer4 { get; set; }

        // Property for Algorithm
        public string ExpectedAnswer1Text { get; set; }
        public string Answer1Input { get; set; }
        public string ExpectedAnswer2Text { get; set; }
        public string Answer2Input { get; set; }
        public string ExpectedAnswer3Text { get; set; }
        public string Answer3Input { get; set; }
    }
}
