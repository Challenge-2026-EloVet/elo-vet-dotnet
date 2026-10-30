namespace EloVet.Application.Models;

public record PagedResult<T>(
    IEnumerable<T> Items,
    int PageNumber,
    int PageSize,
    int TotalPages,
    long TotalRecords);