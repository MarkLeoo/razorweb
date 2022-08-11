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
    public class EditRoleClaimModel : RolePageModel
    {
        public EditRoleClaimModel(RoleManager<IdentityRole> roleManager, MyBlogContext myBlogContext) : base(roleManager, myBlogContext)
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

        public IdentityRoleClaim<string>? claim { get; set; }

        public async Task<IActionResult> OnGet(int? claimid)
        {
            if (claimid == null) return NotFound($"Unable to find a role with id = {claimid}");
            claim = _context.RoleClaims.Where(c => c.Id == claimid).FirstOrDefault();
            if (claim == null) return NotFound($"Unable to find a role with id = {claimid}");
            role = await _roleManager.FindByIdAsync(claim.RoleId);
            if (role == null) return NotFound($"Unable to find a role with id = {claimid}");

            Input = new InputModel()
            {
                ClaimType = claim.ClaimType,
                ClaimValue = claim.ClaimValue
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? claimid)
        {
            if (claimid == null) return NotFound($"Unable to find a role with id = {claimid}");
            claim = _context.RoleClaims.Where(c => c.Id == claimid).FirstOrDefault();
            if (claim == null) return NotFound($"Unable to find a role with id = {claimid}");
            role = await _roleManager.FindByIdAsync(claim.RoleId);
            if (role == null) return NotFound($"Unable to find a role with id = {claimid}");
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (_context.RoleClaims.Any(c => c.RoleId == role.Id && c.ClaimType == Input.ClaimType && c.ClaimValue == Input.ClaimValue && c.Id != claimid))
            {
                ModelState.AddModelError(string.Empty, "This Claim has already exist in role");
                return Page();
            }

            claim.ClaimType = Input.ClaimType;
            claim.ClaimValue = Input.ClaimValue;

            await _context.SaveChangesAsync();

            StatusMessage = "You just updated a claim";

            return RedirectToPage("./Edit", new { roleid = role.Id });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int? claimid)
        {
            if (claimid == null) return NotFound($"Unable to find a role with id = {claimid}");
            claim = _context.RoleClaims.Where(c => c.Id == claimid).FirstOrDefault();
            if (claim == null) return NotFound($"Unable to find a role with id = {claimid}");
            role = await _roleManager.FindByIdAsync(claim.RoleId);
            if (role == null) return NotFound($"Unable to find a role with id = {claimid}");

            await _roleManager.RemoveClaimAsync(role, new Claim(claim.ClaimType, claim.ClaimValue));

            StatusMessage = "You just deleted a claim";

            return RedirectToPage("./Edit", new { roleid = role.Id });
        }
    }
}
