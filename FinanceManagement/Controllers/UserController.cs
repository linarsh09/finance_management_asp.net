using FinanceManagement.Data;
using FinanceManagement.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Controllers
{
    public class UserController : Controller
    {
        private readonly FinanceDbContext _dbContext = null!;

        public UserController(FinanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost]
        public async Task<IActionResult> Registry(CreateUserDto dto)
        {
            _dbContext.Add(dto);
            var isAdded = await _dbContext.SaveChangesAsync();
            if (isAdded > 0)
            {
                return Ok();
            }
            return BadRequest();
        }
    }
}
