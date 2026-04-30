using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BuildForgeApp.Data;
using BuildForgeApp.Models;

namespace BuildForgeApp.Controllers
{
    [Authorize(Roles = "Admin")] // restricts ALL actions in this controller to Admin users only
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        // inject database context for CRUD operations
        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // shows all components (active + inactive)
        public async Task<IActionResult> Index()
        {
            var components = await _context.PcComponents
                .OrderBy(c => c.ComponentType) // group by type first (CPU, GPU, etc.)
                .ThenBy(c => c.Brand)         // then brand (Intel, AMD)
                .ThenBy(c => c.Name)          // then specific model
                .ToListAsync();

            return View(components);
        }

        // returns empty form for creating a new component
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // prevents CSRF attacks from external sites
        public async Task<IActionResult> Create(PcComponent component)
        {
            // validates based on model annotations
            if (!ModelState.IsValid)
            {
                return View(component);
            }

            try
            {
                component.IsActive = true; // ensures new components are visible by default

                _context.PcComponents.Add(component);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                // user-friendly error instead of crashing
                ModelState.AddModelError("", "Unable to create component. Please try again.");
                return View(component);
            }
        }

        // loads existing component into edit form
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var component = await _context.PcComponents.FindAsync(id);

            if (component == null)
                return NotFound();

            return View(component);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PcComponent component)
        {
            // ensures URL id matches form id (prevents tampering)
            if (id != component.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                return View(component);
            }

            try
            {
                _context.Update(component); // updates entire entity in DB
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError("", "Unable to update component. Please try again.");
                return View(component);
            }
        }

        // confirmation page before deleting
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var component = await _context.PcComponents.FindAsync(id);

            if (component == null)
                return NotFound();

            return View(component);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var component = await _context.PcComponents.FindAsync(id);

            if (component == null)
                return NotFound();

            try
            {
                // soft delete: hides component instead of removing it
                component.IsActive = false;
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError("", "Unable to deactivate component. Please try again.");
                return View("Delete", component);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivate(int id)
        {
            var component = await _context.PcComponents.FindAsync(id);

            if (component == null)
                return NotFound();

            component.IsActive = true; // brings back previously "deleted" component
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int id)
        {
            var component = await _context.PcComponents.FindAsync(id);

            if (component == null)
                return NotFound();

            try
            {
                // hard delete: permanently removes from database
                _context.PcComponents.Remove(component);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError("", "Unable to delete component. Please try again.");
                return RedirectToAction(nameof(Index));
            }
        }
    }
}