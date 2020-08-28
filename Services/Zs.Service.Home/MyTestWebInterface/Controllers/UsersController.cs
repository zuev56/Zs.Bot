using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyTestWebInterface.Models;
using Zs.Bot.Model.Db;

namespace MyTestWebInterface.Controllers
{
    public class UsersController : Controller
    {
        private readonly ZsBotDbContext _context;

        public UsersController(ZsBotDbContext context)
        {
            _context = context;
        }

        // GET: Users
        //public async Task<IActionResult> Index()
        //{
        //    return View(await _context.Users.ToListAsync());
        //}

        // GET: Users
        public async Task<IActionResult> Index(string userRoleCode, string searchString)
        {
            var userRoleCodes = _context.Users
                .OrderBy(u => u.UserRoleCode)
                .Select(u => u.UserRoleCode);

            var users = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(userRoleCode))
            {
                users = users.Where(u => u.UserRoleCode == userRoleCode);
            }

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                users = users.Where(u => u.UserName.Contains(searchString)
                                      || u.UserFullName.Contains(searchString));
            }

            var userRoleVM = new UserRoleViewModel()
            {
                RoleCodes = new SelectList(await userRoleCodes.Distinct().ToListAsync()),
                Users = await users.ToListAsync()
            };

            return View(userRoleVM);
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dbUser = await _context.Users
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (dbUser == null)
            {
                return NotFound();
            }

            return View(dbUser);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,UserName,UserFullName,UserRoleCode,UserIsBot,RawData,RawDataHash,RawDataHistory,UpdateDate,InsertDate")] DbUser dbUser)
        {
            if (ModelState.IsValid)
            {
                _context.Add(dbUser);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(dbUser);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dbUser = await _context.Users.FindAsync(id);
            if (dbUser == null)
            {
                return NotFound();
            }
            return View(dbUser);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,UserName,UserFullName,UserRoleCode,UserIsBot,RawData,RawDataHash,RawDataHistory,UpdateDate,InsertDate")] DbUser dbUser)
        {
            if (id != dbUser.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dbUser);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DbUserExists(dbUser.UserId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(dbUser);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dbUser = await _context.Users
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (dbUser == null)
            {
                return NotFound();
            }

            return View(dbUser);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dbUser = await _context.Users.FindAsync(id);
            _context.Users.Remove(dbUser);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DbUserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }
    }
}
