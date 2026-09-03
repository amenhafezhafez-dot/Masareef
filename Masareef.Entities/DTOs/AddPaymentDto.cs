using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masareef.MasareefEntities.DTOs
{
    public class AddPaymentDto
    {
        public int DebtID { get; set; }
        public decimal PaidAmount { get; set; }
        public DateTime PaymentDate { get; set; }
        public byte PaymentMethod { get; set; } = 1;   // 1=كاش 2=كارت 3=تحويل
        public string? Note { get; set; }

    }
}
