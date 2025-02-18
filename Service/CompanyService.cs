using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts;
using Entities.Exceptions;
using Entities.Models;
using Services.Contracts;
using Shared.DataTransferObjects;

namespace Service
{
    internal sealed class CompanyService : ICompanyService
    {
        private readonly IRepositoryManager _repository;
        private readonly ILoggerManager _logger; 
        private readonly IMapper _mapper;

        public CompanyService(IRepositoryManager repository, ILoggerManager logger, IMapper mapper)
        {
            _logger = logger;
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<CompanyDto> CreateCompanyAsync (CompanyForCreationDto company)
        {
            var companyEntity = _mapper.Map<Company>(company);

            _repository.Company.CreateCompany(companyEntity);
            await _repository.SaveAsync();

            var companyToReturn = _mapper.Map<CompanyDto>(companyEntity);
            return companyToReturn;
        }

        public async Task<(IEnumerable<CompanyDto> companies, string ids)> CreateCompanyCollectionAsync(IEnumerable<CompanyForCreationDto> companyCollection)
        {
            if(companyCollection is null)
            throw new CompanyCollectionBadRequest();

            var companyEntities = _mapper.Map<IEnumerable<Company>>(companyCollection);

            foreach(var companyEntity in companyEntities)
            {
                _repository.Company.CreateCompany(companyEntity); 
            }

            await _repository.SaveAsync();

            var companiesToReturn = _mapper.Map<IEnumerable<CompanyDto>>(companyEntities);
            var ids = string.Join(",",  companiesToReturn.Select(x => x.Id));

            return(companies: companiesToReturn, ids: ids);
        }

        public async Task DeleteCompanyAsync(Guid companyId, bool trackChanges)
        {
            var company = await GetCompanyAndCheckIfItExists(companyId, trackChanges);
            
            _repository.Company.DeleteCompany(company);
            await _repository.SaveAsync();

        }

        public async Task<IEnumerable<CompanyDto>> GetAllCompaniesAsync(bool trackChanges)
        {

            var companies = await _repository.Company.GetAllCompaniesAsync(trackChanges);

            var companiesDto = _mapper.Map<IEnumerable<CompanyDto>>(companies);

            return companiesDto;
          
        }

        public async Task<IEnumerable<CompanyDto>> GetByIdsAsync(IEnumerable<Guid> ids, bool trackChanges)
        {
            if (ids is null)
            throw new IdParametersBadRequestException();

            var companyEntities = await _repository.Company.GetByIdsAsync(ids, trackChanges); 
            if(ids.Count() != companyEntities.Count())
            throw new CollectionByIdsBadRequestException();

            var compantToReturn = _mapper.Map<IEnumerable<CompanyDto>>(companyEntities);
            
            return compantToReturn;

        }

        public async Task<CompanyDto> GetCompanyAsync(Guid companyId, bool trackChanges)
        {
           var company = await GetCompanyAndCheckIfItExists(companyId, trackChanges);
            var CompanyDto = _mapper.Map<CompanyDto>(company);

            return CompanyDto;
        }

        public async Task UpdateCompanyAsync(Guid companyId, CompanyForUpdateDto company, bool trackChanges)
        {
           var companyEntity = await GetCompanyAndCheckIfItExists(companyId, trackChanges);

           _mapper.Map(company, companyEntity);
           await _repository.SaveAsync();
        }

        private async Task<Company> GetCompanyAndCheckIfItExists(Guid id, bool trackChanges)
        {
            var company = await _repository.Company.GetCompanyAsync(id, trackChanges);
            if (company is null)
            throw new CompanyNotFoundException(id);
            return company;
        }
    }
}