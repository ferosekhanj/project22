using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Project22.Models
{
    public class Token
    {
        public int Id { get; set; }

        public int Number { get; set; }

        [Required]
        public string Mobile { get; set; }

        public Session Session { get; set; }

        public override string ToString() => $"{Id} {Number} {Mobile} {Session.Name}-{Session.StartTime} {Session.Mobile}";

    }
}
