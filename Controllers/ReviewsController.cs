﻿using CineMatrix_API.DTOs;
using CineMatrix_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineMatrix_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/reviews
        [HttpGet]
        public async Task<IActionResult> GetReviews()
        {
            var reviews = await _context.Reviews.Include(r => r.MovieId).Include(r => r.User).ToListAsync();
            return Ok(new { success = true, data = reviews, message = "Reviews retrieved successfully." });
        }

        // GET: api/reviews/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReview(int id)
        {
            var review = await _context.Reviews.Include(r => r.MovieId).Include(r => r.User).FirstOrDefaultAsync(r => r.Id == id);

            if (review == null)
            {
                return NotFound(new { success = false, message = "Review not found." });
            }

            return Ok(new { success = true, data = review, message = "Review retrieved successfully." });
        }

        // POST: api/reviews
        [HttpPost]
        // [Authorize(Roles = "PrimeUser,Admin")]
        public async Task<IActionResult> CreateReview([FromBody] ReviewDTO reviewDTO)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid review data.",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            try
            {
                var review = new Reviews
                {
                    MovieId = reviewDTO.MovieId,
                    UserId = reviewDTO.UserId,
                    Content = reviewDTO.Content,
                    Rating = reviewDTO.Rating
                };

                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetReview), new { id = review.Id }, new
                {
                    success = true,
                    data = review,
                    message = "Review created successfully."
                });
            }
            catch (DbUpdateException dbEx)
            {
               
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An error occurred while updating the database. Please try again later."
                });
            }
            catch (Exception ex)
            {
               
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An unexpected error occurred. Please try again later."
                });
            }
        }


        // PUT: api/reviews/{id}
        [HttpPut("{id}")]
        // [Authorize(Roles = "PrimeUser,Admin")]
        public async Task<IActionResult> UpdateReview(int id, [FromBody] ReviewDTO reviewDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid review data.", errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
            }

            var review = await _context.Reviews.FindAsync(id);

            if (review == null)
            {
                return NotFound(new { success = false, message = "Review not found." });
            }

            review.MovieId = reviewDTO.MovieId;
            review.UserId = reviewDTO.UserId;
            review.Content = reviewDTO.Content;
            review.Rating = reviewDTO.Rating;

            _context.Entry(review).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/reviews/{id}
        [HttpDelete("{id}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);

            if (review == null)
            {
                return NotFound(new { success = false, message = "Review not found." });
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
