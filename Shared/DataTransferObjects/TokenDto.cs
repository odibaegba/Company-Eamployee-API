using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shared.DataTransferObjects
{
    public record TokenDto(string AccessToken, string RefreshToken);
    
}