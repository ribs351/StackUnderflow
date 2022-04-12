using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StackUnderflow.Infrastructure.Repository;
using StackUnderflow.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Model.EF;
using Model.Entities;
using StackUnderflow.Infrastructure.DiscussionService;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace StackUnderflow.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private IRepositoryService _repo;
        private IDiscussionService _disc;
        private AppDbContext _ctx;

        public HomeController(AppDbContext ctx, ILogger<HomeController> logger, IRepositoryService repo, IDiscussionService disc)
        {
            _logger = logger;
            _repo = repo;
            _disc = disc;
            _ctx = ctx;
        }

        public async Task<IActionResult> Index(int pageNumber)
        {
            if (pageNumber < 1)
                return RedirectToAction("Index", new { pageNumber = 1 });

            int pageSize = 5;
            int skipAmount = pageSize * (pageNumber - 1);
            int articleCount = _ctx.Articles.Count();
            int capacity = skipAmount + pageSize;
            //normal calculations will round this number down, ie 11/5 = 2, meaning you missed a page
            //ceiling rounds this up a number, but you need a double for the calculation, so you convert one of the numbers into a double
            //and then cast the entire calculation to an int and bam
            int pageCount = (int)Math.Ceiling((double)articleCount / pageSize) ;  

            bool nextPage = articleCount > capacity;

            var articles = await _repo.GetAllArticles(pageNumber);

            foreach (var item in articles)
            {
                var tag = await _repo.ListTag(item.Id);
                if (tag != null)
                {
                    item.Tags = tag;
                }
            }
            var discussions = await _disc.GetAllDiscussionsByLatest(3);
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
            ViewBag.LastestDiscussions = discussions;

            return View(articles);
        }

        //public async Task<IActionResult> Trending()
        //{
        //    var articles = await _repo.GetTrendingArticles();
            
        //    foreach (var item in articles)
        //    {
        //        var tag = await _repo.ListTag(item.Id);
        //        if (tag != null)
        //        {
        //            item.Tags = tag;
        //        }
        //    }

        //    return View(articles);
        //}
        public async Task<IActionResult> SimpleSearch(string searchString)
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
            TempData["keyword"] = searchString;
            ViewBag.SearchedDiscussions = await _disc.GetDiscussionsByName(searchString);

            return View(articles);
        }
        public IActionResult AdvancedSearch()
        {
            return View();
        }
        [Authorize]
        public async Task<IActionResult> AllBookmarks()
        {
            var userID = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = new Guid(userID);
            var articles = await _repo.GetAllBookmarks(user);
            var discussions = await _disc.GetAllBookmarks(user);
            //assign tags to articles
            foreach (var article in articles)
            {
                var tag = await _repo.ListTag(article.Id);
                if (tag != null)
                {
                    article.Tags = tag;
                }
            }
            //assign tags to discussions
            foreach (var discussion in discussions)
            {
                var tag = await _disc.ListTag(discussion.Id);
                if (tag != null)
                {
                    discussion.Tags = tag;
                }
            }
            ViewBag.BookmarkedDiscussions = discussions;
            return View(articles);
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Following()
        {
            var userID = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = new Guid(userID);

            var articles = await _repo.GetAllArticlesFromFollows(user);
            var discussions = await _disc.GetAllDiscussionsFromFollows(user);

           

            foreach (var item in articles)
            {
                var tag = await _repo.ListTag(item.Id);
                if (tag != null)
                {
                    item.Tags = tag;
                }
            }

            foreach (var item in discussions)
            {
                var tag = await _disc.ListTag(item.Id);
                if (tag != null)
                {
                    item.Tags = tag;
                }
            }

            

            var followedUsers = await _repo.GetAllFollowedUsers(user);

            ViewBag.FollowedUsers = followedUsers;
            ViewBag.FollowedDiscussions = discussions;

            return View(articles);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
