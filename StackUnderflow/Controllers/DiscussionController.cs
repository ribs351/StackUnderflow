using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model.EF;
using Model.ViewModel.Comment;
using Model.ViewModel.DiscussionViewModel;
using StackUnderflow.Infrastructure.DicussionComments;
using StackUnderflow.Infrastructure.DiscussionService;
using StackUnderflow.Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StackUnderflow.Controllers
{
    public class DiscussionController : Controller
    {
        private IDiscussionService _disc;
        private readonly IWebHostEnvironment _env;
        private IDiscussionCommentService _repcomm;
        private IRepositoryService _repo;
        private AppDbContext _ctx;
        public DiscussionController(AppDbContext ctx, IDiscussionService disc, IWebHostEnvironment env, IDiscussionCommentService repcomm, IRepositoryService repo)
        {
            _disc = disc;
            _env = env;
            _repcomm = repcomm;
            _ctx = ctx;
            _repo = repo;
        }
        [Authorize]
        [HttpGet]
        public IActionResult Publish()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Publish(DiscussionViewModel request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            request.UserId = new Guid(userId);

            var result = await _disc.Create(request);
            if (result == true)
            {
                return RedirectToAction("AllDiscussions", "Discussion");
            }
            else
                return RedirectToAction("Error");
        }
        public async Task<IActionResult> DiscussionByTag (string id)
        {
            var discussions = await _disc.GetAllDiscussionsByTag(id);
            var tempTag = _ctx.Tags.FirstOrDefault(x => x.Id == id);
            TempData["TagName"] = tempTag.Name;
            foreach (var item in discussions)
            {
                var tag = await _disc.ListTag(item.Id);
                if (tag != null)
                {
                    item.Tags = tag;
                }
            }

            return View(discussions);
        }

        public async Task<IActionResult> Discussion(long id)
        {
            var discussion = _disc.GetDiscussion(id);
            var tag = await _disc.ListTag(discussion.Id);
            var isAuthenticated = User.Identity.IsAuthenticated;
            if (isAuthenticated == true)
            {
                var userID = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = new Guid(userID);
                var isFollowed = _disc.CheckFollows(user, discussion.UserId);
                var isBookmarked = _disc.CheckBookmarks(user, id);
                var isOwner = user == discussion.UserId;

                if (isOwner == true)
                {
                    discussion.Owner = true;
                }
                if (isFollowed == true)
                {
                    discussion.Followed = true;
                }
                if (isBookmarked == true)
                {
                    discussion.Bookmarked = true;
                }
            }

            if (tag != null)
            {
                discussion.Tags = tag;
            }

            ViewBag.DiscussionReplies = await _repcomm.GetAllRepliesByArticle(id);
            await _disc.IncreaseViewCount(id);
            return View(discussion);
        }
        [Authorize]
        public IActionResult RemoveTag(string tagID, long discussionID)
        {
            _disc.RemoveDiscussionTag(tagID, discussionID);
            return RedirectToAction("Edit", "Discussion", new { @id = discussionID });
        }

        [Authorize]
        public async Task<IActionResult> Approve(long id, long discussionID)
        {
            await _repcomm.Approve(id);
            return RedirectToAction("Discussion", "Discussion", new { @id = discussionID });
        }

        [Authorize]
        public async Task<IActionResult> Disapprove(long id, long discussionID)
        {
            await _repcomm.Disapprove(id);
            return RedirectToAction("Discussion", "Discussion", new { @id = discussionID });
        }

        [Authorize]
        public async Task<IActionResult> RemoveReply(long id, long discussionID)
        {
            _repcomm.RemoveReply(id);
            await _repcomm.SaveChangesAsync();
            return RedirectToAction("Discussion", "Discussion", new { @id = discussionID });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Remove(long id)
        {
            _disc.RemoveDiscussion(id);
            await _disc.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            var discussion = _disc.GetDiscussion(id);
            var tag = await _disc.ListTag(discussion.Id);
            if (tag != null)
            {
                discussion.Tags = tag;
            }
            return View(discussion);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(long Id, DiscussionViewModel request)
        {
            var result = await _disc.UpdateDiscussion(Id, request);
            if(result == true)
            {
                return RedirectToAction("Discussion", "Discussion", new {@id = Id });
            }
            else
                return RedirectToAction("Error");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Reply(DiscussionReplyViewModel request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            request.UserId = new Guid(userId);
            var result = await _repcomm.CreateReply(request);
            if (result == true)
            {
                // refresh the page lol
                return RedirectToAction("Discussion", "Discussion", new { @id = request.DiscussionID });
            }
            else
                return RedirectToAction("Error");
        }
        public async Task<IActionResult> AllDiscussions(int pageNumber)
        {
            if (pageNumber < 1)
                return RedirectToAction("AllDiscussions", new { pageNumber = 1 });

            int pageSize = 5;
            int skipAmount = pageSize * (pageNumber - 1);
            int discussionCount = _ctx.Discussions.Count();
            int capacity = skipAmount + pageSize;
            //normal calculations will round this number down, ie 11/5 = 2, meaning you missed a page
            //ceiling rounds this up a number, but you need a double for the calculation, so you convert one of the numbers into a double
            //and then cast the entire calculation to an int and bam
            int pageCount = (int)Math.Ceiling((double)discussionCount / pageSize);

            bool nextPage = discussionCount > capacity;

            var discussions = await _disc.GetAllDiscussions(pageNumber);

            foreach (var item in discussions)
            {
                var tag = await _disc.ListTag(item.Id);
                if (tag != null)
                {
                    item.Tags = tag;
                }
            }
            var articles = await _repo.GetAllArticlesByLatest(3);
            foreach (var item in articles)
            {
                var tag = await _disc.ListTag(item.Id);
                if (tag != null)
                {
                    item.Tags = tag;
                }
            }
            ViewBag.PageCount = pageCount;
            ViewBag.NextPage = nextPage;
            ViewBag.PageNumber = pageNumber;
            ViewBag.LastestArticles = articles;

            return View(discussions);
        }
        public async Task<IActionResult> Trending(int pageNumber)
        {
            if (pageNumber < 1)
                return RedirectToAction("Trending", new { pageNumber = 1 });

            int pageSize = 5;
            int skipAmount = pageSize * (pageNumber - 1);
            int discussionCount = _ctx.Discussions.Count();
            int capacity = skipAmount + pageSize;
            //normal calculations will round this number down, ie 11/5 = 2, meaning you missed a page
            //ceiling rounds this up a number, but you need a double for the calculation, so you convert one of the numbers into a double
            //and then cast the entire calculation to an int and bam
            int pageCount = (int)Math.Ceiling((double)discussionCount / pageSize);

            bool nextPage = discussionCount > capacity;

            var discussions = await _disc.GetTrendingDiscussions(pageNumber);
            foreach (var item in discussions)
            {
                var tag = await _disc.ListTag(item.Id);
                if (tag != null)
                {
                    item.Tags = tag;
                }
            }
            ViewBag.PageCount = pageCount;
            ViewBag.NextPage = nextPage;
            ViewBag.PageNumber = pageNumber;
            return View(discussions);
        }

        public async Task<IActionResult> SearchResult(string searchString, string tagName)
        {
            IQueryable<string> discussionTagQuery = from dt in _ctx.DiscussionTags
                                                    join t in _ctx.Tags on dt.TagID equals t.Id
                                                    orderby dt.TagID
                                                    select t.Name;
            if (searchString == null)
            {
                var discussions = await _disc.GetSearchResultByTag(tagName);
                foreach (var item in discussions)
                {
                    var tag = await _disc.ListTag(item.Id);
                    if (tag != null)
                    {
                        item.Tags = tag;
                    }
                }

                return View(discussions);
            }
            if (tagName == null)
            {
                var discussions = await _disc.GetDiscussionsByName(searchString);
                foreach (var item in discussions)
                {
                    var tag = await _disc.ListTag(item.Id);
                    if (tag != null)
                    {
                        item.Tags = tag;
                    }
                }

                return View(discussions);
            }
            if (!discussionTagQuery.Contains(tagName))
            {
                var discussions = await _disc.GetDiscussionsByName(searchString);
                foreach (var item in discussions)
                {
                    var tag = await _disc.ListTag(item.Id);
                    if (tag != null)
                    {
                        item.Tags = tag;
                    }
                }

                return View(discussions);
            }
            else
            {
                var discussions = await _disc.GetSearchResult(searchString, tagName);
                foreach (var item in discussions)
                {
                    var tag = await _disc.ListTag(item.Id);
                    if (tag != null)
                    {
                        item.Tags = tag;
                    }
                }

                return View(discussions);
            }
        }

        [HttpPost]
        public ActionResult UploadImage(List<IFormFile> files)
        {
            var filepath = "";
            foreach (IFormFile photo in Request.Form.Files)
            {
                string serverMapPath = Path.Combine(_env.WebRootPath, "Image", photo.FileName);
                using (var stream = new FileStream(serverMapPath, FileMode.Create))
                {
                    photo.CopyTo(stream);
                }
                filepath = "https://localhost:44353/" + "Image/" + photo.FileName;
            }
            return Json(new { url = filepath });
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Bookmark(long id)
        {
            var userID = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = new Guid(userID);
            await _disc.Bookmark(id, user);
            return RedirectToAction("Discussion", "Discussion", new { @id = id });
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> UnBookmark(long id)
        {
            await _disc.Unbookmark(id);
            return RedirectToAction("Discussion", "Discussion", new { @id = id });
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Follow(Guid authorID, long discussionID)
        {
            var userID = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = new Guid(userID);
            await _disc.Follow(user, authorID);
            return RedirectToAction("Discussion", "Discussion", new { @id = discussionID });
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> UnFollow(Guid authorID, long discussionID)
        {
            var userID = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = new Guid(userID);
            await _disc.UnFollow(user, authorID);
            return RedirectToAction("Discussion", "Discussion", new { @id = discussionID });
        }
    }
}
