using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using App.Admin.Roles;
using efcore.models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace App.Admin.Role
{
    public class AddRoleClaimModel : RolePageModel
    {
        public AddRoleClaimModel(RoleManager<IdentityRole> roleManager, AppDbContext myBlogContext) : base(roleManager, myBlogContext)
        {
        }

        public class InputModel
        {
            [DisplayName("Claim type")]
            [Required]
            [StringLength(256, MinimumLength = 3)]
            public string ClaimType { get; set; }

            [DisplayName("Claim value")]
            [Required]
            [StringLength(256, MinimumLength = 3)]
            public string ClaimValue { get; set; }
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IdentityRole role { get; set; }

        public async Task<IActionResult> OnGet(string roleid)
        {
            role = await _roleManager.FindByIdAsync(roleid);
            if (role == null) return NotFound($"Unable to find a role with id = {roleid}");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string roleid)
        {
            role = await _roleManager.FindByIdAsync(roleid);
            if (role == null) return NotFound($"Unable to find a role with id = {roleid}");
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if ((await _roleManager.GetClaimsAsync(role)).Any(c => c.Type == Input.ClaimType && c.Value == Input.ClaimValue))
            {
                ModelState.AddModelError(string.Empty, "This Claim has already exist in role");
                return Page();
            }

            var newClaim = new Claim(Input.ClaimType, Input.ClaimValue);

            var result = await _roleManager.AddClaimAsync(role, newClaim);

            if (!result.Succeeded)
            {
                result.Errors.ToList().ForEach(e =>
                {
                    ModelState.AddModelError(string.Empty, e.Description);
                });
            }

            StatusMessage = "You just added a new claim";

            return RedirectToPage("./Edit", new { roleid = role.Id });
        }
    }
}
