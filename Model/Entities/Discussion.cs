using System;
using System.Collections.Generic;
using System.Text;

namespace Model.Entities
{
    public class Discussion
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public DateTime DatePosted { get; set; }
        public Guid UserID { get; set; }
        public long Upvotes { get; set; }
        public long BookmarkCount { get; set; }
        public long ViewCount { get; set; }
        public long CommentCount { get; set; }

    }
}
