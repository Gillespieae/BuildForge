using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BuildForgeApp.Data;
using BuildForgeApp.Models;

namespace BuildForgeApp.Controllers
{
    [Authorize] // only logged-in users can access builds
    public class BuildsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        // inject database + identity to link builds to users
        public BuildsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            // only show builds for the current user
            var builds = await _context.Builds
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedDate)
                .ToListAsync();

            return View(builds);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);

            // load build + its components
            var build = await _context.Builds
                .Include(b => b.BuildComponents)
                .ThenInclude(bc => bc.PcComponent)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (build == null)
                return NotFound();

            // recalculate total price to keep it accurate
            build.TotalPrice = build.BuildComponents
                .Where(bc => bc.PcComponent != null)
                .Sum(bc => bc.PcComponent!.Price);

            // generate compatibility warnings for the view
            var warnings = GetCompatibilityWarnings(build);
            ViewBag.Warnings = warnings;

            return View(build);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Build build)
        {
            var userId = _userManager.GetUserId(User);

            // ensure user is logged in
            if (userId == null)
            {
                ModelState.AddModelError("", "You must be logged in to create a build.");
                return View(build);
            }

            // simple validation for build name
            if (string.IsNullOrWhiteSpace(build.BuildName))
            {
                ModelState.AddModelError("BuildName", "Build name is required.");
                return View(build);
            }

            try
            {
                // initialize build data
                build.UserId = userId;
                build.CreatedDate = DateTime.Now;
                build.TotalPrice = 0;

                _context.Builds.Add(build);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError("", "Something went wrong while creating the build. Please try again.");
                return View(build);
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);

            // only allow editing user's own builds
            var build = await _context.Builds
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (build == null)
                return NotFound();

            return View(build);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string buildName)
        {
            var userId = _userManager.GetUserId(User);

            var build = await _context.Builds
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (build == null)
                return NotFound();

            // update only the name (avoids overposting)
            build.BuildName = buildName;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);

            var build = await _context.Builds
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (build == null)
                return NotFound();

            return View(build);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveComponent(int buildId, int componentId)
        {
            // find the relationship between build and component
            var buildComponent = await _context.BuildComponents
                .FirstOrDefaultAsync(bc => bc.BuildId == buildId && bc.PcComponentId == componentId);

            if (buildComponent == null)
                return NotFound();

            // get component to adjust price
            var component = await _context.PcComponents
                .FirstOrDefaultAsync(c => c.Id == componentId);

            if (component != null)
            {
                var build = await _context.Builds
                    .FirstOrDefaultAsync(b => b.Id == buildId);

                if (build != null)
                {
                    // decrease total price when removing component
                    build.TotalPrice -= component.Price;
                }
            }

            _context.BuildComponents.Remove(buildComponent);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Builds", new { id = buildId });
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);

            var build = await _context.Builds
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (build == null)
                return NotFound();

            try
            {
                // permanently delete build
                _context.Builds.Remove(build);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError("", "This build could not be deleted. Please try again.");
                return View("Delete", build);
            }
        }

        // generates compatibility warnings (CPU/socket + PSU power)
        private List<string> GetCompatibilityWarnings(Build build)
        {
            var warnings = new List<string>();

            var components = build.BuildComponents
                .Where(bc => bc.PcComponent != null)
                .Select(bc => bc.PcComponent!)
                .ToList();

            var cpu = components.FirstOrDefault(c => c.ComponentType == "CPU");
            var motherboard = components.FirstOrDefault(c => c.ComponentType == "Motherboard");
            var psu = components.FirstOrDefault(c => c.ComponentType == "PSU");

            // CPU vs motherboard socket compatibility
            if (cpu != null && motherboard != null &&
                !string.IsNullOrEmpty(cpu.SocketType) &&
                !string.IsNullOrEmpty(motherboard.SocketType) &&
                cpu.SocketType != motherboard.SocketType)
            {
                warnings.Add($"CPU socket mismatch: {cpu.SocketType} vs {motherboard.SocketType}");
            }

            // PSU wattage validation
            if (psu != null && psu.Wattage.HasValue)
            {
                int totalWattage = components
                    .Where(c => c.ComponentType != "PSU")
                    .Sum(c => c.Wattage ?? 0);

                if (totalWattage > psu.Wattage.Value)
                {
                    warnings.Add($"Power supply too weak. Required: {totalWattage}W, PSU: {psu.Wattage}W");
                }
            }

            return warnings;
        }
    }
}
