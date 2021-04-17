using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MyApp.Models
{
    public class Order_Details
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Order_Details_ID { get; set; }
        public string ItemName { get; set; }
        public int ItemID { get; set; }
        public decimal ItemPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public string Uom { get; set; }
        public decimal Discount { get; set; }
        public Order_Header Order_Header { get; set; }
    }
}
