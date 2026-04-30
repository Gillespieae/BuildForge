using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BuildForgeApp.Data;
using BuildForgeApp.Models;

namespace BuildForgeApp.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // only logged-in users can access this API
    public class BuildCompatibilityApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        // constructor injects database context + user manager for identity
        public BuildCompatibilityApiController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET api/BuildCompatibilityApi/{buildId}
        [HttpGet("{buildId}")]
        public async Task<IActionResult> CheckCompatibility(int buildId)
        {
            // get currently logged-in user's ID
            var userId = _userManager.GetUserId(User);

            // load the build with its components (and their actual PcComponent data)
            // also ensures the build belongs to the current user
            var build = await _context.Builds
                .Include(b => b.BuildComponents)
                .ThenInclude(bc => bc.PcComponent)
                .FirstOrDefaultAsync(b => b.Id == buildId && b.UserId == userId);

            if (build == null)
            {
                return NotFound();
            }

            // generate compatibility warnings based on components
            var warnings = GetCompatibilityWarnings(build);

            // return result as JSON
            return Ok(new
            {
                isCompatible = !warnings.Any(), // true if no warnings
                warnings = warnings
            });
        }

        private List<string> GetCompatibilityWarnings(Build build)
        {
            var warnings = new List<string>();

            // extract actual component objects from the build
            var components = build.BuildComponents
                .Where(bc => bc.PcComponent != null)
                .Select(bc => bc.PcComponent!)
                .ToList();

            // group components by type
            var cpus = components.Where(c => c.ComponentType == "CPU").ToList();
            var motherboards = components.Where(c => c.ComponentType == "Motherboard").ToList();
            var psus = components.Where(c => c.ComponentType == "PSU").ToList();

            // grab the first of each (used for compatibility checks)
            var cpu = cpus.FirstOrDefault();
            var motherboard = motherboards.FirstOrDefault();
            var psu = psus.FirstOrDefault();

            // ensure exactly one CPU
            if (cpus.Count == 0)
            {
                warnings.Add("No CPU selected.");
            }

            if (cpus.Count > 1)
            {
                warnings.Add("Multiple CPUs selected. Only one CPU is allowed.");
            }

            // ensure exactly one motherboard
            if (motherboards.Count == 0)
            {
                warnings.Add("No motherboard selected.");
            }

            if (motherboards.Count > 1)
            {
                warnings.Add("Multiple motherboards selected. Only one motherboard is allowed.");
            }

            // ensure exactly one PSU
            if (psus.Count == 0)
            {
                warnings.Add("No power supply selected.");
            }

            if (psus.Count > 1)
            {
                warnings.Add("Multiple power supplies selected. Only one PSU is allowed.");
            }

            // check CPU + motherboard socket compatibility
            if (cpu != null && motherboard != null &&
                !string.IsNullOrEmpty(cpu.SocketType) &&
                !string.IsNullOrEmpty(motherboard.SocketType) &&
                !cpu.SocketType.Equals(motherboard.SocketType, StringComparison.OrdinalIgnoreCase))
            {
                warnings.Add($"CPU socket mismatch: {cpu.SocketType} vs {motherboard.SocketType}.");
            }

            // check if PSU wattage is enough for the build
            if (psu != null && psu.Wattage.HasValue)
            {
                // calculate total power usage of all non-PSU components
                int totalWattage = components
                    .Where(c => c.ComponentType != "PSU")
                    .Sum(c => c.Wattage ?? 0);

                if (totalWattage > psu.Wattage.Value)
                {
                    warnings.Add($"Power supply too weak. Required: {totalWattage}W, PSU: {psu.Wattage.Value}W.");
                }
            }

            return warnings;
        }
    }
}