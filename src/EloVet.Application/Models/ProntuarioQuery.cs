namespace EloVet.Application.Models;

public class ProntuarioQuery
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? PetId { get; set; }
    public string? PetName { get; set; }
    public string? Species { get; set; }
    public string? Breed { get; set; }
    public string? Sex { get; set; }
    public bool? IsCastrado { get; set; }
    public string SortBy { get; set; } = "petName";
    public bool Ascending { get; set; } = true;

    public void Normalize()
    {
        PageNumber = Math.Max(PageNumber, 1);
        PageSize = Math.Clamp(PageSize, 1, 50);
        SortBy = SortBy.Trim().ToLowerInvariant();
    }
}