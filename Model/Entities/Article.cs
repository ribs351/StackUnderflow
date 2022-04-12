using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Model.Entities
{
    public class Article
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public DateTime DatePosted { get; set; }
        public long Upvotes { get; set; }
        public bool Hidden { get; set; }
        public long BookmarkCount { get; set; }
        public long ViewCount { get; set; }
        public long CommentCount { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
}
}
