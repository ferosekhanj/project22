using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project22.Models
{
    public class Account
    {
        public int Id { get; set; }

        public string Mobile { get; set; }

        public int Pin { get; set; }

        public bool IsActive { get; set; }

        public DateTime LastLogin { get; set; }

        public int Tokens { get; set; }

    }
}
