using System;
using System.Collections.Generic;
using System.Text;

namespace Model.Entities
{
    public class DiscussionBookmark
    {
        public Guid UserID { get; set; }
        public long DiscussionID { get; set; }
    }
}
