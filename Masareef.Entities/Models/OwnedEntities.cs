using Masareef.Entities.Common;

namespace Masareef.Entities.Models;

// بيربط كل Entity ليها مالك مباشر (عمود UserId) بالـ IUserOwned interface،
// من غير ما نلمس الملفات المولّدة (scaffolded) بتاعتها.
public partial class Business : IUserOwned { }
public partial class Debt : IUserOwned { }
public partial class HomeExpense : IUserOwned { }
public partial class HomeIncome : IUserOwned { }
