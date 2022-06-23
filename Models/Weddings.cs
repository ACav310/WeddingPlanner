#pragma warning disable CS8618
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeddingPlanner.Models;
public class Wedding
{
    [Required]
    public int WeddingId {get;set;}
    [Required]
    public string wedderOne {get;set;}
    [Required]
    public string wedderTwo {get;set;}
    [Required]
    [FutureDate]
    public DateTime Date {get;set;}
    [Required]
    public string Address {get;set;}
    [Required]
    public int UserId {get;set;}
    public User? User {get;set;}
    public List<Guest> Guests {get;set;} = new List<Guest>();
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

public class FutureDateAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return new ValidationResult("Please provide a date for the wedding");
        }
        if (((DateTime)value) < DateTime.Now)
        {
            return new ValidationResult("Date must be in the future");
        }
        return ValidationResult.Success;
    }
}