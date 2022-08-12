using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using App.Admin.Roles;
using efcore.models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace App.Admin.Role
{
    [Authorize(Roles = "Admin")]
    public class DeleteModel : RolePageModel
    {
        public DeleteModel(RoleManager<IdentityRole> roleManager, AppDbContext myBlogContext) : base(roleManager, myBlogContext)
        {
        }

        public IdentityRole role { get; set; }

        public async Task<IActionResult> OnGet(string roleid)
        {
            if (roleid == null) return NotFound("Can not find a role");
            role = await _roleManager.FindByIdAsync(roleid);
            if (role == null)
            {
                return NotFound("Can not find a role");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string roleid)
        {
            if (roleid == null) return NotFound("Can not find a role");
            role = await _roleManager.FindByIdAsync(roleid);
            if (role == null) return NotFound("Can not find a role");
            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                StatusMessage = $"You just deleted a role {role.Name}";
                return RedirectToPage("./Index");
            }
            else
            {
                result.Errors.ToList().ForEach(e =>
                {
                    ModelState.AddModelError(string.Empty, e.Description);
                });
            }
            return Page();
        }
    }
}
