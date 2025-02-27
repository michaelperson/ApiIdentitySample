﻿using System.ComponentModel.DataAnnotations;

namespace BlazorAuthApp.Shared.Models;

public class RegisterRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; }
}
