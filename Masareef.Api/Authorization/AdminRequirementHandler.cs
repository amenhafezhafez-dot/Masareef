using Masareef.Entities.Common;
using Microsoft.AspNetCore.Authorization;

namespace Masareef.Api.Authorization;

/// <summary>
/// بيتأكد إن الـ Role claim جوه التوكن = Admin.
/// الـ claim نفسه متولّد في TokenService وقت اللوجين — مينفعش المستخدم يغيّره لإنه موقّع (signed).
/// </summary>
public class AdminRequirementHandler : AuthorizationHandler<AdminRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, AdminRequirement requirement)
    {
        if (context.User.IsInRole(Roles.Admin))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
