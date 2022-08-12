using Album.Mail;
using efcore.models;
using App.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Security.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace efcore
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
            services.AddOptions();
            var mailSettings = Configuration.GetSection("MailSettings");
            services.Configure<MailSettings>(mailSettings);
            services.AddSingleton<IEmailSender, SendMailService>();


            services.AddRazorPages();
            services.AddDbContext<AppDbContext>(options =>
            {
                string connectionString = Configuration.GetConnectionString("MyBlogContext");
                options.UseSqlServer(connectionString);
            });

            // Đăng ký identity
            services.AddIdentity<AppUser, IdentityRole>()
                    .AddEntityFrameworkStores<AppDbContext>()
                    .AddDefaultTokenProviders();

            // services.AddDefaultIdentity<AppUser>()
            //         .AddEntityFrameworkStores<MyBlogContext>()
            //         .AddDefaultTokenProviders();


            services.Configure<IdentityOptions>(options =>
            {
                // Thiết lập password
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredUniqueChars = 1;
                options.Password.RequiredLength = 3;

                // Cấu hình lockout, khoá user
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 3;
                options.Lockout.AllowedForNewUsers = true;

                // Cấu hình về user
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;

                // Cấu hình đăng nhập
                options.SignIn.RequireConfirmedEmail = true;
                options.SignIn.RequireConfirmedPhoneNumber = false;
                options.SignIn.RequireConfirmedAccount = true;
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/login.html";
                options.LogoutPath = "/Logout.html";
                options.AccessDeniedPath = "/access-denied.html";
            });

            services.AddAuthentication()
                    .AddGoogle(googleOptions =>
                    {
                        var googleConfig = Configuration.GetSection("Authentication:Google");
                        googleOptions.ClientId = googleConfig["ClientId"];
                        googleOptions.ClientSecret = googleConfig["ClientSecret"];
                        googleOptions.CallbackPath = "/dang-nhap-tu-google";
                    })
                    .AddFacebook(facebookOptions =>
                    {
                        var facebookConfig = Configuration.GetSection("Authentication:Facebook");
                        facebookOptions.AppId = facebookConfig["AppId"];
                        facebookOptions.AppSecret = facebookConfig["AppSecret"];
                        facebookOptions.CallbackPath = "/dang-nhap-tu-facebook";
                    });

            services.AddSingleton<IdentityErrorDescriber, AddIdentityErrorDescriber>();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AllowedEditRole", policyBuilder =>
                {
                    policyBuilder.RequireAuthenticatedUser();
                    // policyBuilder.RequireRole("Admin");
                    // policyBuilder.RequireRole("Editor");
                    policyBuilder.RequireClaim("manage.role", "add", "update");
                });
                options.AddPolicy("InGenZ", policyBuilder =>
                {
                    policyBuilder.RequireAuthenticatedUser();
                    policyBuilder.Requirements.Add(new GenZRequirement());
                });

                options.AddPolicy("ShowAdminMenu", policyBuilder =>
                {
                    policyBuilder.RequireRole("Admin");
                });

                options.AddPolicy("CanUpdateArticle", policyBuilder =>
                {
                    policyBuilder.Requirements.Add(new ArticleUpdateRequirement());
                });
            });
            services.AddTransient<IAuthorizationHandler, AppAuthorizationHandler>();

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
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
