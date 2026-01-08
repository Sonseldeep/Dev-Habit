using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Tags;
using DevHabit.Api.Extensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers(options =>
{
    options.ReturnHttpNotAcceptable = true;
}).AddXmlSerializerFormatters();



// fluent validation 
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
    };
});

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();  

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("Database"),
    useSqlServer => useSqlServer.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schema.Application))
);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseSwagger();
    app.UseSwaggerUI();
    await app.ApplyMigrationsAsync();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();