using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetworkTablesAPI.Controllers
{
    [ApiController]
    public class NTController : ControllerBase
    {
        [HttpGet("{key}")]
        public ActionResult<IEnumerable<string>> GetValue(string key)
        {
            return new string[]{ "Hello" };
        }
    }
}
