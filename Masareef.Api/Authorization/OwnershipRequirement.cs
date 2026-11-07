using Microsoft.AspNetCore.Authorization;

namespace Masareef.Api.Authorization;

/// <summary>شرط: المستخدم لازم يكون صاحب المورد (resource) اللي بيحاول يوصله. مجرد علامة (marker).</summary>
public class OwnershipRequirement : IAuthorizationRequirement
{
}
