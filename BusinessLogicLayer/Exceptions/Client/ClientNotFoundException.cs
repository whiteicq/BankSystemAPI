using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.Exceptions.Client
{
    public class ClientNotFoundException : KeyNotFoundException
    {
        public ClientNotFoundException(string message) : base(message)
        {
            
        }

        public ClientNotFoundException() { }
    }
}
