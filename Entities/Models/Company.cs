using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Entities.Models
{
    public class Company
    {
        [Column("CompanyId")]
        public Guid Id {get; set;}
        public string? Name {get; set;}
        public string? Address {get; set;}
        public string? Country {get; set;}
        public ICollection<Employee>? Employees{get; set;}

    }
}