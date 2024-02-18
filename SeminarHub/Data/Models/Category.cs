using System.ComponentModel.DataAnnotations;

namespace SeminarHub.Data.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(ValidationConstants.TypeNameMaxLength)]
        [MinLength(ValidationConstants.TypeNameMinLength)]
        public string Name { get; set; }
        public IEnumerable<Seminar> Seminars { get; set; }
        = new List<Seminar>();
    }
}