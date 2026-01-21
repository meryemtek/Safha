using DataTransferObject.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BussinessLogicLayer.Interfaceses
{
    public interface IAuthManager
    {

        Task<UserModel> Login(string username, string password,bool RememberMe) ;

    }

    
}
