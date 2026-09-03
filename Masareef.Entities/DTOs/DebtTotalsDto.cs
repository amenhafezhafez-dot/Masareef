using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masareef.MasareefEntities.DTOs
{
    public class DebtTotalsDto
    {
 
        public decimal IOwe { get; set; }   // إجمالي عليّ
        public decimal OwedToMe { get; set; }   // إجمالي ليّ
        public int OpenCount { get; set; }
        public int OverdueCount { get; set; }

        /// <summary>موجب = ليّ أكتر | سالب = عليّ أكتر</summary>
        public decimal NetPosition => OwedToMe - IOwe;

        public string NetText => NetPosition >= 0
            ? $"ليّ صافي {NetPosition:N2}"
            : $"عليّ صافي {Math.Abs(NetPosition):N2}";
    }
}

