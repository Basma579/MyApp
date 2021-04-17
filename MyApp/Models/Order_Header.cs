using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MyApp.Models
{
    public class Order_Header
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int Order_Header_ID { get; set; }
        public string CustomerID { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal Tax { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalValue { get; set; }
        public string Status { get; set; }
       public ICollection<Order_Details> Order_Details { get; set; }

    }
}
