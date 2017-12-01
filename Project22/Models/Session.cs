using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Project22.Models
{
    public class Session
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Mobile { get; set; }

        public DateTime StartTime { get; set; }

        public int AccountId { get; set; }

        [ConcurrencyCheck]
        public int TokenCount { get; set; }

        public ICollection<Token> Tokens { get; set; }

        public Session()
        {
            Tokens = new List<Token>();
        }

        public override string ToString() => $"{Id} {Name} {Mobile} {StartTime} {TokenCount} {AccountId}";
    }
}
