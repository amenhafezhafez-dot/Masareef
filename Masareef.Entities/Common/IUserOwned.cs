namespace Masareef.Entities.Common;

/// <summary>أي Entity ليه مالك مباشر (عمود UserId) بيطبّق الـ interface ده عشان يشتغل مع OwnershipRequirementHandler</summary>
public interface IUserOwned
{
    int UserId { get; }
}
