using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.Exceptions.Auth
{
    public class UserRegistrationException : KeyNotFoundException
    {
        public UserRegistrationException(string message) : base(message)
        {
            
        }

        public UserRegistrationException()
        {
            
        }
    }
}
