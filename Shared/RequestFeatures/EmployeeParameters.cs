using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shared.RequestFeatures
{
    public class EmployeeParameters : RequestParameters
    {
        public EmployeeParameters() => OrderBy = "name";
       
        public uint MinAge { get; set; }
        public uint MaxAge { get; set;} = int.MaxValue;

        //implimentation of Filtering
        public bool ValidAgeRange => MaxAge > MinAge;

        //implimentation of Searching
        public string? SearchTerm { get; set; }
    }
}