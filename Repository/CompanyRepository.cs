using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Repository
{
    public class CompanyRepository : RepositoryBase<Company>, ICompanyRepository
    {
        public CompanyRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {
            
        }

        public void CreateCompany(Company company) => Create(company);

        public void DeleteCompany(Company company) => Delete(company);
        

        public async Task<IEnumerable<Company>> GetAllCompaniesAsync(bool trackChanges) => 
         await FindAll(trackChanges).OrderBy(x => x.Name).ToListAsync();

        public async Task<IEnumerable<Company>> GetByIdsAsync(IEnumerable<Guid> ids, bool trackChanges)
        => await FindByCondition(x => ids.Contains(x.Id), trackChanges).ToListAsync();

        public async Task<Company> GetCompanyAsync(Guid companyId, bool trackChanges) => 
        await FindByCondition(c => c.Id.Equals(companyId), trackChanges).SingleOrDefaultAsync();
        
    }
}