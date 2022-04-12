using System;
using System.Collections.Generic;
using System.Text;

namespace Model.Entities
{
    public class ArticleComment
    {
        public long Id { get; set; }
        public Guid UserID { get; set; }
        public long ArticleID { get; set; }
        public long Upvotes { get; set; }
        public string Comment { get; set; }
        public DateTime DatePosted { get; set; }

    }
}
