using Model.ViewModel.Comment;
using StackUnderflow.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StackUnderflow.Infrastructure.DicussionComments
{
    public interface IDiscussionCommentService
    {
        Task<List<DiscussionReplyModel>> GetAllRepliesByArticle(long discussionID);
        Task<bool> CreateReply(DiscussionReplyViewModel request);
        Task<bool> EditReply(DiscussionReplyViewModel request);
        Task<bool> Approve(long id);
        Task<bool> Disapprove(long id);
        void RemoveReply(long id);
        Task<bool> SaveChangesAsync();
    }
}
