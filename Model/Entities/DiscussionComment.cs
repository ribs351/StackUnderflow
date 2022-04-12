using System;
using System.Collections.Generic;
using System.Text;

namespace Model.Entities
{
    public class DiscussionComment
    {
        public long Id { get; set; }
        public Guid UserID { get; set; }
        public long ReplyID { get; set; }
        public string Text { get; set; }
        public long Upvotes { get; set; }
        public bool Approved { get; set; }
        public DateTime DatePosted { get; set; }
    }
}
