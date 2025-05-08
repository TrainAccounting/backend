using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trainacc.Data;
using Trainacc.Models;
using Trainacc.Filters;

namespace Trainacc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(LogActionFilter))]
    public class CreditsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CreditsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("by-record/{recordId}")]
        public async Task<ActionResult<IEnumerable<CreditDto>>> GetCreditsByRecord(int recordId)
        {
            return await _context.Credits
                .Where(c => c.RecordId == recordId)
                .Select(c => new CreditDto
                {
                    Id = c.Id,
                    NameOfCredit = c.NameOfCredit,
                    CreditCurrentValue = c.CreditCurrentValue,
                    DateOfOpening = c.DateOfOpening,
                    PeriodOfPayment = c.PeriodOfPayment,
                    InterestRate = c.InterestRate,
                    PayType = c.PayType
                })
                .ToListAsync();
        }
    }
}