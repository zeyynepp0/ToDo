using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDo.Application.DTOs.Auth
{
    public record TokenResponse(string AccessToken, string RefreshToken);
}
