using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Services.Contracts;

namespace CompanyEmployees.Presentation.Controllers
{
    [ApiVersion("2.0",  Deprecated = true)]
    [Route("api/Companies/v2")]
    [ApiController]
    public class CompaniesV2Controller : ControllerBase
    {
        private IServiceManager _serviceManger;

        public CompaniesV2Controller(IServiceManager serviceManger)
        {
            _serviceManger = serviceManger;
        }

        [HttpGet(Name = "CompaniesV2")]
        public async Task<IActionResult> GetCompanies()
        {
            var companies = await _serviceManger.companyService.GetAllCompaniesAsync(trackChanges: false);

            var companiesV2 = companies.Select(x => $"{x.Name} V2");

            return Ok(companiesV2);
        }
    }
}