using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.Interfaces
{
    public interface IRegisterService
    {
        void RegisterClient(string email, string password);
    }
}
