using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace QuizBuilder.ViewModels.Test
{
    public class TestCreateViewModel
    {
        public int SubjectId { get; set; }

        [Required(ErrorMessage = "Поле 'Назва тестування' є обов'язковим.")]
        [Display(Name = "Назва тестування")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Поле 'Час початку' є обов'язковим.")]
        [Display(Name = "Час початку")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "Поле 'Час кінця' є обов'язковим.")]
        [Display(Name = "Час кінця")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime EndTime { get; set; }
    }

}
