namespace EloVet.Api.Models;

public record PagedResponse<T>(
    IEnumerable<T> Items,
    int PageNumber,
    int PageSize,
    int TotalPages,
    long TotalRecords,
    IReadOnlyCollection<LinkDto> Links);