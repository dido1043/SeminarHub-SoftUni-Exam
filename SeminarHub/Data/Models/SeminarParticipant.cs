﻿using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeminarHub.Data.Models
{
    public class SeminarParticipant
    {
        [Required]
        public int SeminarId { get; set; }
        [ForeignKey(nameof(SeminarId))]
        public Seminar Seminar { get; set; }
        [Required]
        public string ParticipantId { get; set; }
        [ForeignKey(nameof(ParticipantId))]
        public IdentityUser Participant { get; set; }
    }
}
