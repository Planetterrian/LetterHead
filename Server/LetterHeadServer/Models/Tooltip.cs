using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LetterHeadServer.Models
{
    public class Tooltip
    {
        public Tooltip()
        {
            Active = true;
        }

        public int Id { get; set; }
        public string Content { get; set; }
        public bool Active { get; set; }
    }
}