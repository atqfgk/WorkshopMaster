using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WorkshopMaster.WPF.Models
{
    public class PlanStage
    {
        public int ID { get; set; }
        public int IDЗаказа { get; set; }
        public int IDЭтапа { get; set; }
        public int IDСотрудника { get; set; }
        public DateTime ДатаНачалаПлан { get; set; }
        public DateTime ДатаОкончанияПлан { get; set; }
        public decimal ПлановыеЧасы { get; set; }
    }
}