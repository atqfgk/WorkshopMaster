using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WorkshopMaster.WPF.Models
{
    public class FactStage
    {
        public int IDПлановогоЭтапа { get; set; }
        public DateTime? ДатаНачалаФакт { get; set; }
        public DateTime? ДатаОкончанияФакт { get; set; }
        public decimal? ФактическиеЧасы { get; set; }
        public string Статус { get; set; }
        public string Комментарий { get; set; }
    }
}