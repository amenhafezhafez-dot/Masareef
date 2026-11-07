using System.Security.Claims;
using Masareef.Entities.Common;
using Microsoft.AspNetCore.Authorization;

namespace Masareef.Api.Authorization;

/// <summary>
/// Resource-based handler: بيتحقق إن الـ UserId بتاع المورد (Debt, Business, ...) يطابق
/// صاحب التوكن الحالي. الـ Admin بيتخطى الشرط ده تلقائيًا.
/// لازم تستخدمه عن طريق IAuthorizationService.AuthorizeAsync(User, resource, "ResourceOwner")
/// بعد ما تكون حمّلت المورد من الداتابيز — مش بالـ [Authorize] attribute.
/// </summary>
public class OwnershipRequirementHandler
    : AuthorizationHandler<OwnershipRequirement, IUserOwned>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OwnershipRequirement requirement,
        IUserOwned resource)
    {
        var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userIdClaim != null && int.Parse(userIdClaim) == resource.UserId)
        {
            context.Succeed(requirement);
        }
        else if (context.User.IsInRole(Roles.Admin))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
