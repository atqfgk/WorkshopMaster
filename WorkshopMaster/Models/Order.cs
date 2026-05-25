using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkshopMaster.WPF.Models
{
    public class Order
    {
        public int ID { get; set; }
        public string НомерЗаказа { get; set; }
        public int IDКлиента { get; set; }
        public DateTime ДатаПриема { get; set; }
        public DateTime ДатаИсполненияПлан { get; set; }
        public DateTime? ДатаИсполненияФакт { get; set; }
        public string ОписаниеИзделия { get; set; }
        public string Статус { get; set; }
        public decimal? ПредварительнаяСтоимость { get; set; }
        public decimal? ФактическаяСебестоимость { get; set; }
    }
}