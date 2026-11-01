namespace EloVet.Api.Models;

public record HateoasResource<T>(T Data, IReadOnlyCollection<LinkDto> Links);