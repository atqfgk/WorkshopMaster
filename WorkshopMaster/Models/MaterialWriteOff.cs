using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkshopMaster.WPF.Models
{
    public class MaterialWriteOff
    {
        public int ID { get; set; }
        public int IDЗаказа { get; set; }
        public int? IDПлановогоЭтапа { get; set; }
        public int IDМатериала { get; set; }
        public DateTime ДатаСписания { get; set; }
        public decimal Количество { get; set; }
        public decimal ЦенаНаМоментСписания { get; set; }
    }
}