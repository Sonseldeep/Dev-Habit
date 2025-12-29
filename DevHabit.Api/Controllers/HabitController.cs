using DevHabit.Api.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Controllers;

[ApiController]
[Route("api/habits")]
public sealed class HabitController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public HabitController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetHabits()
    {
        var habits = await _dbContext.Habits.ToListAsync();
        return Ok(habits);
    }
}