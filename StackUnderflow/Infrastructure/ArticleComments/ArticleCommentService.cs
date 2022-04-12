using Microsoft.EntityFrameworkCore;
using Model.EF;
using Model.Entities;
using Model.ViewModel.Comment;
using StackUnderflow.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StackUnderflow.Infrastructure.ArticleComments
{
    public class ArticleCommentService : IArticleCommentService
    {
        private AppDbContext _ctx;
        public ArticleCommentService(AppDbContext ctx)
        {
            _ctx = ctx;
        }
        public async Task<bool> Create(ArticleCommentViewModel request)
        {
            var article = _ctx.Articles.FirstOrDefault(x => x.Id == request.ArticleID);
            var comment = new ArticleComment
            {
                Comment = request.Message,
                UserID = request.UserId,
                ArticleID = request.ArticleID,
                DatePosted = DateTime.Now
            };
            _ctx.ArticleComments.Add(comment);
            article.CommentCount++;
            await _ctx.SaveChangesAsync();
            return true;
        }

        public Task<bool> EditComment(ArticleCommentViewModel request)
        {
            throw new NotImplementedException();
        }

        public async Task<List<ArticleCommentModel>> GetAllCommentsByArticle(long articleID)
        {
            var query = from a in _ctx.Articles
                        join u in _ctx.Users on a.UserId equals u.Id
                        join ac in _ctx.ArticleComments on a.Id equals ac.ArticleID
                        where ac.ArticleID == articleID
                        select new { ac, u, a };

            var data = query.OrderByDescending(x => x.ac.DatePosted).Select(x => new ArticleCommentModel()
            {
                Id = x.ac.Id,
                ArticleId = x.a.Id,
                Message = x.ac.Comment,
                UserName = x.u.UserName,
                DatePosted = x.ac.DatePosted,
                Upvotes = x.ac.Upvotes

            }).ToListAsync();

            return await data;
        }

        public void RemoveComment(long id)
        {
            var comment = _ctx.ArticleComments.FirstOrDefault(a => a.Id == id);
            var article = _ctx.Articles.FirstOrDefault(b => b.Id == comment.ArticleID);
            article.CommentCount--;
            _ctx.ArticleComments.Remove(comment);
        }
    }
}
