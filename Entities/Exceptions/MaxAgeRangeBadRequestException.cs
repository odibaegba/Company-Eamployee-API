using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Entities.Exceptions
{
    public class MaxAgeRangeBadRequestException : BadRequestException
    {
        public MaxAgeRangeBadRequestException() : base("Max age can't be less than min age.")
        {
            
        }
    }
}