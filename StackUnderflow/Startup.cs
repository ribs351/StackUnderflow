using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Model.EF;
using Model.Entities;
using StackUnderflow.Infrastructure.Repository;
using StackUnderflow.Infrastructure.Users;
using Microsoft.AspNetCore.Authentication.Cookies;
using StackUnderflow.Infrastructure.DiscussionService;
using StackUnderflow.Infrastructure.ArticleComments;
using StackUnderflow.Infrastructure.DicussionComments;

namespace StackUnderflow
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddIdentity<User, AppRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
            })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();
            //services.AddIdentity<User, AppRole>()
            //  .AddEntityFrameworkStores<AppDbContext>()
            //  .AddDefaultTokenProviders();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IArticleCommentService, ArticleCommentService>();
            services.AddTransient<IDiscussionService, DiscussionService>();
            services.AddTransient<IRepositoryService, RepositoryService>();
            services.AddTransient<IDiscussionCommentService, DiscussionCommentService>();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Login/Index";
                    options.AccessDeniedPath = "/User/AccessDenied";
                });

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(4);
            });
        }
       

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseRouting();
            app.UseSession();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
