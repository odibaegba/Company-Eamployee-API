using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CompanyEmployees.Presentation.ActionFilters;
using Entities.LinkModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Services.Contracts;
using Shared.DataTransferObjects;
using Shared.RequestFeatures;

namespace CompanyEmployees.Presentation.Controllers
{
    [Route("api/companies/{companyId}/employees")]
    public class EmployeesController : ControllerBase
    {
        private readonly  IServiceManager _serviceManager;
       
        public EmployeesController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }
       
       [HttpGet]
       [HttpHead]
       [ServiceFilter(typeof(ValidateMediaTypeAttribute))]
        public async Task<IActionResult> GetEmployeesForCompany(Guid companyId, [FromQuery] EmployeeParameters employeeParameters)
        {
            var linkParam = new LinkParameters(employeeParameters, HttpContext);
            var result = await _serviceManager.employeeService.GetEmployeesAsync(companyId, linkParam, trackChanges: false);

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(result.metaData));

            return result.linkedResponse.HasLinks? Ok(result.linkedResponse.LinkedEntities) : Ok(result.linkedResponse.ShapedEntities);
        }

        [HttpGet("{id:guid}", Name = "GetEmployeeForCompany")]
        public async Task<IActionResult> GetEmployeeForCompany(Guid companyId, Guid id)
        {
            var employee = await _serviceManager.employeeService.GetEmployeeAsync(companyId, id, trackChanges: false);

            return Ok(employee);
        }

        [HttpPost]    
        public async Task<IActionResult> CreateEmployeeForCompany (Guid companyId, [FromBody] EmployeeForCreationDto employee)
        {
            if(employee == null)
            return BadRequest("EmployeeForCreationDto object is null");

            if(!ModelState.IsValid)
             return UnprocessableEntity(ModelState);
            
            var employeeToReturn = await _serviceManager.employeeService.CreateEmployeeForCompanyAsync(companyId, employee, trackChanges: false);

            return CreatedAtRoute("GetEmployeeForCompany", new { companyId, id =  employeeToReturn.Id },
                employeeToReturn);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteEmployeeForCompany(Guid companyId, Guid id)
        {
            await _serviceManager.employeeService.DeleteEmployeeForCompanyAsync(companyId, id, trackChanges: false);

            return NoContent();
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateEmployeeForCompany(Guid companyId, Guid id, [FromBody] EmployeeForUpdateDto employee)
        {
            if(employee is null)
            return BadRequest("EmployeeForUpdateDto object is null");

            if(!ModelState.IsValid)
            return UnprocessableEntity(ModelState);

         await  _serviceManager.employeeService
            .UpdateEmployeeForCompanyAsync(companyId, id, employee, comTrackChanges: false, empTrackChanges: true);

            return NoContent();

        }

        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> PartiallyUpdateEmployeeForCompany(Guid companyId, Guid id,
        [FromBody] JsonPatchDocument<EmployeeForUpdateDto> patchDoc)
        {
            if (patchDoc is null)
            return BadRequest("patchDoc object sent from client is null.");
            var result = await _serviceManager.employeeService.GetEmployeeForPatch(companyId, id,
            compTrackChanges: false,
            empTrackChanges: true);
            patchDoc.ApplyTo(result.employeeToPatch);
            await _serviceManager.employeeService.SaveChangesForPatch(result.employeeToPatch,
            result.employeeEntity);
            return NoContent();
        }
    }
}