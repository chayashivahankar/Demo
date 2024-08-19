﻿using CinematrixAPI.Enums;

namespace CineMatrix_API.Models
{
    public class Subscription
    {
        public int Id { get; set; }
        public SubscriptionType SubscriptionType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }

    }
}
