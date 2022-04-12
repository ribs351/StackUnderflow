using System;
using System.Collections.Generic;
using System.Text;

namespace Model.ViewModel.Comment
{
    public class ArticleCommentViewModel
    {
        public long ArticleID { get; set; }
        public Guid UserId { get; set; }
        public DateTime DatePosted { get; set; }
        public string Message { get; set; }
    }
}
