using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StackUnderflow.Models
{
    public class ArticleCommentModel
    {
        public long Id { get; set; }
        public long ArticleId { get; set; }
        public string Message { get; set; }
        public string UserName { get; set; }
        public DateTime DatePosted { get; set; }
        public long Upvotes { get; set; }
    }
}
