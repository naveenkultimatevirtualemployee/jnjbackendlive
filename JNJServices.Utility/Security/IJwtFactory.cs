using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace JNJServices.Utility.Security
{
    public interface IJwtFactory
    {
        string GenerateToken(List<Claim> claims);
    }
}
