using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masareef.MasareefEntities.DTOs
{
    public class DebtSummaryDto
    {

    /// <summary>
    /// Output — صف واحد في قائمة الديون.
    /// Outstanding و IsOverdue محسوبين — مش مخزّنين.
    /// </summary>
   
        public int DebtID { get; set; }
        public string PersonName { get; set; } = null!;
        public string? PersonPhone { get; set; }
        public byte Direction { get; set; }
        public decimal Amount { get; set; }
        public decimal TotalPaid { get; set; }   // SQL بيحسبها بـ SUM
        public DateTime DebtDate { get; set; }
        public DateTime? DueDate { get; set; }
        public byte Status { get; set; }
        public int PaymentsCount { get; set; }

        // ---------- محسوبة (=> مش { get; set; }) ----------

        /// <summary>المتبقي — بيتحسب كل مرة، مستحيل يبوظ</summary>
        public decimal Outstanding => Amount - TotalPaid;

        /// <summary>متأخر؟ لازم 3 شروط</summary>
        public bool IsOverdue =>
            DueDate.HasValue
            && DueDate.Value.Date < DateTime.Today
            && (Status == 1 || Status == 2);

        public string DirectionText => Direction switch
        {
            1 => "on Me",
            2 => "For Me",
            _ => "؟"
        };

        public string StatusText => Status switch
        {
            1 => "open",
            2 => "Part paid",
            3 => "paid",
            4 => "done",
            _ => "Unknown"
        };

        /// <summary>نسبة السداد — للـ progress bar</summary>
        public decimal PaidPercentage =>
            Amount == 0 ? 0 : Math.Round(TotalPaid / Amount * 100, 1);
    
}
}
