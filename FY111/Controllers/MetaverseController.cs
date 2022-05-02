﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FY111.Models.FY111;
using Microsoft.AspNetCore.Identity;
using FY111.Areas.Identity.Data;
using FY111.Models;
using Microsoft.AspNetCore.Authorization;

namespace FY111.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MetaverseController : ControllerBase
    {
        private readonly FY111Context _context;
        private UserManager<FY111User> _userManager;
        private SignInManager<FY111User> _signInManager;
        private readonly ApplicationSettings _appSettings;

        public MetaverseController(FY111Context context)
        {
            _context = context;
        }

        // GET: api/Metaverses
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<Metaverse>>> GetMetaverses()
        //{
        //    return await _context.Metaverses.ToListAsync();
        //}

        // GET: api/Metaverses/5
        //[Authorize(Roles = "NormalUser")]
        //[HttpGet("{id}")]
        //public async Task<ActionResult<Metaverse>> GetMetaverse(int id)
        //{
        //    var metaverse = await _context.Metaverses.FindAsync(id);

        //    if (metaverse == null)
        //    {
        //        return NotFound();
        //    }

        //    return metaverse;
        //}

        //// PUT: api/Metaverses/5
        //// To protect from overposting attacks, enable the specific properties you want to bind to, for
        //// more details, see https://go.microsoft.com/fwlink/?linkid=2123754.

        [Authorize(Roles = "SuperAdmin,MetaverseAdmin")]
        [HttpPatch("Edit/{id}")]
        public async Task<IActionResult> EditMetaverse(int id, Metaverse metaverse)
        {
            if (id != metaverse.Id)
            {
                return BadRequest();
            }

            _context.Entry(metaverse).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MetaverseExists(id))
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

        // POST: api/Metaverses
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("Create")]
        public async Task<ActionResult<Metaverse>> PostMetaverse(Metaverse metaverse)
        {
            _context.Metaverses.Add(metaverse);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (MetaverseExists(metaverse.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return Ok(new {message = "Create Successfully"});
        }

        // DELETE: api/Metaverses/5
        [Authorize(Roles = "SuperAdmin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<Metaverse>> DeleteMetaverse(string id)
        {
            var metaverse = await _context.Metaverses.FindAsync(id);
            if (metaverse == null)
            {
                return NotFound();
            }

            _context.Metaverses.Remove(metaverse);
            await _context.SaveChangesAsync();

            return metaverse;
        }

        [Authorize]
        [HttpGet("list_available")]
        public async Task<ActionResult<Object>> ListAvailable(ListAvailable_Model model)
        {
            if (model.permission == 3 || model.permission == 2)
            {
                var selected_list = await _context.MetaverseSignIns
                                    .Where(x => x.MemberId == model.member_id.ToString())
                                    .Select(x => x.MetaverseId).ToListAsync();

                var selected_metaverse = await _context.Metaverses
                                        .Where(x => selected_list.Contains(x.Id)).ToListAsync(); 

                var metaverse = await _context.Metaverses
                                .Where(b => b.SigninEnabled == 1 && !selected_metaverse.Contains(b)) // 列出尚未選擇並且可選擇的元宇宙
                                .ToListAsync();

                if (metaverse == null)
                {
                    return NotFound();
                }

                return Ok(new
                {
                    selected_list = selected_metaverse,
                    none_selected_list = metaverse
                });
            }
            else if(model.permission == 1 || model.permission == 0)
            {
                var metaverse = await _context.Metaverses.ToListAsync();

                return Ok(new
                {
                    metaverse = metaverse
                });
            }

            return BadRequest();
        }

        private bool MetaverseExists(int id)
        {
            return _context.Metaverses.Any(e => e.Id == id);
        }
    }
}
