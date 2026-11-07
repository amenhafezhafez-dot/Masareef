using Microsoft.AspNetCore.Authorization;

namespace Masareef.Api.Authorization;

/// <summary>شرط: المستخدم لازم يكون Role=Admin. مفيش بيانات إضافية — مجرد علامة (marker).</summary>
public class AdminRequirement : IAuthorizationRequirement
{
}
