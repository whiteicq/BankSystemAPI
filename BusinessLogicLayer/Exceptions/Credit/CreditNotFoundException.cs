using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.Exceptions.Credit
{
    public class CreditNotFoundException : KeyNotFoundException
    {
        public CreditNotFoundException(string message) : base(message)
        {
            
        }

        public CreditNotFoundException()
        {
            
        }
    }
}
