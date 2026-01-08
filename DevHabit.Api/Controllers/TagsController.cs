using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Tags;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Controllers;

[ApiController]
[Route("api/tags")]

public sealed class TagsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<TagCollectionDto>> GetTags()
    {
        var tags = await dbContext
            .Tags
            .Select(TagQueries.ProjectToDto())
            .ToListAsync();

        var tagCollectionDto = new TagCollectionDto
        {
            Items = tags
        };
        return Ok(tagCollectionDto);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TagDto>> GetTag([FromRoute] string id)
    {
        var tag = await dbContext
            .Tags
            .Where(t => t.Id == id)
            .Select(TagQueries.ProjectToDto())
            .FirstOrDefaultAsync();

        return tag is null 
            ? NotFound()
            : Ok(tag);
        
    }

    [HttpPost]
    public async Task<ActionResult<TagDto>> CreateTag([FromBody] CreateTagDto createTagDto, IValidator<CreateTagDto> validator,ProblemDetailsFactory problemDetailsFactory)
    {
        var validateResult = await validator.ValidateAsync(createTagDto);
        if (!validateResult.IsValid)
        {
            var problem = problemDetailsFactory.CreateProblemDetails(HttpContext, StatusCodes.Status400BadRequest);
            
            problem.Extensions.Add("errors", validateResult.ToDictionary());
            return BadRequest(problem);
        }
        
        var tag = createTagDto.ToEntity();
        if (await dbContext.Tags.AnyAsync(t => t.Name == tag.Name))
        {
            return Problem(detail: $"The tag '{tag.Name}' already exists.", statusCode: StatusCodes.Status409Conflict);
        }

        dbContext.Tags.Add(tag);
        await dbContext.SaveChangesAsync();
        var tagDto = tag.ToDto();
        return CreatedAtAction(nameof(GetTag), new { id = tagDto.Id }, tagDto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateTag([FromRoute] string id, [FromBody] UpdateTagDto updateTagDto)
    {
        var tag = await dbContext.Tags.FirstOrDefaultAsync(t=>t.Id == id);
        if (tag is null)
        {
            return NotFound();
        }
        tag.UpdateFromDto(updateTagDto);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTag([FromRoute] string id)
    {
        var tag = await dbContext.Tags.FirstOrDefaultAsync(t => t.Id == id);
        if (tag is null)
        {
            return NotFound();
        }
        dbContext.Tags.Remove(tag);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }
    
}