using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Entities.Exceptions
{
    public class CompanyNotFoundException : NotFoundException
    {
        public CompanyNotFoundException(Guid companyId) : base($"The company with id : {companyId}, dosen't exist in the DB")
        {
            
        }
    }
}