using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using efcore.models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace App.Admin.User
{
    public class EditUserRoleClaimModel : PageModel
    {
        private readonly MyBlogContext _context;
        private readonly UserManager<AppUser> _userManager;
        public EditUserRoleClaimModel(MyBlogContext myBlogContext, UserManager<AppUser> userManager)
        {
            _context = myBlogContext;
            _userManager = userManager;
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

        [TempData]
        public string StatusMessage { get; set; }
        public NotFoundObjectResult OnGet()
        {
            return NotFound("Access Denied");
        }

        public AppUser user { get; set; }

        public async Task<IActionResult> OnGetAddClaimAsync(string userid)
        {
            user = await _userManager.FindByIdAsync(userid);
            if (user == null) return NotFound("Không tìm thấy user");
            return Page();
        }

        public IdentityUserClaim<string> userClaim { get; set; }

        public async Task<IActionResult> OnGetEditClaimAsync(int? claimid)
        {
            if (claimid == null) return NotFound("Không tìm thấy user");
            userClaim = _context.UserClaims.Where(c => c.Id == claimid).FirstOrDefault();
            user = await _userManager.FindByIdAsync(userClaim.UserId);
            if (user == null) return NotFound("Không tìm thấy user");
            Input = new InputModel()
            {
                ClaimType = userClaim.ClaimType,
                ClaimValue = userClaim.ClaimValue
            };
            return Page();
        }


        public async Task<IActionResult> OnPostAddClaimAsync(string userid)
        {
            user = await _userManager.FindByIdAsync(userid);
            if (user == null) return NotFound("Không tìm thấy user");
            if (!ModelState.IsValid)
            {
                return Page();
            }
            var claims = _context.UserClaims.Where(c => c.UserId == user.Id);
            if (claims.Any(c => c.ClaimType == Input.ClaimType && c.ClaimValue == Input.ClaimValue))
            {
                ModelState.AddModelError(string.Empty, "Đặc tính này đã có");
                return Page();
            }
            await _userManager.AddClaimAsync(user, new Claim(Input.ClaimType, Input.ClaimValue));
            StatusMessage = "Đã thêm đặc tính cho user";
            return RedirectToPage("./AddRole", new { id = user.Id });
        }


        public async Task<IActionResult> OnPostEditClaimAsync(int? claimid)
        {
            if (claimid == null) return NotFound("Không tìm thấy user");
            userClaim = _context.UserClaims.Where(c => c.Id == claimid).FirstOrDefault();
            user = await _userManager.FindByIdAsync(userClaim.UserId);
            if (user == null) return NotFound("Không tìm thấy user");
            if (!ModelState.IsValid)
            {
                return Page();
            }
            if (_context.UserClaims.Any(c => c.UserId == user.Id && c.ClaimType == Input.ClaimType && c.ClaimValue == Input.ClaimValue && c.Id != userClaim.Id))
            {
                ModelState.AddModelError(string.Empty, "Claim này đã có");
                return Page();
            }
            userClaim.ClaimType = Input.ClaimType;
            userClaim.ClaimValue = Input.ClaimValue;
            await _context.SaveChangesAsync();
            StatusMessage = "Bạn vừa cập nhật";
            return RedirectToPage("./AddRole", new { id = user.Id });
        }


        public async Task<IActionResult> OnPostDeleteAsync(int? claimid)
        {
            if (claimid == null) return NotFound("Không tìm thấy user");
            userClaim = _context.UserClaims.Where(c => c.Id == claimid).FirstOrDefault();
            user = await _userManager.FindByIdAsync(userClaim.UserId);
            if (user == null) return NotFound("Không tìm thấy user");
            await _userManager.RemoveClaimAsync(user, new Claim(userClaim.ClaimType, userClaim.ClaimValue));
            StatusMessage = "Bạn vừa xoá";
            return RedirectToPage("./AddRole", new { id = user.Id });
        }

    }
}
