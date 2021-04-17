using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MyApp.Models
{
    public class UOM
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int UomID { get; set; }
        public string Uom { get; set; }
        public string Description { get; set; }

    }
}