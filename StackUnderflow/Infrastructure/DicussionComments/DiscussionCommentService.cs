using Microsoft.EntityFrameworkCore;
using Model.EF;
using Model.Entities;
using Model.ViewModel.Comment;
using StackUnderflow.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StackUnderflow.Infrastructure.DicussionComments
{
    public class DiscussionCommentService : IDiscussionCommentService
    {
        private AppDbContext _ctx;
        public DiscussionCommentService(AppDbContext ctx) 
        {
            _ctx = ctx;
        }

        public async Task<bool> Approve(long id)
        {
            var reply = _ctx.DiscussionReplies.FirstOrDefault(a => a.Id == id);
            if (reply != null)
            {
                reply.Approved = true;
            };
            _ctx.DiscussionReplies.Update(reply);
            await _ctx.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Disapprove(long id)
        {
            var reply = _ctx.DiscussionReplies.FirstOrDefault(a => a.Id == id);
            if (reply != null)
            {
                reply.Approved = false;
            };
            _ctx.DiscussionReplies.Update(reply);
            await _ctx.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CreateReply(DiscussionReplyViewModel request)
        {
            var discussion = _ctx.Discussions.FirstOrDefault(x => x.Id == request.DiscussionID);
            var reply = new DiscussionReply
            {
                Text = request.Text,
                DiscussionID = request.DiscussionID,
                UserID = request.UserId,
                DatePosted = DateTime.Now,
                Approved = false
            };
            _ctx.DiscussionReplies.Add(reply);
            discussion.CommentCount++;
            await _ctx.SaveChangesAsync();
            return true;
        }

        public Task<bool> EditReply(DiscussionReplyViewModel request)
        {
            throw new NotImplementedException();
        }

        public async Task<List<DiscussionReplyModel>> GetAllRepliesByArticle(long discussionID)
        {
            var query = from d in _ctx.Discussions
                        join u in _ctx.Users on d.UserID equals u.Id
                        join dr in _ctx.DiscussionReplies on d.Id equals dr.DiscussionID
                        where dr.DiscussionID == discussionID
                        select new { d, u, dr };
            var data = query.OrderByDescending(x => x.dr.DatePosted).Select(x => new DiscussionReplyModel()
            {
                Id = x.dr.Id,
                DiscussionId = x.d.Id,
                Text = x.dr.Text,
                UserName = x.u.UserName,
                DatePosted = x.dr.DatePosted,
                Approved = x.dr.Approved,
                Upvotes = x.dr.Upvotes

            }).ToListAsync();

            return await data;
        }

        public void RemoveReply(long id)
        {
            var reply = _ctx.DiscussionReplies.FirstOrDefault(a => a.Id == id);
            var discussion = _ctx.Discussions.FirstOrDefault(b => b.Id == reply.DiscussionID);
            discussion.CommentCount--;
            _ctx.DiscussionReplies.Remove(reply);
        }
        public async Task<bool> SaveChangesAsync()
        {
            if (await _ctx.SaveChangesAsync() > 0)
            {
                return true;
            }
            return false;
        }
    }
}
