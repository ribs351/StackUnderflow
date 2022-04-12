using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Model.EF;
using Model.Entities;
using Model.ViewModel.Repository;
using StackUnderflow.Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Model.ViewModel.Comment;
using StackUnderflow.Infrastructure.ArticleComments;
using Microsoft.EntityFrameworkCore;

namespace StackUnderflow.Controllers
{
    
    public class ArticleController : Controller
    {
        private AppDbContext _ctx;
        private IRepositoryService _repo;
        private IArticleCommentService _artcomm;
        private readonly IWebHostEnvironment _env;
        public ArticleController(AppDbContext ctx, IRepositoryService repo, IWebHostEnvironment env, IArticleCommentService artcomm)
        {
            _ctx = ctx;
            _repo = repo;
            _env = env;
            _artcomm = artcomm;
        }

        [Authorize]
        public IActionResult RemoveTag(string tagID, long articleID)
        {
            _repo.RemoveArticleTag(tagID, articleID);
            return RedirectToAction("Edit", "Article", new { @id = articleID });
        }
        [Authorize]
        [HttpGet]
        public async Task <IActionResult> Edit(long id)
        {
            var article = _repo.GetArticle(id);
            var tag = await _repo.ListTag(article.Id);
            if (tag != null)
            {
                article.Tags = tag;
            }
            return View(article);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(long Id, RepositoryViewModel request)
        {
            var result = await _repo.UpdateArticle(Id, request);
            if (result == true)
            {
                return RedirectToAction("Article", "Article", new {@id = Id});
            }
            else
                return RedirectToAction("Error");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Comment(ArticleCommentViewModel request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            request.UserId = new Guid(userId);
            var result = await _artcomm.Create(request);
            if (result == true)
            {
                // refresh the page lol
                return RedirectToAction("Article", "Article", new {@id = request.ArticleID });
            }
            else
                return RedirectToAction("Error");
        }

        [Authorize]
        [HttpGet]
        public IActionResult Publish()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> Publish(RepositoryViewModel request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            request.UserId = new Guid(userId);

            var result = await _repo.Create(request);
            if (result == true)
            {
                return RedirectToAction("Index", "Home");
            }
            else
                return RedirectToAction("Error");
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Remove(long id)
        {
            _repo.RemoveArticle(id);
            await _repo.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }
        public async Task<IActionResult> Article(long id)
        {
            var article = _repo.GetArticle(id);
            var tag = await _repo.ListTag(article.Id);
            var isAuthenticated = User.Identity.IsAuthenticated;
            if (isAuthenticated == true)
            {
                var userID = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = new Guid(userID);
                var isFollowed = _repo.CheckFollows(user, article.UserId);
                var isBookmarked = _repo.CheckBookmarks(user, id);
                var isOwner = user == article.UserId;
                if (isOwner == true)
                {
                    article.Owner = true;
                }
                if (isFollowed == true)
                {
                    article.Followed = true;
                }
                if (isBookmarked == true)
                {
                    article.Bookmarked = true;
                }
            }
            if (tag != null)
            {
                article.Tags = tag;
            }
            
            ViewBag.ArticleComments = await _artcomm.GetAllCommentsByArticle(id);
            await _repo.IncreaseViewCount(id);
            return View(article);
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
        public async Task<IActionResult> ArticleByTag(string id)
        {
            var articles = await _repo.GetAllArticlesByTag(id);
            var tempTag = _ctx.Tags.FirstOrDefault(x => x.Id == id);
            TempData["TagName"] = tempTag.Name;
            foreach (var item in articles)
            {
                var tag = await _repo.ListTag(item.Id);
                if (tag != null)
                {
                    item.Tags = tag;
                }
            }

            return View(articles);
        }

        public async Task<IActionResult> Trending(int pageNumber)
        {
            if (pageNumber < 1)
                return RedirectToAction("Trending", new { pageNumber = 1 });

            int pageSize = 5;
            int skipAmount = pageSize * (pageNumber - 1);
            int articleCount = _ctx.Articles.Count();
            int capacity = skipAmount + pageSize;
            //normal calculations will round this number down, ie 11/5 = 2, meaning you missed a page
            //ceiling rounds this up a number, but you need a double for the calculation, so you convert one of the numbers into a double
            //and then cast the entire calculation to an int and bam
            int pageCount = (int)Math.Ceiling((double)articleCount / pageSize);

            bool nextPage = articleCount > capacity;

            var articles = await _repo.GetTrendingArticles(pageNumber);
            foreach (var item in articles)
            {
                var tag = await _repo.ListTag(item.Id);
                if (tag != null)
                {
                    item.Tags = tag;
                }
            }
            ViewBag.PageCount = pageCount;
            ViewBag.NextPage = nextPage;
            ViewBag.PageNumber = pageNumber;
            return View(articles);
        }

        public async Task<IActionResult> SearchResult(string searchString, string tagName)
        {
            IQueryable<string> articleTagQuery = from at in _ctx.ArticleTags
                                                 join t in _ctx.Tags on at.TagID equals t.Id
                                                 orderby at.TagID
                                                    select t.Name;

            if (searchString == null)
            {
                var articles = await _repo.GetSearchResultByTag(tagName);
                foreach (var item in articles)
                {
                    var tag = await _repo.ListTag(item.Id);
                    if (tag != null)
                    {
                        item.Tags = tag;
                    }
                }

                return View(articles);
            }

            if (tagName == null)
            {
                var articles = await _repo.GetArticleByName(searchString);
                foreach (var item in articles)
                {
                    var tag = await _repo.ListTag(item.Id);
                    if (tag != null)
                    {
                        item.Tags = tag;
                    }
                }

                return View(articles);
            }
            if (!articleTagQuery.Contains(tagName))
            {
                var articles = await _repo.GetArticleByName(searchString);
                foreach (var item in articles)
                {
                    var tag = await _repo.ListTag(item.Id);
                    if (tag != null)
                    {
                        item.Tags = tag;
                    }
                }

                return View(articles);
            }
            else
            {
                var articles = await _repo.GetSearchResult(searchString, tagName);
                foreach (var item in articles)
                {
                    var tag = await _repo.ListTag(item.Id);
                    if (tag != null)
                    {
                        item.Tags = tag;
                    }
                }
                ViewBag.SearchString = searchString;
                ViewBag.TagName = tagName;
                return View(articles);
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Bookmark(long id)
        {
            var userID = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = new Guid(userID);
            await _repo.Bookmark(id, user);
            return RedirectToAction("Article", "Article", new { @id = id });
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> UnBookmark(long id)
        {
            var userID = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = new Guid(userID);
            await _repo.UnBookmark(id, user);
            return RedirectToAction("Article", "Article", new { @id = id });
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Follow(Guid authorID, long articleID)
        {
            var userID = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = new Guid(userID);
            await _repo.Follow(user, authorID);
            return RedirectToAction("Article", "Article", new { @id = articleID });
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> UnFollow(Guid authorID, long articleID)
        {
            var userID = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = new Guid(userID);
            await _repo.UnFollow(user, authorID);
            return RedirectToAction("Article", "Article", new { @id = articleID });
        }
    }
}
