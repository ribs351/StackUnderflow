using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Model.EF;
using Model.Entities;
using Model.ViewModel.Common;
using Model.ViewModel.DiscussionViewModel;
using StackUnderflow.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StackUnderflow.Infrastructure.DiscussionService
{
    public class DiscussionService : IDiscussionService
    {
        private AppDbContext _ctx;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public DiscussionService(AppDbContext ctx, UserManager<User> userManger, SignInManager<User> signInManager, IHttpContextAccessor httpContextAccessor)
        {
            _ctx = ctx;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<bool> Create(DiscussionViewModel request)
        {
            var discussion = new Discussion
            {
                Title = request.Title,
                Text = request.Text,
                UserID = request.UserId,
                DatePosted = DateTime.Now
            };

            _ctx.Discussions.Add(discussion);
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
                    //insert discussionTags
                    this.InsertDicussionTag(discussion.Id, tagId);
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
        private void InsertDicussionTag(long dicussionId, string tagId)
        {
            var discussionTag = new DiscussionTag()
            {
                DiscussionID = dicussionId,
                TagID = tagId
            };
            _ctx.DiscussionTags.Add(discussionTag);
            _ctx.SaveChanges();
        }
        private bool CheckTag(string id)
        {
            return _ctx.Tags.Count(x => x.Id == id) > 0;
        }
        private bool CheckDiscussionTag(string id)
        {
            return _ctx.DiscussionTags.Count(x => x.TagID == id) > 0;
        }

        public async Task<List<DiscussionIndexModel>> GetAllDiscussions(int pageNumber)
        {
            int pageSize = 5;
            int skipAmount = pageSize * (pageNumber - 1);

            var query = from d in _ctx.Discussions
                        join u in _ctx.Users on d.UserID equals u.Id
                        select new { d, u };

            var data = query.OrderByDescending(x => x.d.DatePosted).Select(x => new DiscussionIndexModel()
            {
                Id = x.d.Id,
                DiscussionName = x.d.Title,
                UserName = x.u.UserName,
                DatePosted = x.d.DatePosted,
                Upvotes = x.d.Upvotes,
                BookmarkCount = x.d.BookmarkCount,
                ViewCount = x.d.ViewCount
            }).Skip(skipAmount).Take(pageSize).ToListAsync();


            return await data;
        }

        public async Task<List<DiscussionIndexModel>> GetAllDiscussionsByLatest(int take)
        {
            var query = from d in _ctx.Discussions
                        join u in _ctx.Users on d.UserID equals u.Id
                        select new { d, u };

            var data = query.OrderByDescending(x => x.d.DatePosted).Take(take).Select(x => new DiscussionIndexModel()
            {
                Id = x.d.Id,
                DiscussionName = x.d.Title,
                UserName = x.u.UserName,
                DatePosted = x.d.DatePosted,
                Upvotes = x.d.Upvotes,
                BookmarkCount = x.d.BookmarkCount,
                ViewCount = x.d.ViewCount
            }).ToListAsync();


            return await data;
        }

        public async Task<List<Tag>> ListTag(long discussionId)
        {
            var query = await(from t in _ctx.Tags
                              join dt in _ctx.DiscussionTags on t.Id equals dt.TagID
                              where dt.DiscussionID == discussionId
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

        public async Task<bool> SaveChangesAsync()
        {
            if (await _ctx.SaveChangesAsync() > 0)
            {
                return true;
            }
            return false;
        }

        public DiscussionDetailsModel GetDiscussion(long id)
        {
            var query = from d in _ctx.Discussions
                        join u in _ctx.Users on d.UserID equals u.Id
                        select new { d, u };

            var data = query.Select(x => new DiscussionDetailsModel()
            {
                Id = x.d.Id,
                UserId = x.d.UserID,
                Title = x.d.Title,
                Text = x.d.Text,
                UserName = x.u.UserName,
                DatePosted = x.d.DatePosted,
                Upvotes = x.d.Upvotes,
                BookmarkCount = x.d.BookmarkCount,
                ViewCount = x.d.ViewCount
            }).FirstOrDefault(x => x.Id == id);

            return data;
        }
        public async Task<List<DiscussionIndexModel>> GetAllDiscussionsByViews()
        {
            var query = from d in _ctx.Discussions
                        join u in _ctx.Users on d.UserID equals u.Id
                        select new { d, u };

            var data = query.OrderByDescending(x => x.d.ViewCount).Select(x => new DiscussionIndexModel()
            {
                Id = x.d.Id,
                DiscussionName = x.d.Title,
                UserName = x.u.UserName,
                DatePosted = x.d.DatePosted,
                Upvotes = x.d.Upvotes,
                BookmarkCount = x.d.BookmarkCount,
                ViewCount = x.d.ViewCount
            }).ToListAsync();


            return await data;
        }

        public async Task<bool> IncreaseViewCount(long id)
        {
            var discussion = _ctx.Discussions.FirstOrDefault(a => a.Id == id);
            discussion.ViewCount++;
            await _ctx.SaveChangesAsync();
            return true;
        }

        public async Task<List<DiscussionIndexModel>> GetDiscussionsByName(string searchString)
        {
            var query = from d in _ctx.Discussions
                        join u in _ctx.Users on d.UserID equals u.Id
                        select new { d, u };

            var data = query.OrderByDescending(x => x.d.ViewCount).Select(x => new DiscussionIndexModel()
            {
                Id = x.d.Id,
                DiscussionName = x.d.Title,
                UserName = x.u.UserName,
                DatePosted = x.d.DatePosted,
                Upvotes = x.d.Upvotes,
                BookmarkCount = x.d.BookmarkCount,
                ViewCount = x.d.ViewCount
            }).Where(x => x.DiscussionName.Contains(searchString)).ToListAsync();

            return await data;
        }
        public async Task<List<DiscussionIndexModel>> GetSearchResult(string searchString, string tagName)
        {
            var query = from d in _ctx.Discussions
                        join u in _ctx.Users on d.UserID equals u.Id
                        join dt in _ctx.DiscussionTags on d.Id equals dt.DiscussionID
                        join t in _ctx.Tags on dt.TagID equals t.Id
                        where t.Name.Contains(tagName)
                        select new { d, u };

            var data = query.OrderByDescending(x => x.d.ViewCount).Select(x => new DiscussionIndexModel()
            {
                Id = x.d.Id,
                DiscussionName = x.d.Title,
                UserName = x.u.UserName,
                DatePosted = x.d.DatePosted,
                Upvotes = x.d.Upvotes,
                BookmarkCount = x.d.BookmarkCount,
                ViewCount = x.d.ViewCount
            }).Where(x => x.DiscussionName.Contains(searchString)).ToListAsync();

            return await data;
        }

        public async Task<bool> UpdateDiscussion(long discussionID, DiscussionViewModel request)
        {
            var discussion = _ctx.Discussions.FirstOrDefault(x => x.Id == discussionID);
            if (discussion != null)
            {
                discussion.Title = request.Title;
                discussion.Text = request.Text;
            }

            _ctx.Discussions.Update(discussion);
            await _ctx.SaveChangesAsync();

            //handling tags
            if (!string.IsNullOrEmpty(request.NewTag))
            {
                string[] tags = request.NewTag.Split(',');
                foreach (var tag in tags)
                {
                    var tagId = StringHelper.ToUnsignString(tag);
                    var existedTag = this.CheckTag(tagId);
                    var existedArticleTag = this.CheckDiscussionTag(tagId);
                    //insert tags
                    if (!existedTag)
                    {
                        this.InsertTag(tagId, tag);
                    }
                    //insert discussionTags
                    if (!existedArticleTag)
                    {
                        this.InsertDicussionTag(discussion.Id, tagId);
                    }
                    
                }
            }

            return true;
        }

        public void RemoveDiscussionTag(string tagID, long discussionID)
        {
            var tag = _ctx.DiscussionTags.FirstOrDefault(x => x.TagID == tagID && x.DiscussionID == discussionID);
            _ctx.DiscussionTags.Remove(tag);
            _ctx.SaveChanges();
        }

        public void RemoveDiscussion(long discussionID)
        {
            var discussion = _ctx.Discussions.FirstOrDefault(x => x.Id == discussionID);
            _ctx.Discussions.Remove(discussion);
        }
        public async Task<List<DiscussionIndexModel>> GetAllBookmarks(Guid userID)
        {
            var query = from d in _ctx.Discussions
                        join u in _ctx.Users on d.UserID equals u.Id
                        join db in _ctx.DiscussionBookmarks on d.Id equals db.DiscussionID
                        where db.UserID == userID
                        select new { d, u };

            var data = query.OrderByDescending(x => x.d.DatePosted).Select(x => new DiscussionIndexModel()
            {
                Id = x.d.Id,
                DiscussionName = x.d.Title,
                UserName = x.u.UserName,
                DatePosted = x.d.DatePosted,
                Upvotes = x.d.Upvotes,
                BookmarkCount = x.d.BookmarkCount,
                ViewCount = x.d.ViewCount
            }).ToListAsync();


            return await data;
        }
        public async Task<bool> Bookmark(long discussionID, Guid userID)
        {
            var discussion = _ctx.Discussions.FirstOrDefault(x =>x.Id == discussionID);
            var bookmark = new DiscussionBookmark
            {
                UserID = userID,
                DiscussionID = discussionID
            };
            _ctx.DiscussionBookmarks.Add(bookmark);
            discussion.BookmarkCount++;
            await _ctx.SaveChangesAsync();
            return true;
        }
        public async Task<bool> Unbookmark(long discussionID)
        {
            var discussion = _ctx.Discussions.FirstOrDefault(x => x.Id == discussionID);
            var bookmark = _ctx.DiscussionBookmarks.FirstOrDefault(x => x.DiscussionID == discussionID);
            _ctx.DiscussionBookmarks.Remove(bookmark);
            discussion.BookmarkCount--;
            await _ctx.SaveChangesAsync();
            return true;
        }

        public async Task<List<DiscussionIndexModel>> GetSearchResultByTag(string tagName)
        {
            var query = from d in _ctx.Discussions
                        join u in _ctx.Users on d.UserID equals u.Id
                        join dt in _ctx.DiscussionTags on d.Id equals dt.DiscussionID
                        join t in _ctx.Tags on dt.TagID equals t.Id
                        where t.Name.Contains(tagName)
                        select new { d, u };

            var data = query.OrderByDescending(x => x.d.ViewCount).Select(x => new DiscussionIndexModel()
            {
                Id = x.d.Id,
                DiscussionName = x.d.Title,
                UserName = x.u.UserName,
                DatePosted = x.d.DatePosted,
                Upvotes = x.d.Upvotes,
                BookmarkCount = x.d.BookmarkCount,
                ViewCount = x.d.ViewCount
            }).ToListAsync();

            return await data;
        }

        public async Task<List<DiscussionIndexModel>> GetAllDiscussionsFromFollows(Guid userID)
        {
            var query = from d in _ctx.Discussions
                        join u in _ctx.Users on d.UserID equals u.Id
                        join fu in _ctx.UserFollows on d.UserID equals fu.FollowedUserID
                        where fu.UserID == userID
                        select new { d, u };

            var data = query.OrderByDescending(x => x.d.DatePosted).Select(x => new DiscussionIndexModel()
            {
                Id = x.d.Id,
                DiscussionName = x.d.Title,
                UserName = x.u.UserName,
                DatePosted = x.d.DatePosted,
                Upvotes = x.d.Upvotes,
                BookmarkCount = x.d.BookmarkCount,
                ViewCount = x.d.ViewCount
            }).ToListAsync();

            return await data;
        }

        public bool CheckFollows(Guid userID, Guid followedUserID)
        {
            return _ctx.UserFollows.Count(x => x.UserID == userID && x.FollowedUserID == followedUserID) > 0;
        }

        public bool CheckBookmarks(Guid userID, long discussionID)
        {
            return _ctx.DiscussionBookmarks.Count(x => x.UserID == userID && x.DiscussionID == discussionID) > 0;
        }

        public async Task<List<DiscussionIndexModel>> GetAllDiscussionsByTag(string tagID)
        {
            var query = from d in _ctx.Discussions
                        join u in _ctx.Users on d.UserID equals u.Id
                        join dt in _ctx.DiscussionTags on d.Id equals dt.DiscussionID
                        where dt.TagID == tagID
                        select new { d, u };

            var data = query.OrderByDescending(x => x.d.DatePosted).Select(x => new DiscussionIndexModel()
            {
                Id = x.d.Id,
                DiscussionName = x.d.Title,
                UserName = x.u.UserName,
                DatePosted = x.d.DatePosted,
                Upvotes = x.d.Upvotes,
                BookmarkCount = x.d.BookmarkCount,
                ViewCount = x.d.ViewCount
            }).ToListAsync();

            return await data;
        }

        public async Task<List<DiscussionIndexModel>> GetTrendingDiscussions(int pageNumber)
        {
            var query = from d in _ctx.Discussions
                        join u in _ctx.Users on d.UserID equals u.Id
                        select new { d, u };

            var data = query.OrderByDescending(x => x.d.ViewCount).ThenBy(x =>x.d.BookmarkCount).Select(x => new DiscussionIndexModel()
            {
                Id = x.d.Id,
                DiscussionName = x.d.Title,
                UserName = x.u.UserName,
                DatePosted = x.d.DatePosted,
                Upvotes = x.d.Upvotes,
                BookmarkCount = x.d.BookmarkCount,
                ViewCount = x.d.ViewCount
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
    }
}
