using System;
using System.Collections.Generic;
using System.Text;

namespace Model.Entities
{
    public class ArticleBookmark
    {
        public Guid UserID { get; set; }
        public long ArticleID { get; set; }

    }
}
