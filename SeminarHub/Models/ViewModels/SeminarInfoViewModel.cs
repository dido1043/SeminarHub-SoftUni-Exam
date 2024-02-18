using Microsoft.AspNetCore.Identity;
using SeminarHub.Data;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SeminarHub.Models.ViewModels
{
    public class SeminarInfoViewModel
    {
        public SeminarInfoViewModel(int id, string topic, string lecturer, string details, DateTime dateAndTime , string category, string organizer)
        {
            Id = id;
            Topic = topic;
            Lecturer = lecturer;
            Details = details;
            DateAndTime = dateAndTime.ToString(ValidationConstants.DateTimeFormat);
            Category = category;
            Organizer = organizer;

        }
        public int Id { get; set; }

        public string Topic { get; set; }

        public string Lecturer { get; set; }
        public string Category { get; set; }
        public string Organizer { get; set; }
        public string Details { get; set; }
        public string DateAndTime { get; set; }
    }
}
