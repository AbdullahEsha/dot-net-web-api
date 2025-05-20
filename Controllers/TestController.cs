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
    public static readonly object[] DataToReturn = new object[]
    {
        new { Id = 1, Message = "Test Data 1" },
        new { Id = 2, Message = "Test Data 2" },
        new { Id = 3, Message = "Test Data 3" },
        new { Id = 4, Message = "Test Data 4" },
        new { Id = 5, Message = "Test Data 5" },
        new { Id = 6, Message = "Test Data 6" },
        new { Id = 7, Message = "Test Data 7" },
        new { Id = 8, Message = "Test Data 8" },
        new { Id = 9, Message = "Test Data 9" },
        new { Id = 10, Message = "Test Data 10" },
    };

    private readonly ILogger<TestController> _logger;
    public TestController(ILogger<TestController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetTestData")]

    public IEnumerable<object> Get()
    {
        // Return the test data
        return DataToReturn;
    }

    [HttpGet("{id:int}", Name = "GetTestDataById")]
    public IActionResult GetById(int id)
    {
        // Find the test data by ID
        var data = DataToReturn.FirstOrDefault(d => ((dynamic)d).Id == id);
        if (data == null)
        {
            return NotFound();
        }
        return Ok(data);
    }
    
}