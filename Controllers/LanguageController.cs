﻿using AutoMapper;
using CineMatrix_API;
using CineMatrix_API.DTOs;
using CineMatrix_API.Helpers;
using CineMatrix_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class LanguageController : ControllerBase
{
    private readonly ApplicationDbContext context;
    private readonly IMapper mapper;

    public LanguageController(ApplicationDbContext context, IMapper mapper)
    {
        this.context = context;
        this.mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<List<LanguageDTO>>> Get([FromQuery] PaginationDTO pagination)
    {
        var queryable = context.Languages.AsQueryable();
        await HttpContext.InsertPaginationParametersInResponse(queryable, pagination.RecordsPerPage);
        var languages = await queryable.Paginate(pagination).ToListAsync();
        var languageDTOs = mapper.Map<List<LanguageDTO>>(languages);
        return Ok(languageDTOs);
    }

    [HttpGet("{id}", Name = "getLanguage")]
    public async Task<ActionResult<LanguageDTO>> GetById(int id)
    {
        var language = await context.Languages.FirstOrDefaultAsync(x => x.Id == id);
        if (language == null)
        {
            return NotFound();
        }
        return mapper.Map<LanguageDTO>(language);
    }

    [HttpPost]
    public async Task<ActionResult> Post([FromBody] LanguageCreationDTO languageCreationDto)
    {
        if (languageCreationDto == null)
        {
            return BadRequest("LanguageCreationDTO is required.");
        }

        try
        {
            // Ensure the language name is unique
            var existingLanguage = await context.Languages
                .Where(l => l.Name.ToLower() == languageCreationDto.Name.ToLower())
                .FirstOrDefaultAsync();

            if (existingLanguage != null)
            {
                return Conflict("A language with the same name already exists.");
            }

            var language = new Language
            {
                Name = languageCreationDto.Name
            };

       
            context.Languages.Add(language);
            await context.SaveChangesAsync();

         
            var languageDto = mapper.Map<LanguageDTO>(language);
           
            return CreatedAtRoute("getLanguage", new { id = language.Id }, new
            {
                message = "Language created successfully!",
                language = languageDto
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateLanguage(int id, [FromBody] LanguageCreationDTO languageUpdateDto)
    {
        try
        {
            var existingLanguage = await context.Languages
                .FirstOrDefaultAsync(l => l.Name.ToLower() == languageUpdateDto.Name.ToLower() && l.Id != id);

            if (existingLanguage != null)
            {
                return Conflict(new { message = "A language with the same name already exists. Please choose a different name." });
            }

            var language = await context.Languages.FindAsync(id);

            if (language == null)
            {
                return NotFound();
            }


            language.Name = languageUpdateDto.Name;

        
            context.Entry(language).State = EntityState.Modified;

  
            await context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLanguage(int id)
    {
        try
        {
            var language = await context.Languages.FindAsync(id);

            if (language == null)
            {
                return NotFound();
            }

            context.Languages.Remove(language);
            await context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
