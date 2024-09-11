using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Entities.LinkModels;
using Entities.Models;
using Shared.DataTransferObjects;
using Shared.RequestFeatures;

namespace Services.Contracts
{
    public interface IEmployeeService
    {
       Task<(LinkResponse  linkedResponse, MetaData metaData)> GetEmployeesAsync(Guid companyId, LinkParameters linkParameters, bool trackChanges);
       Task<EmployeeDto> GetEmployeeAsync(Guid companyId, Guid id, bool trackChanges);
       Task<EmployeeDto> CreateEmployeeForCompanyAsync(Guid companyId, EmployeeForCreationDto employeeForCreationDto, bool trackChanges);
       Task DeleteEmployeeForCompanyAsync(Guid companyId, Guid id, bool trackChanges);
       Task UpdateEmployeeForCompanyAsync(Guid companyId, Guid id, EmployeeForUpdateDto employeeForUpdate, 
       bool comTrackChanges, bool empTrackChanges);

       Task<(EmployeeForUpdateDto employeeToPatch, Employee employeeEntity)> GetEmployeeForPatch(
        Guid companyId, Guid id, bool compTrackChanges, bool empTrackChanges);
        Task SaveChangesForPatch(EmployeeForUpdateDto employeeToPatch, Employee
        employeeEntity);
    }
}