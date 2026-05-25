using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkshopMaster.WPF.Models
{
    public class Material
    {
        public int ID { get; set; }
        public string Название { get; set; }
        public string Категория { get; set; }
        public string ЕдиницаИзмерения { get; set; }
        public decimal ЦенаЗакупки { get; set; }
        public decimal ЦенаПродажи { get; set; }
        public decimal ТекущийОстаток { get; set; }
        public decimal МинОстаток { get; set; }
    }
}
