
using HyperInsurance.Models;
using Microsoft.AspNetCore.Mvc;

namespace HyperInsurance.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InsuranceController : ControllerBase
{
    [HttpGet]
    public ActionResult<InsuranceHealthResponse> Get()
    {
        return Ok(new InsuranceHealthResponse(
            "Hyper Insurance",
            "Welcome to Hyper Insurance!",
            "Specialized automotive insurance platform",
            "Running"));
    }
}
