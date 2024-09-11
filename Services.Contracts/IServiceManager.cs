using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Contracts
{
    public interface IServiceManager
    {
        ICompanyService companyService { get; }
        IEmployeeService employeeService { get; }
        IAuthenticationService AuthenticationService { get; }
    }
}