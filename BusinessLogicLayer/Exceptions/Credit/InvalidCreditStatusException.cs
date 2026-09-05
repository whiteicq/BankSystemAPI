using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.Exceptions.Credit
{
    public class InvalidCreditStatusException : InvalidOperationException
    {
        public InvalidCreditStatusException(string message) : base(message)
        {
            
        }
    }
}
