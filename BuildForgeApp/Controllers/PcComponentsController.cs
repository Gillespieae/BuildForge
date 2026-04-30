using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BuildForgeApp.Data;
using BuildForgeApp.Models;

namespace BuildForgeApp.Controllers
{
    public class PcComponentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Constructor: receives the database context so this controller can query data
        public PcComponentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // buildId: used when adding a component to a specific build
        // category: filters components by type (CPU, GPU, etc.)
        // searchString: keyword search across name, brand, or type
        // brand: filters components by brand name
        public async Task<IActionResult> Index(int? buildId, string? category, string? searchString, string? brand)
        {
            var query = _context.PcComponents
                .Where(pc => pc.IsActive) // only show active components
                .AsQueryable();

            // filter by component category (e.g., CPU, GPU)
            if (!string.IsNullOrEmpty(category))
                query = query.Where(pc => pc.ComponentType == category);

            // search by name, brand, or type
            if (!string.IsNullOrEmpty(searchString))
                query = query.Where(pc =>
                    pc.Name.Contains(searchString) ||
                    pc.Brand.Contains(searchString) ||
                    pc.ComponentType.Contains(searchString));

            // filter by brand (e.g., Intel, AMD)
            if (!string.IsNullOrEmpty(brand))
                query = query.Where(pc => pc.Brand == brand);

            // execute query and sort results
            var components = await query
                .OrderBy(pc => pc.ComponentType)
                .ThenBy(pc => pc.Brand)
                .ThenBy(pc => pc.Name)
                .ToListAsync();

            // store the current build ID so the view can use it when adding components
            ViewBag.BuildId = buildId;

            // list of categories for dropdown/filter UI
            ViewBag.Categories = await _context.PcComponents
                .Where(pc => pc.IsActive)
                .Select(pc => pc.ComponentType)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            // list of brands for dropdown/filter UI
            ViewBag.Brands = await _context.PcComponents
                .Where(pc => pc.IsActive)
                .Select(pc => pc.Brand)
                .Distinct()
                .OrderBy(b => b)
                .ToListAsync();

            return View(components);
        }

        // id: the specific component ID to view details for
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var component = await _context.PcComponents
                .FirstOrDefaultAsync(pc => pc.Id == id && pc.IsActive);

            if (component == null) return NotFound();

            return View(component);
        }

        // buildId: the build the component is being added to
        // componentId: the selected component to add
        [HttpPost]
        public async Task<IActionResult> AddToBuild(int buildId, int componentId)
        {
            // load the build including its current components
            var build = await _context.Builds
                .Include(b => b.BuildComponents)
                .FirstOrDefaultAsync(b => b.Id == buildId);

            // load the selected component
            var component = await _context.PcComponents
                .FirstOrDefaultAsync(c => c.Id == componentId);

            if (build == null || component == null)
                return NotFound();

            // check if this component is already in the build
            var existing = await _context.BuildComponents
                .FirstOrDefaultAsync(bc => bc.BuildId == buildId && bc.PcComponentId == componentId);

            // only add if it doesn't already exist
            if (existing == null)
            {
                _context.BuildComponents.Add(new BuildComponent
                {
                    BuildId = buildId,
                    PcComponentId = componentId
                });

                // update total build price when adding component
                build.TotalPrice += component.Price;
            }

            await _context.SaveChangesAsync();

            // redirect back to the build details page after adding
            return RedirectToAction("Details", "Builds", new { id = buildId });
        }
    }
}
