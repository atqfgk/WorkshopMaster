using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkshopMaster.WPF.Models
{
    public class Employee
    {
        public int ID { get; set; }
        public string ФИО { get; set; }
        public string Должность { get; set; }
        public string Специализация { get; set; }
        public decimal СтавкаЧас { get; set; }
        public string Телефон { get; set; }
    }
}