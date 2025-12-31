using System.Linq.Expressions;
using DevHabit.Api.DTOs.Habits;
using DevHabit.Api.Entities;

namespace DevHabit.Api.DTOs.Tags;

public class TagQueries
{
    public static Expression<Func<Tag, TagDto>> ProjectToDto()
    {
        return t => new TagDto
        {
            Id = t.Id,
            Name = t.Name,
            Description = t.Description,
            CreatedAtUtc = t.CreatedAtUtc,
            UpdatedAtUtc = t.UpdatedAtUtc,
        };
    }
}