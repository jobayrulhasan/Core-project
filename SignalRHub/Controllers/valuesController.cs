using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SignalRHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class valuesController : ControllerBase
    {
        [HttpGet("[action]")]
        public object index()
        {
            object result = null;
            result = new { message = "Your system is running" };
            return result;
        }
    }
}