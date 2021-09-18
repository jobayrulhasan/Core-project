﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SignalRHub.Models;
using static SignalRHub.Services.StaticInfos;

namespace SignalRHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        private BusTicketContext _ctx = null;
        public ScheduleController()
        {
            _ctx = new BusTicketContext();
        }

        [HttpGet("[action]")]
        public IEnumerable<object> getall()
        {
            var list = (from s in _ctx.Schedule
                        join b in _ctx.Bus on s.BusId equals b.BusId
                        join r in _ctx.Route on s.RouteId equals r.RouteId
                        select new
                        {
                            s.ScheduleId,
                            s.DepartureTime,
                            s.ArrivalTime,
                            s.BusId,
                            s.RouteId,
                            b.BusName,
                            b.BusType,
                            r.RouteName,
                            s.ActualDepartureTime,
                            s.ActualArrivalTime,
                            s.BusStatus,
                            s.ScheduleCancel
                        });
            return list;
        }

        // POST: api/schedule/save
        [HttpPost("[action]")]
        public async Task<object> save()
        {
            object result = null; string message = string.Empty; bool resstate = false;
            try
            {
                //Save
                var model = new Schedule()
                {
                    ScheduleId = Convert.ToInt32(Request.Form["scheduleId"]),
                    DepartureTime = Convert.ToDateTime(Request.Form["departureTime"].ToString()),
                    ArrivalTime = Convert.ToDateTime(Request.Form["arrivalTime"].ToString()),
                    BusId = Convert.ToInt32(Request.Form["busId"].ToString()),
                    RouteId = Convert.ToInt32(Request.Form["routeId"].ToString()),
                    ScheduleCancel = Request.Form["scheduleCancel"].ToString() == null ? false : Convert.ToBoolean(Request.Form["scheduleCancel"].ToString()),
                    BusStatus = Request.Form["busStatus"].ToString()
                };

                DateTime dt = DateTime.Now;
                model.ActualDepartureTime = !DateTime.TryParse(Request.Form["actualDepartureTime"].ToString(), out dt) ? (DateTime?)null : Convert.ToDateTime(Request.Form["actualDepartureTime"].ToString());
                model.ActualArrivalTime = !DateTime.TryParse(Request.Form["actualDepartureTime"].ToString(), out dt) ? (DateTime?)null : Convert.ToDateTime(Request.Form["actualArrivalTime"].ToString());

                var obj = _ctx.Schedule.Where(s => s.ScheduleId == model.ScheduleId).FirstOrDefault();
                if (obj == null)
                {
                    // validation for add schedule a bus at same time and at same route
                    var scheduleExist = _ctx.Schedule.Where(s => s.BusId == model.BusId && s.RouteId == model.RouteId && s.DepartureTime <= model.DepartureTime && s.ArrivalTime >= model.DepartureTime).FirstOrDefault();
                    if(model.DepartureTime >= model.ArrivalTime)
                    {
                        message = "Departure time should not be greater or equal to Arrive time.";
                        resstate = false;
                    }
                    else if (scheduleExist != null)
                    {
                        message = "This bus already deployed at the same time or the same route.";
                        resstate = false;
                    }
                    else
                    {
                        await _ctx.Schedule.AddAsync(model);
                        await _ctx.SaveChangesAsync();

                        var oBus = _ctx.Bus.Where(b => b.BusId == model.BusId).FirstOrDefault();
                        var seats = oBus.NoOfSeat;
                        var listScheduleDetail = new List<ScheduleDetail>();
                        string[] letters = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" };
                        foreach (var l in letters)
                        {
                            for (var s = 1; s <= 4; s++)
                            {
                                var oScheduleDetail1 = new ScheduleDetail();
                                oScheduleDetail1.BusId = model.BusId;
                                oScheduleDetail1.ScheduleId = model.ScheduleId;
                                oScheduleDetail1.ScheduleStatus = ScheduleStatus.empty.ToString();
                                oScheduleDetail1.SeatNo = l + s;

                                listScheduleDetail.Add(oScheduleDetail1);
                            }
                        }
                        await _ctx.ScheduleDetail.AddRangeAsync(listScheduleDetail);
                        await _ctx.SaveChangesAsync();

                        message = "Added successfully.";
                        resstate = true;
                    }
                }
                else
                {
                    var listScheduleDetailRemove = _ctx.ScheduleDetail.Where(sd => sd.ScheduleId == model.ScheduleId).ToList();
                    _ctx.RemoveRange(listScheduleDetailRemove);
                    _ctx.SaveChanges();

                    obj.DepartureTime = model.DepartureTime;
                    obj.ArrivalTime = model.ArrivalTime;
                    obj.BusId = model.BusId;
                    obj.RouteId = model.RouteId;
                    obj.ActualDepartureTime = model.ActualDepartureTime;
                    obj.ActualArrivalTime = model.ActualDepartureTime;
                    obj.ScheduleCancel = model.ScheduleCancel;
                    obj.BusStatus = model.BusStatus;
                    var oBus = _ctx.Bus.Where(b => b.BusId == model.BusId).FirstOrDefault();
                    var seats = oBus.NoOfSeat;
                    var listScheduleDetail = new List<ScheduleDetail>();
                    string[] letters = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" };
                    foreach (var l in letters)
                    {
                        for (var s = 1; s <= 4; s++)
                        {
                            var oScheduleDetail1 = new ScheduleDetail();
                            oScheduleDetail1.BusId = model.BusId;
                            oScheduleDetail1.ScheduleId = model.ScheduleId;
                            oScheduleDetail1.ScheduleStatus = ScheduleStatus.empty.ToString();
                            oScheduleDetail1.SeatNo = l + s;

                            listScheduleDetail.Add(oScheduleDetail1);
                        }
                    }
                    await _ctx.ScheduleDetail.AddRangeAsync(listScheduleDetail);

                    await _ctx.SaveChangesAsync();
                    message = "Update successfully.";
                    resstate = true;
                }
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

        // DELETE api/schedule/deletebyid/1
        [HttpDelete("[action]/{id}")]
        public async Task<object> deletebyid(int id)
        {
            object result = null; string message = string.Empty; bool resstate = false;
            try
            {
                var list = _ctx.ScheduleDetail.Where(s => s.ScheduleId == id).FirstOrDefault();
                _ctx.ScheduleDetail.RemoveRange(list);

                var obj = _ctx.Schedule.Where(s => s.ScheduleId == id).FirstOrDefault();
                _ctx.Schedule.Remove(obj);
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
    }
}