using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Artikolly
{
    public class CurrentUser
    {
        public static string UserID { get; set; }
        public static string Surname { get; set; }
        public static string Name { get; set; }
        public static string Patronymic { get; set; }
        public static string Role { get; set; }
        public static string RoleName { get; set; }

        // Метод для проверки, является ли пользователь администратором
        public static bool IsAdministrator()
        {
            return RoleName?.ToLower() == "администратор" || UserID == "0";
        }
    }
}
