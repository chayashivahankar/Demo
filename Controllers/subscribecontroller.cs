﻿using CineMatrix_API.DTOs;
using CineMatrix_API.Enums;
using CineMatrix_API.Models;
using CineMatrix_API.Repository;
using CineMatrix_API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineMatrix_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscribeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly OtpService _otpService;
        private readonly ISMSService _smsService;

        public SubscribeController(
            ApplicationDbContext context,
            OtpService otpService,
            ISMSService smsService)
        {
            _context = context;
            _otpService = otpService;
            _smsService = smsService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateSubscription([FromBody] subscribecreationdto subscriptionDto)
        {
            if (subscriptionDto == null || string.IsNullOrEmpty(subscriptionDto.PhoneNumber))
            {
                return BadRequest("Subscription data is not valid.");
            }


            var existingSubscription = await _context.Subscribes
                .FirstOrDefaultAsync(s => s.PhoneNumber == subscriptionDto.PhoneNumber);

            if (existingSubscription != null)
            {
                return BadRequest("Subscription already exists with this phone number.");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == subscriptionDto.email); 

            if (user == null)
            {
                return BadRequest("User not found with the provided username.");
            }

     
            var subscription = new Subscribe
            {
                UserId = user.Id,
                PhoneNumber = subscriptionDto.PhoneNumber,
                IsVerified = false,
                IsPaymentSuccessful = false
            };

            await _context.Subscribes.AddAsync(subscription);
            await _context.SaveChangesAsync();

          
            var otpCode = await _otpService.GenerateOTP();
            await _otpService.SaveOtpAsync(user.Id, otpCode, OTPType.Phonenumberverification);
            await _smsService.SendOtpSmsAsync(subscriptionDto.PhoneNumber, otpCode);

            return Ok("Subscription created. Please verify your phone number.");
        }


        [HttpPost("verify")]
        public async Task<IActionResult> VerifySubscription([FromBody] subscribeverificationdto verificationDto)
        {
            if (verificationDto == null || string.IsNullOrEmpty(verificationDto.Code))
            {
                return BadRequest("Verification data is not valid.");
            }

      
            var otp = await _context.OTP
                .FirstOrDefaultAsync(o => o.Code == verificationDto.Code
                                           && o.OtpType == OTPType.Phonenumberverification
                                           && !o.IsUsed
                                           && o.ExpiryDate > DateTime.UtcNow);

            if (otp == null)
            {
                return BadRequest("Invalid or expired OTP code.");
            }

      
            otp.IsUsed = true;
            _context.OTP.Update(otp);
            await _context.SaveChangesAsync();

       
            var subscription = await _context.Subscribes
                .FirstOrDefaultAsync(s => s.UserId == otp.UserId);

            if (subscription == null)
            {
                return BadRequest("Subscription not found.");
            }

            subscription.IsVerified = true;
            subscription.IsPaymentSuccessful = true; 
            _context.Subscribes.Update(subscription);
            await _context.SaveChangesAsync();

            return Ok("Subscription verified and payment status updated to successful.");
        }


    }
}
