using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masareef.MasareefEntities.DTOs
{
    public class CreateDebtDto
    {

        public int UserID { get; set; }
        public int? BusinessID { get; set; }   // null = دين شخصي
        public string PersonName { get; set; } = null!;
        public string? PersonPhone { get; set; }

        public byte Direction { get; set; }   // 1 = عليّ | 2 = ليّ
        public decimal Amount { get; set; }

        public DateTime DebtDate { get; set; }
        public DateTime? DueDate { get; set; }   // اختياري

        public string? Description { get; set; }

    }
}
