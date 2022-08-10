using efcore.models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace App.Admin.User
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly UserManager<AppUser> _userManager;
        public IndexModel(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public class UserRole : AppUser
        {
            public string RoleNames { get; set; }
        }

        [TempData]
        public string StatusMessage { get; set; }

        public const int ITEMS_PER_PAGE = 10;

        [BindProperty(SupportsGet = true, Name = "pages")]
        public int currentPage { get; set; }
        public int countPages { get; set; }

        public List<UserRole> users { get; set; }

        public int totalUsers { get; set; }

        public async Task OnGet()
        {
            var query = _userManager.Users.OrderBy(u => u.UserName);
            totalUsers = query.Count();
            countPages = (int)Math.Ceiling((double)totalUsers / ITEMS_PER_PAGE);
            if (currentPage < 1)
            {
                currentPage = 1;
            }
            if (currentPage > countPages)
            {
                currentPage = countPages;
            }
            var query1 = query.Skip((currentPage - 1) * ITEMS_PER_PAGE).Take(ITEMS_PER_PAGE).Select(u => new UserRole()
            {
                Id = u.Id,
                UserName = u.UserName
            });
            users = query1.ToList();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                user.RoleNames = string.Join(", ", roles);
            }
        }

        public void OnPost() => RedirectToPage();
    }
}
