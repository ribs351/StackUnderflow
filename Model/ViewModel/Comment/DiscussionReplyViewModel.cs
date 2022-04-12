using System;
using System.Collections.Generic;
using System.Text;

namespace Model.ViewModel.Comment
{
    public class DiscussionReplyViewModel
    {
        public long DiscussionID { get; set; }
        public Guid UserId { get; set; }
        public DateTime DatePosted { get; set; }
        public bool Approved { get; set; }
        public string Text { get; set; }
    }
}
