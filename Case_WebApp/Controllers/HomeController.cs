using Case_Service.Context;
using Case_Service.Models;
using Case_WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Case_WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ServiceDbContext _context;

        public HomeController(ServiceDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }
        public IActionResult GetData()
        {
            List<Word> data = _context.Words.ToList();

            return PartialView("_DataPartialView", data);
        }

        public async Task<IActionResult> Index(int? page)
        {
            int pageSize = 10;

            var words = await _context.Words.AsNoTracking().ToListAsync();

            int pageNumber = page ?? 1;
            var paginatedWords = words.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling(words.Count / (double)pageSize);

            return View(paginatedWords.ToList());
        }

        public IActionResult Delete(int id)
        {
            var word = _context.Words.Find(id);
            if (word == null)
            {
                return NotFound();
            }

            return View(word);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var word = _context.Words.Find(id);
            if (word != null)
            {
                _context.Words.Remove(word);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();

        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}