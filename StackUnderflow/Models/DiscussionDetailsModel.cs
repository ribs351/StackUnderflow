using Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StackUnderflow.Models
{
    public class DiscussionDetailsModel
    {
        public long Id { get; set; }
        public Guid UserId { get; set; }
        public bool Followed { get; set; }
        public bool Bookmarked { get; set; }
        public bool Owner { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public string UserName { get; set; }
        public DateTime DatePosted { get; set; }
        public long Upvotes { get; set; }
        public long BookmarkCount { get; set; }
        public long ViewCount { get; set; }
        public string NewTag { get; set; }
        public List<Tag> Tags { get; set; } = new List<Tag>();
    }
}
