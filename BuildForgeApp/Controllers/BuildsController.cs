using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BuildForgeApp.Data;
using BuildForgeApp.Models;

namespace BuildForgeApp.Controllers
{
    [Authorize]
    public class BuildsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public BuildsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var builds = await _context.Builds
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedDate)
                .ToListAsync();

            return View(builds);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            var build = await _context.Builds
                .Include(b => b.BuildComponents)
                .ThenInclude(bc => bc.PcComponent)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (build == null)
            {
                return NotFound();
            }

            build.TotalPrice = CalculateTotalPrice(build);

            return View(build);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BuildName")] Build build)
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return Challenge();
            }

            if (ModelState.IsValid)
            {
                build.UserId = userId;
                build.CreatedDate = DateTime.Now;
                build.TotalPrice = 0;

                _context.Add(build);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(build);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            var build = await _context.Builds
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (build == null)
            {
                return NotFound();
            }

            return View(build);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,BuildName")] Build build)
        {
            if (id != build.Id)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var existingBuild = await _context.Builds
                .Include(b => b.BuildComponents)
                .ThenInclude(bc => bc.PcComponent)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (existingBuild == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                existingBuild.BuildName = build.BuildName;
                existingBuild.TotalPrice = CalculateTotalPrice(existingBuild);

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(build);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            var build = await _context.Builds
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (build == null)
            {
                return NotFound();
            }

            return View(build);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);

            var build = await _context.Builds
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (build == null)
            {
                return NotFound();
            }

            _context.Builds.Remove(build);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private decimal CalculateTotalPrice(Build build)
        {
            return build.BuildComponents
                .Where(bc => bc.PcComponent != null)
                .Sum(bc => bc.PcComponent!.Price);
        }
    }
}
