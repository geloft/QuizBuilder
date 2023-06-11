using QuizBuilder.Data.Entities;

namespace QuizBuilder.ViewModels.Test
{
    public class QuestionEditViewModel
    {
        public int QuestionId { get; set; }
        public string Text { get; set; }
        public string Type { get; set; }
        public ICollection<Option> Options { get; set; }

        // Properties for SingleChoice
        public string CorrectOptionText { get; set; }
        public string WrongOption1Text { get; set; }
        public string WrongOption2Text { get; set; }

        // Properties for Algorithm
        public string ExpectedAnswer1Text { get; set; }
        public string ExpectedAnswer2Text { get; set; }
        public string ExpectedAnswer3Text { get; set; }
    }
}
