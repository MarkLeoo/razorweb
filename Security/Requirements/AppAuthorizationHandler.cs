using System.Security.Claims;
using efcore.models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace App.Security.Requirements
{
    public class AppAuthorizationHandler : IAuthorizationHandler
    {
        private readonly ILogger<AppAuthorizationHandler> _logger;
        private readonly UserManager<AppUser> _userManager;
        public AppAuthorizationHandler(ILogger<AppAuthorizationHandler> logger, UserManager<AppUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            var requirements = context.PendingRequirements.ToList();
            foreach (var requirement in requirements)
            {
                if (requirement is GenZRequirement)
                {
                    if (IsGenZ(context.User, (GenZRequirement)requirement))
                    {
                        context.Succeed(requirement);
                    }
                }
                if (requirement is ArticleUpdateRequirement)
                {
                    if (CanUpdateArticle(context.User, context.Resource, (ArticleUpdateRequirement)requirement))
                    {
                        context.Succeed(requirement);
                    }
                }
            }
            return Task.CompletedTask;
        }

        private bool CanUpdateArticle(ClaimsPrincipal user, object? resource, ArticleUpdateRequirement requirement)
        {
            if (user.IsInRole("Admin"))
            {
                _logger.LogInformation("Admin cập nhật....");
                return true;
            }
            var article = resource as Article;
            var dateCreated = article.Created;
            var dateCanUpdate = new DateTime(requirement.Year, requirement.Month, requirement.Date);
            if (dateCreated > dateCanUpdate)
            {
                _logger.LogInformation("Quá ngày cập nhật");
                return false;
            }
            return true;
        }

        private bool IsGenZ(ClaimsPrincipal user, GenZRequirement requirement)
        {
            var appUserTask = _userManager.GetUserAsync(user);
            Task.WaitAll(appUserTask);
            var appUser = appUserTask.Result;

            if (appUser.BirthDate == null)
            {
                _logger.LogInformation($"{appUser.UserName} không có ngày sinh, không thoả mãn GenZRequirement");
                return false;
            }
            int year = appUser.BirthDate.Value.Year;

            var isSuccess = (year >= requirement.FromYear && year <= requirement.ToYear);
            if (isSuccess)
            {
                _logger.LogInformation($"{appUser.UserName} thoả mãn GenZRequirement");
            }
            else
            {
                _logger.LogInformation($"{appUser.UserName} không thoả mãn GenZRequirement");
            }
            return isSuccess;
        }
    }
}