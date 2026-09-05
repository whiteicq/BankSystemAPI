using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.Exceptions.BankAccount
{
    public class InvalidBankAccountStatusException : InvalidOperationException
    {
        public InvalidBankAccountStatusException()
        {

        }

        public InvalidBankAccountStatusException(string message) : base(message)
        {
            
        }
    }
}
