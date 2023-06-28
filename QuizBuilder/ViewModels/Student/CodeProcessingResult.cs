namespace QuizBuilder.ViewModels.Student
{
    public class CodeProcessingResult
    {
        public string cpuTime { get; set; }
        public string memory { get; set; }
        public string output { get; set; }
        public LanguageInfo language { get; set; }
    }

    public class LanguageInfo
    {
        public string id { get; set; }
        public string version { get; set; }
        public string version_name { get; set; }
    }
}
