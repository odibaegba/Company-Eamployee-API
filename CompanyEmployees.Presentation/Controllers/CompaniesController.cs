using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CompanyEmployees.Presentation.ActionFilters;
using CompanyEmployees.Presentation.ModelBinder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using Services.Contracts;
using Shared.DataTransferObjects;

namespace CompanyEmployees.Presentation.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/Companies")]
    [ApiController]
    //[ResponseCache(CacheProfileName = "120SecondsDuration")]
    [OutputCache(PolicyName = "120SecondsDuration")]
    public class CompaniesController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;

        public CompaniesController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }
       /// <summary>
       /// Get the list of all companies.
       /// </summary>
       /// <returns></returns>
       [HttpGet(Name = "GetCompanies")]
       [EnableRateLimiting("SpecificPolicy")]
       //[DisableRateLimiting]
       [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetCompanies()
        {
           //throw new Exception("Exception");
            var companies = await _serviceManager.companyService.GetAllCompaniesAsync(trackChanges: false);
            return Ok(companies);
          
        }

        [HttpGet("CompanyById/{id}")]
        //[ResponseCache(Duration = 60)]
        [OutputCache(Duration = 60)]
        public async Task<IActionResult> GetCompany([FromRoute]Guid id)
        {
            var company = await _serviceManager.companyService.GetCompanyAsync(id, trackChanges: false);

            return Ok(company);

        }

        [HttpPost(Name = "CreateCompany")]
         [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> CreateCompany([FromBody] CompanyForCreationDto company)
        {

            var createdCompany = await  _serviceManager.companyService.CreateCompanyAsync(company);

            return CreatedAtRoute("CompanyById", new {id = createdCompany.Id}, createdCompany);
        }

        [HttpGet("collection/({ids})", Name = "CompanyCollection")]
        public async Task<IActionResult> GetCompanyCollection ([ModelBinder(BinderType =
        typeof(ArrayModelBinder))] IEnumerable<Guid> ids, bool trackChanges)
        {
            var companies = await _serviceManager.companyService.GetByIdsAsync(ids, trackChanges: false);

            return Ok(companies);
        }

        [HttpPost("Collection")]
        public async Task<IActionResult> CreateCompanyCollection([FromBody] IEnumerable<CompanyForCreationDto> companyCollection)
        {
            var result = await _serviceManager.companyService.CreateCompanyCollectionAsync(companyCollection);
            return CreatedAtRoute("CompanyCollection", new { result.ids },
            result.companies);
        }

        [HttpDelete]

        public async Task<IActionResult> DeleteCompany (Guid companyId)
        {
            await _serviceManager.companyService.DeleteCompanyAsync(companyId, trackChanges: false);
            return NoContent();
        }

        [HttpPut]
         [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> UpdateCompany (Guid companyId, [FromBody] CompanyForUpdateDto companyForUpdateDto)
        {

           await  _serviceManager.companyService.UpdateCompanyAsync(companyId, companyForUpdateDto, trackChanges: true);
            return NoContent();
        }

        [HttpOptions]
        public IActionResult GetCompaniesOptions()
        {
            Response.Headers.Add("Allow", "GET, OPTIONS, POST, PUT, DELETE");

            return Ok();
        }

    }
}