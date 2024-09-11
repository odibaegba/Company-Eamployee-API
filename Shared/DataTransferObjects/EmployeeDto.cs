using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shared.DataTransferObjects
{
    public record EmployeeDto (Guid Id, string Name, int Age, string Position );
   
}