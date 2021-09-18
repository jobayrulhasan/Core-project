using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SignalRHub.Models;

namespace SignalRHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RouteController : ControllerBase
    {
        private BusTicketContext _ctx = null;
        public RouteController()
        {
            _ctx = new BusTicketContext();
        }

        [HttpGet("[action]")]
        public IEnumerable<object> getall()
        {
            var list = (from r in _ctx.Route
                        join b in _ctx.Bus on r.BusId equals b.BusId
                        select new
                        {
                            r.RouteId,
                            r.RouteName,
                            r.StartPoint,
                            r.EndPoint,
                            r.BusId,
                            r.UnitPrice,
                            b.BusName,
                            b.BusType
                        });
            //return _ctx.Route;
            return list;
        }

        // POST: api/route/save
        [HttpPost("[action]")]
        public async Task<object> save()
        {
            object result = null; string message = string.Empty; bool resstate = false;
            try
            {
                //Save
                var model = new Route()
                {
                    RouteId = Convert.ToInt32(Request.Form["routeId"]),
                    RouteName = Request.Form["routeName"].ToString(),
                    StartPoint = Request.Form["startPoint"].ToString(),
                    EndPoint = Request.Form["endPoint"].ToString(),
                    BusId = Convert.ToInt32(Request.Form["busId"].ToString()),
                    UnitPrice = Convert.ToInt32(Request.Form["unitPrice"].ToString()),
                };

                var obj = _ctx.Route.Where(s => s.RouteId == model.RouteId).FirstOrDefault();
                if (obj == null)
                {
                    _ctx.Route.Add(model);
                    message = "Added successfully.";
                }
                else
                {
                    obj.RouteName = model.RouteName;
                    obj.StartPoint = model.StartPoint;
                    obj.EndPoint = model.EndPoint;
                    obj.BusId = model.BusId;
                    obj.UnitPrice = model.UnitPrice;
                    message = "Update successfully.";
                }
                await _ctx.SaveChangesAsync();
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

        // DELETE api/route/deletebyid/1
        [HttpDelete("[action]/{id}")]
        public async Task<object> deletebyid(int id)
        {
            object result = null; string message = string.Empty; bool resstate = false;
            try
            {
                var obj = _ctx.Route.Where(s => s.RouteId == id).FirstOrDefault();
                _ctx.Route.Remove(obj);
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