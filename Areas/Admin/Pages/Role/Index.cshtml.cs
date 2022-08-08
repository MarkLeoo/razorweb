using efcore.models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace App.Admin.Roles
{
    public class IndexModel : RolePageModel
    {
        public IndexModel(RoleManager<IdentityRole> roleManager, MyBlogContext myBlogContext) : base(roleManager, myBlogContext)
        {
        }

        public List<IdentityRole> roles { get; set; }

        public void OnGet()
        {
            roles = _roleManager.Roles.OrderBy(r => r.Name).ToList();
        }

        public void OnPost() => RedirectToPage();
    }
}
