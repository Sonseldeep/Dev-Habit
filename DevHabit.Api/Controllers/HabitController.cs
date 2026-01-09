using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Habits;
using DevHabit.Api.Entities;
using FluentValidation;
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
    public async Task<ActionResult<HabitsCollectionDto>> GetHabits([FromQuery(Name ="q" )] string? search,
       HabitType? type, 
       HabitStatus? status)
    {
        search ??= search?.Trim().ToLower();

      
        var habits = await _dbContext
            .Habits
            .Where(h => search == null || h.Name.ToLower().Contains(search) ||
                        h.Description != null && h.Description.ToLower().Contains(search))
            .Where(h => type == null || h.Type == type)
            .Where(h => status == null || h.Status == status)
            .Select(HabitQueries.ProjectToDto())
            .ToListAsync();

        var habitsCollectionDto = new HabitsCollectionDto
        {
            Items = habits,
        };
        return Ok(habitsCollectionDto);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<HabitWithTagsDto>> GetHabit(string id)
    {
        var habit = await _dbContext
            .Habits
            .Where(h => h.Id == id)
            .Select(HabitQueries.ProjectToHabitWithTagDto()).FirstOrDefaultAsync();

        if (habit is null)
        {
            return NotFound();
        } 
        return Ok(habit);
    }


    [HttpPost]
    public async Task<ActionResult<HabitDto>> CreateHabit( [FromBody] CreateHabitDto createHabitDto, IValidator<CreateHabitDto> validator)
    {
        await validator.ValidateAndThrowAsync(createHabitDto);
        
        var habit = createHabitDto.ToEntity();
        
        _dbContext.Habits.Add(habit);
        await _dbContext.SaveChangesAsync();

        var habitDto = habit.ToDto();
        
        return CreatedAtAction(nameof(GetHabit), new { id = habitDto.Id }, habitDto );
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateHabit([FromRoute] string id,[FromBody] UpdateHabitDto updateHabitDto)
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

    [HttpDelete("{id}")]

    public async Task<ActionResult> DeleteHabit([FromRoute] string id)
    {
        var habit = await _dbContext.Habits.FirstOrDefaultAsync(h => h.Id == id);
        if (habit is null)
        {
            return NotFound(); 
           
        }

        _dbContext.Habits.Remove(habit);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }
    
}