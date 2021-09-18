using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SignalRHub.Models;
using SignalRHub.Services;
using SignalRHub.ViewModels;
using static SignalRHub.Services.StaticInfos;

namespace SignalRHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private IHubContext<NotifyHub, ITypedHubClient> _hubContext;
        private BusTicketContext _ctx = null;

        public BookingController(IHubContext<NotifyHub, ITypedHubClient> hubContext)
        {
            _hubContext = hubContext;
            _ctx = new BusTicketContext();
        }

        // GET api/booking/getall/1/1
        [HttpGet("[action]/{scheduleId}/{busId}")]
        public IEnumerable<object> getall(int scheduleId, int busId)
        { 
            var list = (from bk in _ctx.Booking
                        join bs in _ctx.Bus on bk.BusId equals bs.BusId
                        join s in _ctx.Schedule on bk.ScheduleId equals s.ScheduleId
                        join r in _ctx.Route on s.RouteId equals r.RouteId
                        where bk.ScheduleId == scheduleId && bk.BusId == busId
                        select new
                        {
                            bk.BookingId,
                            bk.CustomerName,
                            bk.CustomerMobile,
                            bk.BookedDate,
                            bk.BookedStatus,
                            bk.Price,
                            bk.CancelDate,
                            bk.ReturnAmount,
                            bk.ReturnStatus,
                            bk.BusId,
                            bk.ScheduleId,
                            bs.BusName,
                            bs.BusType,
                            s.DepartureTime,
                            s.ArrivalTime,
                            s.RouteId,
                            r.RouteName,
                            Seat = (from bd in _ctx.BookingDetail where bd.BookingId == bk.BookingId select bd.BookingId).Count()
                        });
            return list;
        }

        // GET api/booking/getscheduledetail/1/1
        [HttpGet("[action]/{scheduleId}/{busId}")]
        public IEnumerable<object> getscheduledetail(int scheduleId, int busId)
        {
            var listScheduleDetail = _ctx.ScheduleDetail.Where(sd => sd.ScheduleId == scheduleId && sd.BusId == busId).ToList();
            char[] alphas = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J' };
            var listVmScheduleDetail = new List<VmScheduleDetail>();
            foreach (var alpha in alphas)
            {
                var oVmScheduleDetail = new VmScheduleDetail();
                oVmScheduleDetail.Row = alpha.ToString();
                var listVmScheduleDetailSeat = new List<VmScheduleDetail.VmScheduleDetailSeat>();
                for (var i = 1; i <= 4; i++)
                {
                    string seatNo = alpha.ToString() + i;
                    var oSeatColor = _ctx.ScheduleDetail.Where(sd => sd.SeatNo == seatNo).FirstOrDefault();
                    if (oSeatColor != null)
                    {
                        var oVmScheduleDetailSeat = new VmScheduleDetail.VmScheduleDetailSeat();
                        oVmScheduleDetailSeat.ScheduleDetailsId = oSeatColor.ScheduleDetailsId;
                        oVmScheduleDetailSeat.BusId = oSeatColor.BusId;
                        oVmScheduleDetailSeat.ScheduleId = oSeatColor.ScheduleId;
                        oVmScheduleDetailSeat.ScheduleStatus = oSeatColor.ScheduleStatus;
                        oVmScheduleDetailSeat.SeatNo = oSeatColor.SeatNo;
                        oVmScheduleDetailSeat.SeatColor = oSeatColor.ScheduleStatus == StaticInfos.ScheduleStatus.reserved.ToString() ? "red" : "cornflowerblue";
                        listVmScheduleDetailSeat.Add(oVmScheduleDetailSeat);
                    }

                    if (i == 2)
                    {
                        var oVmScheduleDetailSeat1 = new VmScheduleDetail.VmScheduleDetailSeat();
                        oVmScheduleDetailSeat1.SeatColor = "";
                        listVmScheduleDetailSeat.Add(oVmScheduleDetailSeat1);
                    }
                }
                oVmScheduleDetail.VmScheduleDetailSeats = listVmScheduleDetailSeat;
                listVmScheduleDetail.Add(oVmScheduleDetail);
            }
            return listVmScheduleDetail;
        }

        // POST: api/booking/save
        [HttpPost("[action]")]
        public async Task<object> save()
        {
            object result = null; string message = string.Empty; bool resstate = false;
            try
            {
                //Save
                var model = new Booking()
                {   
                    BookingId = Convert.ToInt32(Request.Form["bookingId"].ToString()),
                    CustomerName = Request.Form["customerName"].ToString(),
                    CustomerMobile = Request.Form["customerMobile"].ToString(),
                    BookedDate = Request.Form["bookedDate"].ToString() == "" ? (DateTime?)null : Convert.ToDateTime(Request.Form["bookedDate"].ToString()),
                    BookedStatus = Request.Form["bookedStatus"].ToString(),
                    Price = Request.Form["price"].ToString() == "" ? (int?)null : Convert.ToInt32(Request.Form["price"].ToString()),
                    CancelDate = Request.Form["cancelDate"].ToString() == "" ? (DateTime?)null : Convert.ToDateTime(Request.Form["cancelDate"].ToString()),
                    ReturnAmount = Request.Form["returnAmount"].ToString() == "" ? (int?)null : Convert.ToInt32(Request.Form["returnAmount"].ToString()),
                    ReturnStatus = Request.Form["returnStatus"].ToString() == "" ? (bool?)null : Convert.ToBoolean(Request.Form["returnStatus"].ToString()),
                    BusId = Request.Form["busId"].ToString() == "" ? (int?)null : Convert.ToInt32(Request.Form["busId"].ToString()),
                    ScheduleId = Request.Form["scheduleId"].ToString() == "" ? (int?)null : Convert.ToInt32(Request.Form["scheduleId"])
                };

                

                var obj = _ctx.Booking.Where(s => s.BookingId == model.BookingId).FirstOrDefault();
                if (obj == null)
                {
                    _ctx.Booking.Add(model);
                    await _ctx.SaveChangesAsync();

                    var listBookingDetail = new List<BookingDetail>();
                    var listSchedule = _ctx.ScheduleDetail.Where(w => w.ScheduleId == model.ScheduleId).ToList();
                    var ids = Request.Form["ids"].ToString();
                    string[] arrID = ids.Contains(",") ? ids.Split(',') : new string[0];
                    foreach (var id in arrID)
                    {
                        int nid = Convert.ToInt32(id);
                        var ScheduleDetails = listSchedule.Where(w => w.ScheduleDetailsId == nid).FirstOrDefault();
                        if (ScheduleDetails != null)
                        {
                            var oBookingDetail = new BookingDetail();
                            //oBookingDetail.BookingDetailsId PK
                            oBookingDetail.SeatNo = ScheduleDetails.SeatNo;
                            oBookingDetail.CustomerName = model.CustomerName;
                            oBookingDetail.CustomerMobile = model.CustomerMobile;
                            oBookingDetail.BookingId = model.BookingId;
                            oBookingDetail.BusId = ScheduleDetails.BusId;
                            oBookingDetail.ScheduleId = ScheduleDetails.ScheduleId;
                            oBookingDetail.ScheduleDetailsId = ScheduleDetails.ScheduleDetailsId;
                            listBookingDetail.Add(oBookingDetail);

                            ScheduleDetails.ScheduleStatus = ScheduleStatus.reserved.ToString();
                            await _ctx.SaveChangesAsync();
                        }
                    }
                    _ctx.BookingDetail.AddRange(listBookingDetail);
                    await _ctx.SaveChangesAsync();

                    await _ctx.Booking.AddAsync(model);

                    message = "Added successfully.";
                    resstate = true;

                    await _hubContext.Clients.All.BroadcastMessage("success", "Seat Booked!");
                }
                //else
                //{
                //    var removeList = _ctx.BookingDetail.Where(w => w.BookingId == model.BookingId).ToList();
                //    _ctx.RemoveRange(removeList);
                //    _ctx.SaveChanges();

                //    var listBookingDetail = new List<BookingDetail>();
                //    var listSchedule = _ctx.ScheduleDetail.Where(w => w.ScheduleId == model.ScheduleId).ToList();
                //    var ids = Request.Form["ids"].ToString();
                //    string[] arrID = ids.Contains(",") ? ids.Split(',') : new string[0];
                //    foreach (var id in arrID)
                //    {
                //        int nid = Convert.ToInt32(id);
                //        var oSchedule = listSchedule.Where(w => w.ScheduleDetailsId == nid).FirstOrDefault();
                //        if (oSchedule != null)
                //        {
                //            var oBookingDetail = new BookingDetail();
                //            //oBookingDetail.BookingDetailsId PK
                //            oBookingDetail.SeatNo = oSchedule.SeatNo;
                //            oBookingDetail.CustomerName = model.CustomerName;
                //            oBookingDetail.CustomerMobile = model.CustomerMobile;
                //            oBookingDetail.BookingId = model.BookingId;
                //            oBookingDetail.BusId = oSchedule.BusId;
                //            oBookingDetail.ScheduleId = oSchedule.ScheduleId;
                //            oBookingDetail.ScheduleDetailsId = oSchedule.ScheduleDetailsId;

                //            listBookingDetail.Add(oBookingDetail);
                //        }
                //    }
                //    _ctx.BookingDetail.AddRange(listBookingDetail);
                //    await _ctx.SaveChangesAsync();

                //    obj.BookingId = model.BookingId;
                //    obj.CustomerName = model.CustomerName;
                //    obj.CustomerMobile = model.CustomerMobile;
                //    obj.BookedDate = model.BookedDate;
                //    obj.BookedStatus = model.BookedStatus;
                //    obj.Price = model.Price;
                //    obj.CancelDate = model.CancelDate;
                //    obj.ReturnAmount = model.ReturnAmount;
                //    obj.ReturnStatus = model.ReturnStatus;
                //    obj.BusId = model.BusId;
                //    obj.ScheduleId = model.ScheduleId;
                    
                //    await _ctx.SaveChangesAsync();
                //    resstate = true;
                //    message = "Update successfully.";

                //    await _hubContext.Clients.All.BroadcastMessage("success", "Seat Booked!");
                //}
            }
            catch (Exception ex)
            {
                ex.ToString();
            }

            result = new
            {
                message,
                resstate
            };

            return result;
        }

        // DELETE api/booking/deletebyid/1
        [HttpDelete("[action]/{id}")]
        public async Task<object> deletebyid(int id)
        {
            object result = null; string message = string.Empty; bool resstate = false;
            try
            {
                var list = _ctx.BookingDetail.Where(s => s.BookingId == id).ToList();
                foreach (var o in list)
                {
                    var ScheduleDetails = _ctx.ScheduleDetail.Where(w => w.ScheduleDetailsId == o.ScheduleDetailsId).FirstOrDefault();
                    if (ScheduleDetails != null)
                    {
                        ScheduleDetails.ScheduleStatus = ScheduleStatus.empty.ToString();
                        await _ctx.SaveChangesAsync();
                    }
                }
                _ctx.BookingDetail.RemoveRange(list);
                var obj = _ctx.Booking.Where(s => s.BookingId == id).FirstOrDefault();
                _ctx.Booking.Remove(obj);
                await _ctx.SaveChangesAsync();
                message = "Remove successfully.";
                resstate = true;
            }
            catch (Exception ex)
            {
                ex.ToString();
            }

            result = new
            {
                message,
                resstate
            };

            return result;
        }


        // GET api/booking/getschedule/{Dhaka}/{Chittagong}
        [HttpGet("[action]/{startpoint}/{endpoint}")]
        public async Task<object> getschedule(string startpoint, string endpoint)
        {
            object result = null; string message = string.Empty; bool resstate = false; List<VmSchedule> listVmSchedule = null;
            try
            {
                var service = new MsSqlService();
                var dt = await service.Get(StaticInfos.connecitonString, "EXEC SP_GetSchedule @startPoint = '" + startpoint + "', @endPoint = '" + endpoint + "'", true);
                listVmSchedule = Conversion.ConvertDataTableToObject<VmSchedule>(dt);
                message = "Data retrieve successfully.";
                resstate = true;
            }
            catch (Exception ex)
            {
                ex.ToString();
            }

            result = new
            {
                message,
                resstate,
                listVmSchedule
            };

            return result;
        }

        // GET api/booking/getbookingbyscheduleid/1/1
        /*[HttpGet("[action]/{scheduleId}/{busId}")]
        public async Task<object> getbookingbyscheduleid (int scheduleId, int busId)
        {
            object result = null; string message = string.Empty; bool resstate = false; List<VmBooking> listVmBooking = null; List<ScheduleDetail> listScheduleDetail = null;
            try
            {
                var service = new MsSqlService();
                var dt = await service.Get(StaticInfos.connecitonString, "EXEC [SP_GetBookingByScheduleId] @scheduleID = " + scheduleId + ", @busId = " + busId, true);
                listVmBooking = Conversion.ConvertDataTableToObject<VmBooking>(dt);

                listScheduleDetail = _ctx.ScheduleDetail.Where(sd => sd.ScheduleId == scheduleId && sd.BusId == busId).ToList();

                message = "Data retrieve successfully.";
                resstate = true;
            }
            catch (Exception ex)
            {
                ex.ToString();
            }

            result = new
            {
                message,
                resstate,
                listVmBooking,
                listScheduleDetail
            };

            return result;
        }*/

    }
}