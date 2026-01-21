using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DataTransferObject.User
{
    public class UserModel
    {
        public ClaimsIdentity? ClaismIdentity { get; set; }
        public AuthenticationProperties? authProperties { get; set; }
    }
}
