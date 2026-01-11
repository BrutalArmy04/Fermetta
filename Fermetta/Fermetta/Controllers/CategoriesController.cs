using Fermetta.Data;
using Fermetta.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Fermetta.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CategoriesController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .Include(c => c.Products)
                .ToListAsync();
            return View(categories);
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(m => m.Category_Id == id);
            if (category == null) return NotFound();

            return View(category);
        }

        // GET: Categories/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Category_Id,Name,Description,Disponibility,ImageFile,ImagePath")] Category category)
        {
            if (ModelState.IsValid)
            {
                if (category.ImageFile != null)
                {
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + category.ImageFile.FileName;
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "categories");

                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await category.ImageFile.CopyToAsync(fileStream);
                    }
                    category.ImagePath = "/images/categories/" + uniqueFileName;
                }

                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categories/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            return View(category);
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Category_Id,Name,Description,Disponibility,ImageFile,ImagePath")] Category category)
        {
            if (id != category.Category_Id) return NotFound();

            var existingCategory = await _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Category_Id == id);

            if (ModelState.IsValid)
            {
                try
                {
                    if (category.ImageFile != null)
                    {
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + category.ImageFile.FileName;
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "categories");

                        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await category.ImageFile.CopyToAsync(fileStream);
                        }
                        category.ImagePath = "/images/categories/" + uniqueFileName;
                    }
                    else
                    {
                        if (existingCategory != null) category.ImagePath = existingCategory.ImagePath;
                    }

                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Category_Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            if (existingCategory != null && string.IsNullOrEmpty(category.ImagePath))
            {
                category.ImagePath = existingCategory.ImagePath;
                ModelState.Remove("ImagePath");
            }

            return View(category);
        }

        // GET: Categories/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Category_Id == id);
            if (category == null) return NotFound();

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Category_Id == id);
        }
    }
}