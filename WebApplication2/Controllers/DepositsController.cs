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
    public class DepositsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DepositsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("by-record/{recordId}")]
        public async Task<ActionResult<IEnumerable<DepositDto>>> GetDepositsByRecord(int recordId)
        {
            return await _context.Deposits
                .Where(d => d.RecordId == recordId)
                .Select(d => new DepositDto
                {
                    Id = d.Id,
                    NameOfDeposit = d.NameOfDeposit,
                    DepositStartValue = d.DepositStartValue,
                    DepositCurrentValue = d.DepositCurrentValue,
                    DateOfOpening = d.DateOfOpening,
                    PeriodOfPayment = d.PeriodOfPayment,
                    InterestRate = d.InterestRate,
                    Capitalisation = d.Capitalisation,
                    PayType = d.PayType
                })
                .ToListAsync();
        }
    }
}