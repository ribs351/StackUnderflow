using System;
using System.Collections.Generic;
using System.Text;

namespace Model.Entities
{
    public class Series
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public DateTime DatePosted { get; set; }
        public long BookmarkCount { get; set; }
        public string UserID { get; set; }
    }
}
