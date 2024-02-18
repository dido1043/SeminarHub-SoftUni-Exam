using SeminarHub.Data.Models;

namespace SeminarHub.Models.ViewModels
{
    public class FormViewModel
    {
        public string Topic { get; set; }

        public string Lecturer { get; set; }
        public string Details { get; set; }
        public string DateAndTime { get; set; }
        public int Duration { get; set; }
        public int CategoryId { get; set; }
        public IEnumerable<CategoryViewModel> Categories { get; set; }
        = new List<CategoryViewModel>();


    }
}
