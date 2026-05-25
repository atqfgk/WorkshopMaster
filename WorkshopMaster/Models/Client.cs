using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkshopMaster.WPF.Models
{
    public class Client
    {
        public int ID { get; set; }
        public string ФИО { get; set; }
        public string Телефон { get; set; }
        public string Email { get; set; }
        public string АдресДоставки { get; set; }
    }
}
