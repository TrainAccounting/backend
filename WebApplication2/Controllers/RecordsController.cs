using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trainacc.Data;
using Trainacc.Models;

//namespace Trainacc.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class RecordsController : ControllerBase
//    {
//        private readonly AppDbContext _context;

//        public RecordsController(AppDbContext context)
//        {
//            _context = context;
//        }

//        [HttpGet("{id}")]
//        public async Task<ActionResult<Record>> GetRecord(int id)
//        {
//            var record = await _context.Records
//                .Include(r => r.Restrictions)
//                .Include(r => r.Accounts)
//                .FirstOrDefaultAsync(r => r.Id == id);

//            return record;
//        }

//        [HttpPost]
//        public async Task<ActionResult<Record>> CreateRecord(Record record)
//        {
//            _context.Records.Add(record);
//            await _context.SaveChangesAsync();
//            return CreatedAtAction(nameof(GetRecord), new { id = record.Id }, record);
//        }
//    }
//}