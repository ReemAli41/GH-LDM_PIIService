using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GH_LDM_PIIService.Entities.Request
{
    public class AuthRequestDto
    {
        public string GrantType { get; set; } 
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string ClientAuthenticationMethod { get; set; } 
    }
}
