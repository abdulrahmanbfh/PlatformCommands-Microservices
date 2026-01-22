using System.ComponentModel.DataAnnotations;

namespace PlatformService.Dtos;

public class PlatformCreateDto
{
    [Required]
    public required string Name { get; init; }

    [Required]
    public required string Cost { get; init; }

    [Required]
    public required string Publisher { get; init; }
}