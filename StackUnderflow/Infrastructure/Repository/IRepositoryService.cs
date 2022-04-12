using Model.Entities;
using Model.ViewModel.Repository;
using StackUnderflow.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StackUnderflow.Infrastructure.Repository
{
    public interface IRepositoryService
    {
        ArticleDetailsModel GetArticle(long id);
        Task<List<ArticleIndexModel>> GetAllArticles(int pageNumber);
        Task<List<ArticleIndexModel>> GetAllArticlesByLatest(int take);
        Task<List<ArticleIndexModel>> GetAllArticlesByViews();
        Task<List<ArticleIndexModel>> GetAllArticlesByTag(string tagID);
        Task<List<ArticleIndexModel>> GetAllBookmarks(Guid userID);
        Task<List<ArticleIndexModel>> GetTrendingArticles(int pageNumber);
        Task<List<ArticleIndexModel>> GetArticleByName(string searchString);
        Task<List<ArticleIndexModel>> GetSearchResultByTag(string tagName);
        Task<List<ArticleIndexModel>> GetSearchResult(string searchString, string tagName);
        Task<List<ArticleIndexModel>> GetAllArticlesFromFollows(Guid userID);
        Task<List<ArticleDetailsModel>> GetAllFollowedUsers(Guid userID);
        Task<List<Tag>> ListTag(long articleId);
        bool CheckFollows(Guid userID, Guid followedUserID);
        bool CheckBookmarks(Guid userID, long articleID);
        Task<bool> Bookmark(long articleID, Guid userID);
        Task <bool> UnBookmark(long articleID, Guid userID);
        Task<bool> Follow(Guid userID, Guid followedUserID);
        Task<bool> UnFollow(Guid userID, Guid followedUserID);
        Task<bool> Create(RepositoryViewModel request);
        Task<bool> UpdateArticle(long articleID, RepositoryViewModel request);
        Task<bool> IncreaseViewCount(long id);
        void RemoveArticleTag(string tagID, long articleID);
        void RemoveArticle(long id);
        Task<bool> SaveChangesAsync();
    }
}
