using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Entities.Exceptions
{
    public class NotFoundException : Exception
    {
        protected NotFoundException(string message) : base(message)
        {
            
        }
    }
}