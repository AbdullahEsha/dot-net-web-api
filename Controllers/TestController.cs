using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace dot_net_web_api.Controllers;

[ApiController]
[Route("[controller]")]  // Auto-route: "/test"
public class TestController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { Message = "This is a test API!" });
    }

    // Add more practice endpoints here...
    // Example: POST, DELETE, or custom routes like "/test/calculate"
}