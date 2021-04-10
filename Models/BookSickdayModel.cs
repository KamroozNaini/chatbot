using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreBot.Models
{
    public class BookSickDayModel
    {
        public string Name { get; set; }

        public DateTime StartFrom { get; set; }

        public int Duration { get; set; }
    }
}
