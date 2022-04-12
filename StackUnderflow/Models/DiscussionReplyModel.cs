using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StackUnderflow.Models
{
    public class DiscussionReplyModel
    {
        public long Id { get; set; }
        public long DiscussionId { get; set; }
        public string Text { get; set; }
        public string UserName { get; set; }
        public DateTime DatePosted { get; set; }
        public bool Approved { get; set; }
        public long Upvotes { get; set; }
    }
}
