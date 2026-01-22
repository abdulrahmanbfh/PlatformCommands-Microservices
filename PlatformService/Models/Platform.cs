using System.ComponentModel.DataAnnotations;

namespace PlatformService.Models;

public class Platform
{
    [Key]
    [Required]
    public int Id { get; init; }

    [Required]
    public required string Name { get; init; }

    [Required]
    public required string Cost { get; init; }

    [Required]
    public required string Publisher { get; init; }
}