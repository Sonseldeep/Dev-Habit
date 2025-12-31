using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Habits;
using DevHabit.Api.Entities;
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
    public async Task<ActionResult<HabitsCollectionDto>> GetHabits()
    {   
        var habits = await _dbContext
            .Habits
            .Select(HabitQueries.ProjectToDto())
            .ToListAsync();

        var habitsCollectionDto = new HabitsCollectionDto
        {
            Items = habits,
        };
        return Ok(habitsCollectionDto);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<HabitDto>> GetHabit(string id)
    {
        var habit = await _dbContext
            .Habits
            .Where(h => h.Id == id)
            .Select(HabitQueries.ProjectToDto()).FirstOrDefaultAsync();

        if (habit is null)
        {
            return NotFound();
        } 
        return Ok(habit);
    }


    [HttpPost]
    public async Task<ActionResult<HabitDto>> CreateHabit( [FromBody] CreateHabitDto createHabitDto)
    {
        var habit = createHabitDto.ToEntity();
        _dbContext.Habits.Add(habit);
        await _dbContext.SaveChangesAsync();

        var habitDto = habit.ToDto();
        
        return CreatedAtAction(nameof(GetHabit), new { id = habitDto.Id }, habitDto );
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateHabit(string id,UpdateHabitDto updateHabitDto)
    {
        var habit = await _dbContext.Habits.FirstOrDefaultAsync(h => h.Id == id);
        if (habit is null)
        {
            return NotFound();
        }

        habit.UpdateFromDto(updateHabitDto);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }
    
}