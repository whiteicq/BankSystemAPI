using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.Exceptions.Deposit
{
    public class InvalidDepositStatusException : InvalidOperationException
    {
        public InvalidDepositStatusException(string message) : base(message)
        {
            
        }

        public InvalidDepositStatusException()
        {
            
        }
    }
}
