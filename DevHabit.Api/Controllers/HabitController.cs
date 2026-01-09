using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Habits;
using DevHabit.Api.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Controllers;

[ApiController]
[Route("api/habits")]
public sealed class HabitController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<HabitsCollectionDto>> GetHabits( [FromQuery] HabitsQueryParameter query)
    {
        // for search, trim and convert to lower case
        query.Search ??= query.Search?.Trim().ToLower();

      
        var habits = await dbContext
            .Habits
            .Where(h => query.Search == null || h.Name.ToLower().Contains(query.Search) ||
                        h.Description != null && h.Description.ToLower().Contains(query.Search)) // filter by search
            .Where(h => query.Type == null || h.Type == query.Type) // filter by type
            .Where(h => query.Status == null || h.Status == query.Status) // filter by status
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
        var habit = await dbContext
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
        
        dbContext.Habits.Add(habit);
        await dbContext.SaveChangesAsync();

        var habitDto = habit.ToDto();
        
        return CreatedAtAction(nameof(GetHabit), new { id = habitDto.Id }, habitDto );
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateHabit([FromRoute] string id,[FromBody] UpdateHabitDto updateHabitDto)
    {
        var habit = await dbContext.Habits.FirstOrDefaultAsync(h => h.Id == id);
        if (habit is null)
        {
            return NotFound();
        }

        habit.UpdateFromDto(updateHabitDto);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]

    public async Task<ActionResult> DeleteHabit([FromRoute] string id)
    {
        var habit = await dbContext.Habits.FirstOrDefaultAsync(h => h.Id == id);
        if (habit is null)
        {
            return NotFound(); 
           
        }

        dbContext.Habits.Remove(habit);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }
    
}