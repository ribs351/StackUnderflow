using System;
using System.Collections.Generic;
using System.Text;

namespace Model.ViewModel.DiscussionViewModel
{
    public class DiscussionViewModel
    {
        public string Title { get; set; }
        public string Text { get; set; }
        public string Tag { get; set; }
        public Guid UserId { get; set; }
        public string NewTag { get; set; }
        public DateTime DatePosted { get; set; }
    }
}
