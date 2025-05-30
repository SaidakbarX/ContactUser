using ContactUser.Application.Services;
using Microsoft.AspNetCore.Authorization;

namespace ContactUser.Api.Endpoints;

public static class RoleEndpoints
{
    public static void MapRoleEndpoints(this WebApplication app)
    {
        var userGroup = app.MapGroup("/api/role")
            .RequireAuthorization()
            .WithTags("UserRole Management");

        userGroup.MapGet("/get-all-roles", [Authorize(Roles = "Admin, SuperAdmin")]
        async (IRoleService _roleService) =>
        {
            var roles = await _roleService.GetAllRolesAsync();
            return Results.Ok(roles);
        })
        .WithName("GetAllRoles");

        userGroup.MapGet("/GetAllUsersByRole", [Authorize(Roles = "Admin, SuperAdmin")]
        async (string role,IRoleService _roleService) =>
        {
            var users = await _roleService.GetAllUsersByRoleAsync(role);
            return Results.Ok(users);
        })
        .WithName("GetAllUsers_ByRole");
    }
}
