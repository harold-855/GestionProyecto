using Microsoft.AspNetCore.Routing;

namespace GestionProyecto.Api.Routing;

/// <summary>
/// Transforma los tokens de ruta (como [controller] y [action]) a minúsculas.
/// </summary>
public sealed class LowercaseParameterTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value)
    {
        return value?.ToString()?.ToLowerInvariant();
    }
}
