using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkshopMaster.WPF.Helpers
{
    public static class RoleHelper
    {
        public static bool CanCreateOrder(string role) => role == "Руководитель" || role == "Менеджер";
        public static bool CanEditMaterials(string role) => role == "Руководитель" || role == "Кладовщик";
        public static bool CanViewReports(string role) => role == "Руководитель";
        public static bool CanEditStagesAndMaterialsInOrder(string role) => role == "Руководитель" || role == "Менеджер" || role == "Дизайнер";
        public static bool CanEditOrderStatus(string role) => role == "Руководитель" || role == "Менеджер";
        public static bool CanReceiveMaterials(string role) => role == "Руководитель" || role == "Кладовщик";
        public static bool CanManageEmployees(string role) => role == "Руководитель";
    }
}