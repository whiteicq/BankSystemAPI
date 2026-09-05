using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.Exceptions.Bank
{
    public class BankNotFoundException : KeyNotFoundException
    {
        public BankNotFoundException()
        {
            
        }

        public BankNotFoundException(string message) : base(message)
        {
            
        }
    }
}
