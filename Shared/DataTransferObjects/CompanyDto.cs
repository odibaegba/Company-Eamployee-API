using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shared.DataTransferObjects
{
   // [Serializable] this is used for returning an tex/xml when making use of a parameterized type of Dto
    public record CompanyDto 
    {
        public Guid Id { get; init; }
        public string? Name { get; init; }
        public string? FullAddress { get; init; }
    }
   
}