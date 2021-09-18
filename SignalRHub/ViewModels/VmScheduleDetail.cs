﻿using SignalRHub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalRHub.ViewModels
{
    public class VmScheduleDetail
    {
        public string Row { get; set; }
        public List<VmScheduleDetailSeat> VmScheduleDetailSeats { get; set; }
        public class VmScheduleDetailSeat
        {
            public int ScheduleDetailsId { get; set; }
            public string SeatNo { get; set; }
            public int? BusId { get; set; }
            public int? ScheduleId { get; set; }
            public string ScheduleStatus { get; set; }
            public string SeatColor { get; set; }
        }
    }
}