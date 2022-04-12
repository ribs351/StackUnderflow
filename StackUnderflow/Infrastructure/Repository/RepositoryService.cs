using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Model.EF;
using Model.Entities;
using StackUnderflow.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Model.ViewModel.Repository;
using Model.ViewModel.Common;

namespace StackUnderflow.Infrastructure.Repository
{
    public class RepositoryService : IRepositoryService

    {

        private AppDbContext _ctx;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public RepositoryService(AppDbContext ctx, UserManager<User> userManger, SignInManager<User> signInManager, IHttpContextAccessor httpContextAccessor)
        {
            _ctx = ctx;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<ArticleIndexModel>> GetAllArticles(int pageNumber)
        {
            int pageSize = 5;
            int skipAmount = pageSize * (pageNumber - 1);
            

            var query = from ac in _ctx.Articles
                        join u in _ctx.Users on ac.UserId equals u.Id
                        select new { ac, u };

            var data = query.OrderByDescending(x => x.ac.DatePosted).Select(x => new ArticleIndexModel()
            {
                Id = x.ac.Id,
                ArticleName = x.ac.Title,
                UserName = x.u.UserName,
                DatePosted = x.ac.DatePosted,
                Upvotes = x.ac.Upvotes,
                CommentCount = x.ac.CommentCount,
                BookmarkCount = x.ac.BookmarkCount,
                ViewCount = x.ac.ViewCount
            }).Skip(skipAmount).Take(pageSize).ToListAsync();
                       

            return await data;
        }

        public async Task<List<ArticleIndexModel>> GetAllArticlesByLatest(int take)
        {
            

            var query = from ac in _ctx.Articles
                        join u in _ctx.Users on ac.UserId equals u.Id
                        select new { ac, u };

            var data = query.OrderByDescending(x => x.ac.DatePosted).Take(take).Select(x => new ArticleIndexModel()
            {
                Id = x.ac.Id,
                ArticleName = x.ac.Title,
                UserName = x.u.UserName,
                DatePosted = x.ac.DatePosted,
                Upvotes = x.ac.Upvotes,
                CommentCount = x.ac.CommentCount,
                BookmarkCount = x.ac.BookmarkCount,
                ViewCount = x.ac.ViewCount
            }).ToListAsync();


            return await data;
        }

        public async Task<List<ArticleIndexModel>> GetTrendingArticles(int pageNumber)
        {
            int pageSize = 5;
            int skipAmount = pageSize * (pageNumber - 1);

            var query = from ac in _ctx.Articles
                        join u in _ctx.Users on ac.UserId equals u.Id

                        select new { ac, u };

            var data = query.OrderByDescending(x => x.ac.ViewCount).ThenBy(x=>x.ac.BookmarkCount).ThenBy(x=>x.ac.CommentCount).Select(x => new ArticleIndexModel()
            {
                Id = x.ac.Id,
                ArticleName = x.ac.Title,
                UserName = x.u.UserName,
                DatePosted = x.ac.DatePosted,
                Upvotes = x.ac.Upvotes,
                CommentCount = x.ac.CommentCount,
                BookmarkCount = x.ac.BookmarkCount,
                ViewCount = x.ac.ViewCount
            }).Skip(skipAmount).Take(pageSize).ToListAsync();
            return await data;

        }

        public ArticleDetailsModel GetArticle(long id)
        {
            
            var query = from ac in _ctx.Articles
                        join u in _ctx.Users on ac.UserId equals u.Id

                        select new { ac, u };

            var data = query.Select(x => new ArticleDetailsModel()
            {
                Id = x.ac.Id,
                UserId = x.ac.UserId,
                Title = x.ac.Title,
                UserName = x.u.UserName,
                DatePosted = x.ac.DatePosted,
                Upvotes = x.ac.Upvotes,
                BookmarkCount = x.ac.BookmarkCount,
                ViewCount = x.ac.ViewCount,
                Text = x.ac.Text
            }).FirstOrDefault(x => x.Id == id);


            return data;
        }

        public void RemoveArticle(long id)
        {
            var article = _ctx.Articles.FirstOrDefault(a => a.Id == id);
            _ctx.Articles.Remove(article);
        }

        
        public async Task<bool> UpdateArticle(long articleID, RepositoryViewModel request)
        {
            var article = _ctx.Articles.FirstOrDefault(x => x.Id == articleID);
            if (article!= null)
            {
                article.Title = request.Title;
                article.Text = request.Text;
            };
            _ctx.Articles.Update(article);
            await _ctx.SaveChangesAsync();

            //handling tags
            if (!string.IsNullOrEmpty(request.NewTag))
            {
                string[] tags = request.NewTag.Split(',');
                foreach (var tag in tags)
                {
                    var tagId = StringHelper.ToUnsignString(tag);
                    var existedTag = this.CheckTag(tagId);
                    var existedArticleTag = this.CheckArticleTag(tagId);
                    //insert tags
                    if (!existedTag)
                    {
                        this.InsertTag(tagId, tag);
                    }
                    //insert articleTags
                    if (!existedArticleTag)
                    {
                        this.InsertArticleTag(article.Id, tagId);
                    }
                }
            }

            return true;
        }

        public async Task<bool> SaveChangesAsync()
        {
            if (await _ctx.SaveChangesAsync() > 0)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> Create(RepositoryViewModel request)
        {
            var article = new Article
            {
                Title = request.Title,
                Text = request.Text,
                UserId = request.UserId,
                DatePosted = DateTime.Now
            };

            _ctx.Articles.Add(article);
            await _ctx.SaveChangesAsync();

            //handling tags
            if (!string.IsNullOrEmpty(request.Tag))
            {
                string[] tags = request.Tag.Split(',');
                foreach (var tag in tags)
                {
                    var tagId = StringHelper.ToUnsignString(tag);
                    var existedTag = this.CheckTag(tagId);
                    //insert tags
                    if (!existedTag)
                    {
                        this.InsertTag(tagId, tag);
                    }
                    //insert articleTags
                    this.InsertArticleTag(article.Id, tagId);
                }
            }

            return true;
        }

        private void InsertTag(string id, string name)
        {
            var tag = new Tag()
            {
                Id = id,
                Name = name
            };
            _ctx.Tags.Add(tag);
            _ctx.SaveChanges();
        }
        public void RemoveArticleTag(string tagID, long articleID)
        {
            var tag = _ctx.ArticleTags.FirstOrDefault(a => a.TagID == tagID && a.ArticleID == articleID);
            _ctx.ArticleTags.Remove(tag);
            _ctx.SaveChanges();
        }
        private void RemoveAllArticleTags(long id)
        {
            _ctx.ArticleTags.RemoveRange(_ctx.ArticleTags.Where(x => x.ArticleID == id));
            _ctx.SaveChanges();
        }
        private void InsertArticleTag(long articleId, string tagId)
        {
            var articleTag = new ArticleTag()
            {
                ArticleID = articleId,
                TagID = tagId
            };
            _ctx.ArticleTags.Add(articleTag);
            _ctx.SaveChanges();
        }
        private bool CheckTag(string id)
        {
            return _ctx.Tags.Count(x => x.Id == id) > 0;
        }

        private bool CheckArticleTag(string id)
        {
            return _ctx.ArticleTags.Count(x => x.TagID == id) > 0;
        }

        public async Task<List<Tag>> ListTag(long articleId)
        {
            var query = await (from t in _ctx.Tags
                               join at in _ctx.ArticleTags on t.Id equals at.TagID
                               where at.ArticleID == articleId
                               select new
                               {
                                   ID = t.Id,
                                   Name = t.Name
                               }).Select(x => new Tag()
                               {
                                   Id = x.ID,
                                   Name = x.Name
                               }).ToListAsync();

            return query;
        }

        public async Task<bool> IncreaseViewCount(long id)
        {
            var article = _ctx.Articles.FirstOrDefault(a => a.Id == id);
            article.ViewCount++;
            await _ctx.SaveChangesAsync();
            return true;
        }

        public async Task<List<ArticleIndexModel>> GetAllArticlesByViews()
        {
            var query = from ac in _ctx.Articles
                        join u in _ctx.Users on ac.UserId equals u.Id
                        select new { ac, u };

            var data = query.OrderByDescending(x => x.ac.ViewCount).Select(x => new ArticleIndexModel()
            {
                Id = x.ac.Id,
                ArticleName = x.ac.Title,
                UserName = x.u.UserName,
                DatePosted = x.ac.DatePosted,
                Upvotes = x.ac.Upvotes,
                CommentCount = x.ac.CommentCount,
                BookmarkCount = x.ac.BookmarkCount,
                ViewCount = x.ac.ViewCount
            }).ToListAsync();


            return await data;
        }

        public async Task<List<ArticleIndexModel>> GetAllArticlesByTag(string tagID)
        {
            var query = from ac in _ctx.Articles
                        join u in _ctx.Users on ac.UserId equals u.Id
                        join at in _ctx.ArticleTags on ac.Id equals at.ArticleID
                        where at.TagID == tagID
                        select new { ac, u};

            var data = query.OrderByDescending(x => x.ac.DatePosted).Select(x => new ArticleIndexModel()
            {
                Id = x.ac.Id,
                ArticleName = x.ac.Title,
                UserName = x.u.UserName,
                DatePosted = x.ac.DatePosted,
                Upvotes = x.ac.Upvotes,
                CommentCount = x.ac.CommentCount,
                BookmarkCount = x.ac.BookmarkCount,
                ViewCount = x.ac.ViewCount
            }).ToListAsync();
            
            return await data;
        }

        public async Task<List<ArticleIndexModel>> GetAllBookmarks(Guid userID)
        {
            var query = from a in _ctx.Articles
                        join u in _ctx.Users on a.UserId equals u.Id
                        join ab in _ctx.ArticleBookmarks on a.Id equals ab.ArticleID
                        where ab.UserID == userID
                        select new { a, u };

            var data = query.OrderByDescending(x => x.a.DatePosted).Select(x => new ArticleIndexModel()
            {
                Id = x.a.Id,
                ArticleName = x.a.Title,
                UserName = x.u.UserName,
                DatePosted = x.a.DatePosted,
                Upvotes = x.a.Upvotes,
                CommentCount = x.a.CommentCount,
                BookmarkCount = x.a.BookmarkCount,
                ViewCount = x.a.ViewCount
            }).ToListAsync();

            return await data;
        }

        public async Task<bool> Bookmark(long articleID, Guid userID)
        {
            var article = _ctx.Articles.FirstOrDefault(x => x.Id == articleID);
            var bookmark = new ArticleBookmark
            {
                UserID = userID,
                ArticleID = articleID
            };
            _ctx.ArticleBookmarks.Add(bookmark);
            article.BookmarkCount++;
            await _ctx.SaveChangesAsync();
            return true;
        }
        public async Task<bool> UnBookmark(long articleID, Guid userID)
        {
            var article = _ctx.Articles.FirstOrDefault(x => x.Id == articleID);
            var bookmark = _ctx.ArticleBookmarks.FirstOrDefault(x => x.ArticleID == articleID && x.UserID == userID);
            _ctx.ArticleBookmarks.Remove(bookmark);
            article.BookmarkCount--;
            await _ctx.SaveChangesAsync();
            return true;
        }

        public async Task<List<ArticleIndexModel>> GetArticleByName(string searchString)
        {
            var query = from ac in _ctx.Articles
                        join u in _ctx.Users on ac.UserId equals u.Id
                        select new { ac, u };

            var data = query.OrderByDescending(x => x.ac.ViewCount).Select(x => new ArticleIndexModel()
            {
                Id = x.ac.Id,
                ArticleName = x.ac.Title,
                UserName = x.u.UserName,
                DatePosted = x.ac.DatePosted,
                Upvotes = x.ac.Upvotes,
                CommentCount = x.ac.CommentCount,
                BookmarkCount = x.ac.BookmarkCount,
                ViewCount = x.ac.ViewCount
            }).Where(x => x.ArticleName.Contains(searchString)).ToListAsync();

            return await data;
        }

        public async Task<List<ArticleIndexModel>> GetSearchResult(string searchString, string tagName)
        {
            var query = from ac in _ctx.Articles
                        join u in _ctx.Users on ac.UserId equals u.Id
                        join at in _ctx.ArticleTags on ac.Id equals at.ArticleID
                        join t in _ctx.Tags on at.TagID equals t.Id
                        where t.Name.Contains(tagName)
                        select new { ac, u };

            var data = query.OrderByDescending(x => x.ac.ViewCount).Select(x => new ArticleIndexModel()
            {
                Id = x.ac.Id,
                ArticleName = x.ac.Title,
                UserName = x.u.UserName,
                DatePosted = x.ac.DatePosted,
                Upvotes = x.ac.Upvotes,
                CommentCount = x.ac.CommentCount,
                BookmarkCount = x.ac.BookmarkCount,
                ViewCount = x.ac.ViewCount
            }).Where(x => x.ArticleName.Contains(searchString)).ToListAsync();

            return await data;
        }

        public async Task<List<ArticleIndexModel>> GetSearchResultByTag(string tagName)
        {
            var query = from ac in _ctx.Articles
                        join u in _ctx.Users on ac.UserId equals u.Id
                        join at in _ctx.ArticleTags on ac.Id equals at.ArticleID
                        join t in _ctx.Tags on at.TagID equals t.Id
                        where t.Name.Contains(tagName)
                        select new { ac, u };

            var data = query.OrderByDescending(x => x.ac.ViewCount).Select(x => new ArticleIndexModel()
            {
                Id = x.ac.Id,
                ArticleName = x.ac.Title,
                UserName = x.u.UserName,
                DatePosted = x.ac.DatePosted,
                Upvotes = x.ac.Upvotes,
                CommentCount = x.ac.CommentCount,
                BookmarkCount = x.ac.BookmarkCount,
                ViewCount = x.ac.ViewCount
            }).ToListAsync();

            return await data;
        }

        public async Task<bool> Follow(Guid userID, Guid followedUserID)
        {
            var follow = new UserFollow
            {
                UserID = userID,
                FollowedUserID = followedUserID
            };
            _ctx.UserFollows.Add(follow);
            await _ctx.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnFollow(Guid userID, Guid followedUserID)
        {
            var follow = _ctx.UserFollows.FirstOrDefault(x => x.UserID == userID && x.FollowedUserID == followedUserID);
            _ctx.UserFollows.Remove(follow);
            await _ctx.SaveChangesAsync();
            return true;
        }
        public async Task<List<ArticleIndexModel>> GetAllArticlesFromFollows(Guid userID)
        {
            var query = from ac in _ctx.Articles
                        join u in _ctx.Users on ac.UserId equals u.Id
                        join fu in _ctx.UserFollows on ac.UserId equals fu.FollowedUserID
                        where fu.UserID == userID
                        select new { ac, u };

            var data = query.OrderByDescending(x => x.ac.DatePosted).Select(x => new ArticleIndexModel()
            {
                Id = x.ac.Id,
                ArticleName = x.ac.Title,
                UserName = x.u.UserName,
                DatePosted = x.ac.DatePosted,
                Upvotes = x.ac.Upvotes,
                CommentCount = x.ac.CommentCount,
                BookmarkCount = x.ac.BookmarkCount,
                ViewCount = x.ac.ViewCount
            }).ToListAsync();


            return await data;
        }

        public bool CheckFollows(Guid userID, Guid followedUserID)
        {
            return _ctx.UserFollows.Count(x => x.UserID == userID && x.FollowedUserID == followedUserID) > 0;
        }

        public bool CheckBookmarks(Guid userID, long articleID)
        {
            return _ctx.ArticleBookmarks.Count(x => x.UserID == userID && x.ArticleID == articleID) > 0;
        }

        public async Task<List<ArticleDetailsModel>> GetAllFollowedUsers(Guid userID)
        {
            var query = from u in _ctx.Users 
                        join fu in _ctx.UserFollows on u.Id equals fu.FollowedUserID
                        where fu.UserID == userID
                        select new { u, fu };
            var data = query.Select(x => new ArticleDetailsModel()
            {
               UserName = x.u.UserName,
               UserId = x.u.Id
            }).ToListAsync();

            return await data;
        }
    }
}
