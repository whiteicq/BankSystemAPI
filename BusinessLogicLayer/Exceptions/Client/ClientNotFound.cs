using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.Exceptions.Client
{
    public class ClientNotFound : KeyNotFoundException
    {
        public ClientNotFound(string message) : base(message)
        {
            
        }
    }
}
