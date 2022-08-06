using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using efcore.models;
using Microsoft.AspNetCore.Authorization;

namespace efcore.Pages_Blog
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly efcore.models.MyBlogContext _context;

        public IndexModel(efcore.models.MyBlogContext context)
        {
            _context = context;
        }

        public IList<Article> Article { get; set; } = default!;

        public const int ITEMS_PER_PAGE = 10;

        [BindProperty(SupportsGet = true, Name = "pages")]
        public int currentPage { get; set; }
        public int countPages { get; set; }

        public async Task OnGetAsync(string SearchString)
        {

            if (_context.Articles != null)
            {
                // Article = await _context.Articles.ToListAsync();
                int totalArticle = await _context.Articles.CountAsync();
                countPages = (int)Math.Ceiling((double)totalArticle / ITEMS_PER_PAGE);
                if (currentPage < 1)
                {
                    currentPage = 1;
                }
                if (currentPage > countPages)
                {
                    currentPage = countPages;
                }
                var query = (from a in _context.Articles
                             orderby a.Created descending
                             select a).Skip((currentPage - 1) * ITEMS_PER_PAGE).Take(ITEMS_PER_PAGE);
                if (!string.IsNullOrEmpty(SearchString))
                {
                    Article = query.Where(a => a.Title.Contains(SearchString)).ToList();
                }
                else
                {
                    Article = await query.ToListAsync();
                }

            }
        }
    }
}
