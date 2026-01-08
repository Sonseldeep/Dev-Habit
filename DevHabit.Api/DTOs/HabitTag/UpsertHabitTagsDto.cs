namespace DevHabit.Api.DTOs.HabitTag;

public record UpsertHabitTagsDto
{
    public required List<string> TagIds { get; init; }
}