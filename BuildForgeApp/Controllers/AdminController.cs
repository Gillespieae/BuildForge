using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BuildForgeApp.Data;
using BuildForgeApp.Models;

namespace BuildForgeApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var components = await _context.PcComponents.ToListAsync();
            return View(components);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PcComponent component)
        {
            if (ModelState.IsValid)
            {
                _context.Add(component);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(component);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var component = await _context.PcComponents.FindAsync(id);
            if (component == null) return NotFound();

            return View(component);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PcComponent component)
        {
            if (id != component.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(component);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(component);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var component = await _context.PcComponents.FindAsync(id);
            if (component == null) return NotFound();

            return View(component);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var component = await _context.PcComponents.FindAsync(id);
            if (component == null) return NotFound();

            component.IsActive = false;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}