using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Model.Entities;

namespace Model.EF
{
    public class AppDbContext : IdentityDbContext<User, AppRole, Guid>
    {
        public AppDbContext(DbContextOptions options)
          : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            //base.OnModelCreating(builder);
            builder.Entity<IdentityUser<Guid>>().ToTable("AppUsers");
            builder.Entity<IdentityRole<Guid>>().ToTable("AppRoles");
            builder.Entity<IdentityUserClaim<Guid>>().ToTable("AppUserClaims");
            builder.Entity<IdentityUserRole<Guid>>().ToTable("AppUserRoles").HasKey(x => new { x.UserId, x.RoleId });
            builder.Entity<IdentityUserLogin<Guid>>().ToTable("AppUserLogins").HasKey(x => x.UserId);

            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("AppRoleClaims");
            builder.Entity<IdentityUserToken<Guid>>().ToTable("AppUserTokens").HasKey(x => x.UserId);
            
            builder.Entity<ArticleTag>().HasKey(x => new { x.ArticleID, x.TagID });
            builder.Entity<DiscussionTag>().HasKey(x => new { x.DiscussionID, x.TagID });
            builder.Entity<ArticleBookmark>().HasKey(x => new { x.UserID, x.ArticleID });
            builder.Entity<DiscussionBookmark>().HasKey(x => new { x.UserID, x.DiscussionID });
            builder.Entity<SeriesBookmark>().HasKey(x => new { x.UserID, x.SeriesID });
            builder.Entity<SeriesArticle>().HasKey(x => new{x.SeriesID, x.ArticleID});
            builder.Entity<UserFollow>().HasKey(x => new { x.UserID, x.FollowedUserID });
        }
        
        public DbSet<Article> Articles { get; set; }
        public DbSet<ArticleComment> ArticleComments { get; set; }
        public DbSet<Discussion> Discussions { get; set; }
        public DbSet<DiscussionReply> DiscussionReplies { get; set; }
        public DbSet<DiscussionComment> DiscussionComments { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<ArticleTag> ArticleTags { get; set; }
        public DbSet<DiscussionTag> DiscussionTags { get; set; }
        public DbSet<Series> SeriesTable { get; set; }
        public DbSet<SeriesBookmark> SeriesBookmarks { get; set; }
        public DbSet<ArticleBookmark> ArticleBookmarks { get; set; }
        public DbSet<DiscussionBookmark> DiscussionBookmarks { get; set; }
        public DbSet<UserFollow> UserFollows { get; set; }
    }
}
