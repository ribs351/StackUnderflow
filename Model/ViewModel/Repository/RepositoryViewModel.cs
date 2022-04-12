using System;
using System.Collections.Generic;
using System.Text;

namespace Model.ViewModel.Repository
{
    public class RepositoryViewModel
    {
        public string Title { get; set; }
        public string Text { get; set; }
        public string Tag { get; set; }
        public string NewTag { get; set; }
        public Guid UserId { get; set; }
        public DateTime DatePosted { get; set; }
    }
}
