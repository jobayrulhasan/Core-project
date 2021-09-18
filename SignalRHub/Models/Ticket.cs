using System;
using System.Collections.Generic;

namespace SignalRHub.Models
{
    public partial class Ticket
    {
        public int TicketId { get; set; }
        public int? RouteId { get; set; }
        public int? BusId { get; set; }
        public int? UnitPrice { get; set; }
    }
}
