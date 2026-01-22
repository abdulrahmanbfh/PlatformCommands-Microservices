namespace PlatformService.Dtos;

public class PlatformReadDto
{

    public int Id { get; init; }

    public required string Name { get; init; }

    public required string Cost { get; init; }

    public required string Publisher { get; init; }
}