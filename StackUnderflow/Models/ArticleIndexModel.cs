using Model.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StackUnderflow.Models
{
    public class ArticleIndexModel
    {
        public long Id { get; set; }
        public string ArticleName { get; set; }
        public string UserName { get; set; }
        public DateTime DatePosted { get; set; }
        public long Upvotes { get; set; }
        public long BookmarkCount { get; set; }
        public long ViewCount { get; set; }
        public long CommentCount { get; set; }
        public List<Tag> Tags { get; set; } = new List<Tag>();
    }
}
