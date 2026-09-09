using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.Exceptions.Transaction
{
    public class InsufficientFundsException : InvalidOperationException
    {
        public InsufficientFundsException(string message) : base(message)
        {
            
        }

        public InsufficientFundsException()
        {
            
        }
    }
}
