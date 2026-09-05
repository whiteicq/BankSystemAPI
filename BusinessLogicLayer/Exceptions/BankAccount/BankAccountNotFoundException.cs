using Microsoft.Identity.Client.Extensibility;
using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.Exceptions.BankAccount
{
    public class BankAccountNotFoundException : KeyNotFoundException
    {
        public BankAccountNotFoundException()
        {
            
        }

        public BankAccountNotFoundException(string message) : base(message) 
        {
            
        }
    }
}
