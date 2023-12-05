using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Case_Service.Models
{
    public class Word
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string LetterCounter { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime DeletedAt { get; set; }
    }
}
