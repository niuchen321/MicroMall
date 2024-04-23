using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Entitys;
using MicroMall.Data;
using IServices;
using Services;

namespace MicroMall.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoodsController : ControllerBase
    {
        private readonly MicroMallContext _context;

        public GoodsController(MicroMallContext context)
        {
            _context = context;
        }

        // GET: api/GoodsEntities
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GoodsEntity>>> GetGoodsEntity()
        {
            return await _context.GoodsEntity.ToListAsync();
        }

        // GET: api/GoodsEntities/5
        [HttpGet("{id}")]
        public async Task<ActionResult<GoodsEntity>> GetGoodsEntity(string id)
        {
            var goodsEntity = await _context.GoodsEntity.FindAsync(id);

            if (goodsEntity == null)
            {
                return NotFound();
            }

            return goodsEntity;
        }

        // PUT: api/GoodsEntities/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGoodsEntity(string id, GoodsEntity goodsEntity)
        {
            if (id != goodsEntity.Id)
            {
                return BadRequest();
            }

            _context.Entry(goodsEntity).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GoodsEntityExists(id))
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

        // POST: api/GoodsEntities
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<GoodsEntity>> PostGoodsEntity(GoodsEntity goodsEntity)
        {
            _context.GoodsEntity.Add(goodsEntity);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (GoodsEntityExists(goodsEntity.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetGoodsEntity", new { id = goodsEntity.Id }, goodsEntity);
        }

        // DELETE: api/GoodsEntities/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGoodsEntity(string id)
        {
            var goodsEntity = await _context.GoodsEntity.FindAsync(id);
            if (goodsEntity == null)
            {
                return NotFound();
            }

            _context.GoodsEntity.Remove(goodsEntity);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool GoodsEntityExists(string id)
        {
            IGoodsService goodsService=new GoodsService();

            return _context.GoodsEntity.Any(e => e.Id == id);
        }
    }
}
