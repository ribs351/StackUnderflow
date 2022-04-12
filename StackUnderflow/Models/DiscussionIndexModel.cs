using Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StackUnderflow.Models
{
    public class DiscussionIndexModel
    {
        public long Id { get; set; }
        public string DiscussionName { get; set; }
        public string UserName { get; set; }
        public DateTime DatePosted { get; set; }
        public long Upvotes { get; set; }
        public long BookmarkCount { get; set; }
        public long ViewCount { get; set; }
        public long CommentCount { get; set; }
        public List<Tag> Tags { get; set; } = new List<Tag>();
    }
}
