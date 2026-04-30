using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BuildForgeApp.Data;
using BuildForgeApp.Models;

namespace BuildForgeApp.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController] // enables automatic model validation + API behavior (JSON responses, etc.)
    public class PcComponentsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        // inject database context so we can query/update components
        public PcComponentsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET api/PcComponentsApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PcComponent>>> GetComponents()
        {
            // returns only active components (soft-deleted ones are hidden)
            return await _context.PcComponents
                .Where(c => c.IsActive)
                .ToListAsync();
        }

        // GET api/PcComponentsApi/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PcComponent>> GetComponent(int id)
        {
            // FindAsync looks up by primary key
            var component = await _context.PcComponents.FindAsync(id);

            // also ensures inactive (soft-deleted) components are not returned
            if (component == null || !component.IsActive)
            {
                return NotFound();
            }

            return component; // automatically serialized to JSON
        }

        // POST api/PcComponentsApi
        [HttpPost]
        public async Task<ActionResult<PcComponent>> CreateComponent(PcComponent component)
        {
            // validates model based on data annotations (Required, Range, etc.)
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // adds new component to database
            _context.PcComponents.Add(component);
            await _context.SaveChangesAsync();

            // returns 201 Created + location header pointing to GET endpoint
            return CreatedAtAction(nameof(GetComponent), new { id = component.Id }, component);
        }

        // PUT api/PcComponentsApi/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComponent(int id, PcComponent component)
        {
            // prevents updating the wrong record (URL id must match body id)
            if (id != component.Id)
            {
                return BadRequest();
            }

            // validates incoming data
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // tells EF Core this object is already existing and should be updated
            _context.Entry(component).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent(); // standard response for successful update
        }

        // DELETE api/PcComponentsApi/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComponent(int id)
        {
            var component = await _context.PcComponents.FindAsync(id);

            if (component == null)
            {
                return NotFound();
            }

            // soft delete: keeps data but hides it from queries
            component.IsActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}