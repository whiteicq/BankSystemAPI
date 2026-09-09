using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.Exceptions.Deposit
{
    public class DepositNotFoundException : KeyNotFoundException
    {
        public DepositNotFoundException(string message) : base(message)
        {

        }

        public DepositNotFoundException()
        {
            
        }
    }
}
