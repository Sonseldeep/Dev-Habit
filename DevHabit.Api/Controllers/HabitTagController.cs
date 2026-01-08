using DevHabit.Api.Database;
using DevHabit.Api.DTOs.HabitTag;
using DevHabit.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Controllers;

[ApiController]
[Route("api/habits/{habitId}/tags")]

public sealed class HabitTagController(ApplicationDbContext dbContext) : ControllerBase
{

    [HttpPut]
    public async Task<IActionResult> UpsertHabitTag(string habitId, UpsertHabitTagsDto upsertHabitTagsDto)
    {
        var habit = await dbContext.Habits.Include(h => h.HabitTags).FirstOrDefaultAsync(h => h.Id == habitId);
        if (habit is null)
        {
            return NotFound();
        }

        var currentTagIds = habit.HabitTags.Select(ht => ht.TagId).ToHashSet();
        if (currentTagIds.SetEquals(upsertHabitTagsDto.TagIds))
        {
            return NoContent();
        }
        
        var existingTagIds = await dbContext.Tags.Where(t => upsertHabitTagsDto.TagIds.Contains(t.Id)).ToListAsync();

        if (existingTagIds.Count != upsertHabitTagsDto.TagIds.Count)
        {
            return BadRequest("One or more tags IDs is invalid.");
        }

        habit.HabitTags.RemoveAll(ht => !upsertHabitTagsDto.TagIds.Contains(ht.TagId));
        
        var tagIdsToAdd = upsertHabitTagsDto.TagIds.Except(currentTagIds).ToArray();
        
        habit.HabitTags.AddRange(tagIdsToAdd.Select(tagId => new HabitTag
        {
            HabitId = habitId,
            TagId = tagId,
            CreatedAtUtc = DateTime.UtcNow
        }));
        
        await dbContext.SaveChangesAsync();
        
        return Ok();
    }

    [HttpDelete("{tagId}")]
    public async Task<IActionResult> DeleteHabitTag(string habitId, string tagId)
    {
        var habitTag = await dbContext.HabitTags.SingleOrDefaultAsync(ht => ht.HabitId == habitId && ht.TagId == tagId);
        if (habitTag is null)
        {
            return NotFound();
        }
        dbContext.HabitTags.Remove(habitTag);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }
}