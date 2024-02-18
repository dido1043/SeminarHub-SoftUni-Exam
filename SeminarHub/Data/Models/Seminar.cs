using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeminarHub.Data.Models
{
    public class Seminar
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(ValidationConstants.TopicMaxLength)]
        [MinLength(ValidationConstants.TopicMinLength)]
        public string Topic { get; set; }
        [Required]
        [MaxLength(ValidationConstants.LecturerMaxLength)]
        [MinLength(ValidationConstants.LecturerMinLength)]
        public string  Lecturer { get; set; }
        [Required]
        [MaxLength(ValidationConstants.DetailsMaxLength)]
        [MinLength(ValidationConstants.DetailsMinLength)]
        public string Details { get; set; }
        [Required]
        public string OrganizerId { get; set; }
        [Required]
        [ForeignKey(nameof(OrganizerId))]
        public IdentityUser Organizer { get; set; }
        [Required]
        public DateTime DateAndTime { get; set; }

        [Range(30,180)]
        public int Duration { get; set; }
        [Required]
        public int CategoryId { get; set; }
        [Required]
        [ForeignKey(nameof(CategoryId))]
        public Category Category { get; set; }

        public IList<SeminarParticipant> SeminarParticipants =
            new List<SeminarParticipant>();

    }
}
