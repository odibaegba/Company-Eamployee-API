using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Formats.Asn1;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts;
using Entities.Exceptions;
using Entities.LinkModels;
using Entities.Models;
using Services.Contracts;
using Shared.DataTransferObjects;
using Shared.RequestFeatures;

namespace Service
{
    internal sealed class EmployeeService : IEmployeeService
    {
        private readonly IRepositoryManager _repository;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;
        private readonly IDataShaper<EmployeeDto> _dataShaper;
        private readonly IEmployeeLinks _employeeLinks;

        public EmployeeService(IRepositoryManager repository, ILoggerManager logger, IMapper mapper, IEmployeeLinks employeeLinks)
        {
            _logger = logger;
            _repository = repository;
            _mapper = mapper;
           // _dataShaper = dataShaper;
            _employeeLinks = employeeLinks;
        }

        public async Task<EmployeeDto> CreateEmployeeForCompanyAsync(Guid companyId, EmployeeForCreationDto employeeForCreationDto, bool trackChanges)
        {
            await CheckIfCompanyExists(companyId, trackChanges);

            var employeeEntity = _mapper.Map<Employee>(employeeForCreationDto);

            _repository.employee.CreateEmployeeForCompany(companyId, employeeEntity);
            await _repository.SaveAsync();

            var employeeToReturn = _mapper.Map<EmployeeDto>(employeeEntity);

            return employeeToReturn;

        }

        public async Task DeleteEmployeeForCompanyAsync(Guid companyId, Guid id, bool trackChanges)
        {
           await CheckIfCompanyExists(companyId, trackChanges);

             var employeeForCompany = await GetEmployeeForCompanyAndCheckIfItExists(companyId, id, trackChanges);

            _repository.employee.DeleteEmployee(employeeForCompany);
            await _repository.SaveAsync();

        }

        public async Task<EmployeeDto> GetEmployeeAsync(Guid companyId, Guid id, bool trackChanges)
        {
             await CheckIfCompanyExists(companyId, trackChanges);

             var employee = await GetEmployeeForCompanyAndCheckIfItExists(companyId, id, trackChanges);
             var employeeDto = _mapper.Map<EmployeeDto>(employee);
             
             return employeeDto;
        }

        public async Task<(EmployeeForUpdateDto employeeToPatch, Employee employeeEntity)> GetEmployeeForPatch(
            Guid companyId, Guid id, bool compTrackChanges, bool empTrackChanges)
        {
            await CheckIfCompanyExists(companyId, compTrackChanges);
           
            var employeeEntity = await GetEmployeeForCompanyAndCheckIfItExists(companyId, id, empTrackChanges);

            var employeeToPatch = _mapper.Map<EmployeeForUpdateDto>(employeeEntity);

            return (employeeToPatch, employeeEntity);

        }

        public async Task<(LinkResponse linkedResponse, MetaData metaData)> GetEmployeesAsync(Guid companyId, LinkParameters linkParameters, bool trackChanges)
        {
             if(!linkParameters.EmployeeParameters.ValidAgeRange)
             throw new MaxAgeRangeBadRequestException();

            await CheckIfCompanyExists(companyId, trackChanges);

             var employeesWithMetaData = await _repository.employee.GetEmployeesAsync(companyId, linkParameters.EmployeeParameters, trackChanges);

             var employeesDto = _mapper.Map<IEnumerable<EmployeeDto>>(employeesWithMetaData);

             var links = _employeeLinks.TryGenerateLinks(employeesDto, linkParameters.EmployeeParameters.Fields, companyId, linkParameters.Context);

             //var shapedData = _dataShaper.ShapeData(employeesDto, employeeParameters.Fields);

             return (linkResponse: links, metaData : employeesWithMetaData.MetaData);

        }

        public async Task SaveChangesForPatch(EmployeeForUpdateDto employeeToPatch, Employee employeeEntity)
        {
            _mapper.Map(employeeToPatch, employeeEntity);
            await _repository.SaveAsync();
        }

        public async Task UpdateEmployeeForCompanyAsync(Guid companyId, Guid id, EmployeeForUpdateDto employeeForUpdate, bool comTrackChanges, bool empTrackChanges)
        {
           await CheckIfCompanyExists (companyId, comTrackChanges);
            var employeeEntity = await GetEmployeeForCompanyAndCheckIfItExists(companyId, id, empTrackChanges);

            _mapper.Map(employeeForUpdate, employeeEntity);
            await _repository.SaveAsync();
        }

        private async Task CheckIfCompanyExists(Guid companyId, bool trackChanges)
        {
            var company = await _repository.Company.GetCompanyAsync(companyId, trackChanges);
            if (company is null)
            throw new CompanyNotFoundException(companyId);
        }

        private async Task<Employee> GetEmployeeForCompanyAndCheckIfItExists(Guid companyId, Guid id, bool trackChanges)
        {
            var employeeDb = await _repository.employee.GetEmployeeAsync(companyId, id, trackChanges);
            if (employeeDb is null)
            throw new EmployeeNotFoundException(id);
            return employeeDb;
        }
    }
}