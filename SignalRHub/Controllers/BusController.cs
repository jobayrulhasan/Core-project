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
    public class BusController : ControllerBase
    {
        private BusTicketContext _ctx = null;
        public BusController()
        {
            _ctx = new BusTicketContext();
        }

        [HttpGet("[action]")]
        public IEnumerable<Bus> getall()
        {
            return _ctx.Bus;
        }

        // POST: api/bus/save
        [HttpPost("[action]")]
        public async Task<object> save()
        {
            object result = null; string message = string.Empty; bool resstate = false;
            try
            {
                //Save
                var model = new Bus()
                {
                    BusId = Convert.ToInt32(Request.Form["BusId"].ToString()),
                    BusName = Request.Form["busName"].ToString(),
                    BusType = Request.Form["busType"].ToString(),
                    NoOfSeat = Convert.ToInt32(Request.Form["noOfSeat"]),
                    LicenseNo = Request.Form["licenseNo"].ToString(),
                    FitnessStatus = Convert.ToBoolean(Request.Form["fitnessStatus"].ToString())
                };

                var obj = _ctx.Bus.Where(s => s.BusId == model.BusId).FirstOrDefault();
                if (obj == null)
                {
                    _ctx.Bus.Add(model);
                    message = "Added successfully.";
                }
                else
                {
                    obj.BusName = model.BusName;
                    obj.BusType = model.BusType;
                    obj.NoOfSeat = model.NoOfSeat;
                    obj.LicenseNo = model.LicenseNo;
                    obj.FitnessStatus = model.FitnessStatus;
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

        // DELETE api/bus/deletebyid/1
        [HttpDelete("[action]/{id}")]
        public async Task<object> deletebyid(int id)
        {
            object result = null; string message = string.Empty; bool resstate = false;
            try
            {
                var obj = _ctx.Bus.Where(s => s.BusId == id).FirstOrDefault();
                _ctx.Bus.Remove(obj);
                await _ctx.SaveChangesAsync();
                message = "Removed.";
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