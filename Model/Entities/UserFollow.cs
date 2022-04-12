using System;
using System.Collections.Generic;
using System.Text;

namespace Model.Entities
{
    public class UserFollow
    {
        public Guid UserID { get; set; }
        public Guid FollowedUserID { get; set; }
    }
}
