// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using efcore.models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace App.Admin.User
{
    public class SetRoleModel : PageModel
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly MyBlogContext _context;

        public SetRoleModel(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            MyBlogContext myBlogContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = myBlogContext;
        }

        [TempData]
        public string StatusMessage { get; set; }

        public AppUser user { get; set; }

        [BindProperty]
        [Display(Name = "Roles are assign for user")]
        public string[] RoleNames { get; set; }

        public SelectList allRoles { get; set; }

        public List<IdentityRoleClaim<string>> claimsInRole { get; set; }
        public List<IdentityUserClaim<string>> claimsInUserClaim { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound($"Unable to load user with ID '{id}'");
            user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{id}'");
            }

            RoleNames = (await _userManager.GetRolesAsync(user)).ToArray<string>();

            List<string> roleName = _roleManager.Roles.Select(r => r.Name).ToList();

            allRoles = new SelectList(roleName);

            GetClaims(id);

            return Page();
        }

        void GetClaims(string id)
        {
            var listRoles = from r in _context.Roles
                            join ur in _context.UserRoles on r.Id equals ur.RoleId
                            where ur.UserId == id
                            select r;
            var _claimsInRole = from c in _context.RoleClaims
                                join r in listRoles on c.RoleId equals r.Id
                                select c;
            claimsInRole = _claimsInRole.ToList();

            claimsInUserClaim = (from c in _context.UserClaims where c.UserId == id select c).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound($"Unable to load user with ID '{id}'");
            user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{id}'");
            }

            GetClaims(id);

            var OldRoleNames = (await _userManager.GetRolesAsync(user)).ToArray();

            var deleteRoles = OldRoleNames.Where(r => !RoleNames.Contains(r));
            var addRoles = RoleNames.Where(r => !OldRoleNames.Contains(r));

            List<string> roleName = _roleManager.Roles.Select(r => r.Name).ToList();

            allRoles = new SelectList(roleName);

            var resultDelete = await _userManager.RemoveFromRolesAsync(user, deleteRoles);

            if (!resultDelete.Succeeded)
            {
                resultDelete.Errors.ToList().ForEach(e =>
                {
                    ModelState.AddModelError(string.Empty, e.Description);
                });
                return Page();
            }


            var resultAdd = await _userManager.AddToRolesAsync(user, addRoles);

            if (!resultAdd.Succeeded)
            {
                resultAdd.Errors.ToList().ForEach(e =>
                {
                    ModelState.AddModelError(string.Empty, e.Description);
                });
                return Page();
            }


            StatusMessage = $"{user.UserName} role has been set.";

            return RedirectToPage("./Index");
        }
    }
}
