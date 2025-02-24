using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Yorit.Api.Data.Models;
using Yorit.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Yorit.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageSourcesController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public ImageSourcesController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: /api/imagesources
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ImageSource>>> GetImageSources()
        {
            return await _dbContext.ImageSources.ToListAsync();
        }

        // GET: /api/imagesources/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ImageSource>> GetImageSourceById(Guid id)
        {
            var imageSource = await _dbContext.ImageSources.FindAsync(id);

            if (imageSource == null)
            {
                return NotFound();
            }

            return imageSource;
        }


        // POST: /api/imagesources
        [HttpPost]
        public async Task<ActionResult<ImageSource>> AddImageSource([FromBody] ImageSource imageSource)
        {
            if (string.IsNullOrWhiteSpace(imageSource.Path))
            {
                return BadRequest("Path is required.");
            }

            imageSource.Id = Guid.NewGuid();
            imageSource.DateAdded = DateTime.UtcNow;

            _dbContext.ImageSources.Add(imageSource);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetImageSourceById), new { id = imageSource.Id }, imageSource);
        }

        // DELETE: /api/imagesources/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteImageSource(Guid id)
        {
            var imageSource = await _dbContext.ImageSources.FindAsync(id);
            if (imageSource == null)
            {
                return NotFound();
            }

            _dbContext.ImageSources.Remove(imageSource);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
