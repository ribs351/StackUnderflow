using Model.Entities;
using Model.ViewModel.DiscussionViewModel;
using StackUnderflow.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StackUnderflow.Infrastructure.DiscussionService
{
    public interface IDiscussionService
    {
        DiscussionDetailsModel GetDiscussion(long id);
        Task<List<DiscussionIndexModel>> GetAllDiscussions(int pageNumber);
        Task<List<DiscussionIndexModel>> GetAllDiscussionsByViews();
        Task<List<DiscussionIndexModel>> GetAllDiscussionsByLatest(int take);
        Task<List<DiscussionIndexModel>> GetAllDiscussionsByTag(string tagID);
        Task<List<DiscussionIndexModel>> GetDiscussionsByName(string searchString);
        Task<List<DiscussionIndexModel>> GetTrendingDiscussions(int pageNumber);
        Task<List<DiscussionIndexModel>> GetSearchResultByTag(string tagName);
        Task<List<DiscussionIndexModel>> GetSearchResult(string searchString, string tagName);
        Task<List<DiscussionIndexModel>> GetAllDiscussionsFromFollows(Guid userID);
        Task<List<Tag>> ListTag(long discussionId);
        Task<bool> Create(DiscussionViewModel request);
        Task<bool> UpdateDiscussion(long discussionID, DiscussionViewModel request);
        bool CheckFollows(Guid userID, Guid followedUserID);
        bool CheckBookmarks(Guid userID, long discussionID);
        void RemoveDiscussionTag(string tagID, long discussionID);
        void RemoveDiscussion(long discussionID);
        Task<bool> SaveChangesAsync();
        Task<bool> IncreaseViewCount(long id);
        Task<bool> Follow(Guid userID, Guid followedUserID);
        Task<bool> UnFollow(Guid userID, Guid followedUserID);
        Task<List<DiscussionIndexModel>> GetAllBookmarks(Guid userID);
        Task<bool> Bookmark(long discussionID, Guid userID);
        Task<bool> Unbookmark(long discussionID);

    }
}
