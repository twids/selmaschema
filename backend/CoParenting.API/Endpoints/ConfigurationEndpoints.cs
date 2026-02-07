using CoParenting.API.DTOs;
using CoParenting.API.Services;

namespace CoParenting.API.Endpoints;

public static class ConfigurationEndpoints
{
    public static void MapConfigurationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/config")
            .WithTags("Configuration")
            .RequireAuthorization();

        // GET /api/config/parent-names
        group.MapGet("/parent-names", async (ConfigurationService service) =>
        {
            var (parentAName, parentBName) = await service.GetParentNamesAsync();
            return Results.Ok(new ParentNamesDto(parentAName, parentBName));
        })
        .WithName("GetParentNames")
        .WithDescription("Get parent names configuration")
        .Produces<ParentNamesDto>();

        // PUT /api/config/parent-names
        group.MapPut("/parent-names", async (ParentNamesDto dto, ConfigurationService service) =>
        {
            await service.SetParentNamesAsync(dto.ParentAName, dto.ParentBName);
            return Results.Ok(dto);
        })
        .WithName("UpdateParentNames")
        .WithDescription("Update parent names configuration")
        .Produces<ParentNamesDto>();
    }
}
