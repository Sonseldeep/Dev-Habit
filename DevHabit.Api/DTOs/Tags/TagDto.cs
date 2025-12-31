namespace DevHabit.Api.DTOs.Tags;

public sealed record TagCollectionDto
{
    public List<TagDto>? Items { get; init; }
}
public sealed record TagDto
{
    public string Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
}