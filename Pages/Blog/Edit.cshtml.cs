using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using efcore.models;
using Microsoft.AspNetCore.Authorization;

namespace efcore.Pages_Blog
{
    public class EditModel : PageModel
    {
        private readonly efcore.models.AppDbContext _context;
        private readonly IAuthorizationService _authorizationService;

        public EditModel(efcore.models.AppDbContext context, IAuthorizationService authorizationService)
        {
            _context = context;
            _authorizationService = authorizationService;
        }

        [BindProperty]
        public Article Article { get; set; } = default!;

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.Articles == null)
            {
                return NotFound();
            }

            var article = await _context.Articles.FirstOrDefaultAsync(m => m.Id == id);
            if (article == null)
            {
                return NotFound();
            }
            Article = article;
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Article).State = EntityState.Modified;

            try
            {
                var canUpdate = await _authorizationService.AuthorizeAsync(this.User, Article, "CanUpdateArticle");
                if (canUpdate.Succeeded)
                {

                    await _context.SaveChangesAsync();
                }
                else
                {
                    return Content("Không được quyền cập nhật");
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ArticleExists(Article.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool ArticleExists(int id)
        {
            return (_context.Articles?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
