using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.Exceptions.Transaction
{
    public class InvalidTransactionStatusException : InvalidOperationException
    {
        public InvalidTransactionStatusException(string message) : base(message)
        {
            
        }

        public InvalidTransactionStatusException()
        {
            
        }
    }
}
