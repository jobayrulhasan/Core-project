using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalRHub.Services
{
    public static class StaticInfos
    {
        public const string connecitonString = @"Server=DESKTOP-RO854JB;Database=BusTicket;User Id=sa; Password=123;";

        public enum ScheduleStatus { reserved, empty}

        public static string[] Role = { "Sale Executive", "Manager"};
    }
}
