using Model.ViewModel.Comment;
using StackUnderflow.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StackUnderflow.Infrastructure.ArticleComments
{
    public interface IArticleCommentService
    {
        Task<List<ArticleCommentModel>> GetAllCommentsByArticle(long articleID);
        Task<bool> Create(ArticleCommentViewModel request);
        Task<bool> EditComment(ArticleCommentViewModel request);
        void RemoveComment(long id);

    }
}
