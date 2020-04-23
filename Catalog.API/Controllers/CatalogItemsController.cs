using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Catalog.API.Infrastructure;
using Catalog.API.Model;
using Catalog.API.IntegrationEvents;
using Catalog.API.IntegrationEvents.Events;

namespace Catalog.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatalogItemsController : ControllerBase
    {
        private readonly CatalogContext _context;
        private readonly ICatalogIntegrationEventService catalogIntegrationEventService;

        public CatalogItemsController(CatalogContext context, ICatalogIntegrationEventService catalogIntegrationEventService)
        {
            _context = context;
            this.catalogIntegrationEventService = catalogIntegrationEventService;
        }

        // GET: api/CatalogItems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CatalogItem>>> GetCatalogItems()
        {
            return await _context.CatalogItems.ToListAsync();
        }

        // GET: api/CatalogItems/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CatalogItem>> GetCatalogItem(int id)
        {
            var catalogItem = await _context.CatalogItems.FindAsync(id);

            if (catalogItem == null)
            {
                return NotFound();
            }

            return catalogItem;
        }

        // PUT: api/CatalogItems/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCatalogItem(int id, CatalogItem productToUpdate)
        {
            if (id != productToUpdate.Id)
            {
                return BadRequest();
            }


            try
            {
                var catalogItem = await _context.CatalogItems.SingleOrDefaultAsync(i => i.Id == productToUpdate.Id);

                var oldPrice = catalogItem.Price;
                var raiseProductPriceChangedEvent = oldPrice != productToUpdate.Price;

                if (raiseProductPriceChangedEvent)
                {
                    var priceChangedEvent = new ProductPriceChangedIntegrationEvent(catalogItem.Id, productToUpdate.Price, oldPrice);
                    catalogIntegrationEventService.PublishThroughEventBusAsync(priceChangedEvent);
                }
                _context.Entry(productToUpdate).State = EntityState.Modified;

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CatalogItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/CatalogItems
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<CatalogItem>> PostCatalogItem(CatalogItem catalogItem)
        {
            _context.CatalogItems.Add(catalogItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCatalogItem", new { id = catalogItem.Id }, catalogItem);
        }

        // DELETE: api/CatalogItems/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<CatalogItem>> DeleteCatalogItem(int id)
        {
            var catalogItem = await _context.CatalogItems.FindAsync(id);
            if (catalogItem == null)
            {
                return NotFound();
            }

            _context.CatalogItems.Remove(catalogItem);
            await _context.SaveChangesAsync();

            return catalogItem;
        }

        private bool CatalogItemExists(int id)
        {
            return _context.CatalogItems.Any(e => e.Id == id);
        }
    }
}
