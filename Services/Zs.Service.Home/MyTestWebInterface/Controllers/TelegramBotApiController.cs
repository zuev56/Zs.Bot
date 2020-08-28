using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MyTestWebInterface.Models;
using Zs.Bot.Model.Db;
using Zs.Common.Extensions;

namespace MyTestWebInterface.Controllers
{
    public class TelegramBotApiController : Controller
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        private readonly ZsBotDbContext _context;
        private TelegramBotApiViewModel _viewModel;


        public TelegramBotApiController(ZsBotDbContext context, IConfiguration configuration)
        {
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            //_httpClient.DefaultRequestHeaders.Accept.Add(
            //    new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            //
            //_httpClient.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");

            _context = context;
            _viewModel = new TelegramBotApiViewModel()
            {
                BotToken = configuration["BotToken"]
            };
        }

        // GET: TelegramBotApi
        public async Task<IActionResult> Index()
        {
            return View(_viewModel);
        }

        // GET: TelegramBotApi/Request/GetMe
        //[Route("TelegramBotApi/Request/{methodName?}")]
        public async Task<IActionResult> Request(string id)
        {
            _viewModel.MethodName = id;
            _viewModel.Response = await GetApiAnswer(_viewModel.BotToken, _viewModel.MethodName);
            _viewModel.Response = _viewModel.Response.NormalizeJsonString();

            return View(_viewModel);
        }

        private async Task<string> GetApiAnswer(string botToken, string methodName)
        {
            return await _httpClient.GetStringAsync($"https://api.telegram.org/bot{botToken}/{methodName}");
        }

        // GET: TelegramBotApi/Details/5
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

        // GET: TelegramBotApi/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TelegramBotApi/Create
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

        // GET: TelegramBotApi/Edit/5
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

        // POST: TelegramBotApi/Edit/5
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

        // GET: TelegramBotApi/Delete/5
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

        // POST: TelegramBotApi/Delete/5
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
