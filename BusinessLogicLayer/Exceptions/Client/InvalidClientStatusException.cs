using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.Exceptions.Client
{
    public class InvalidClientStatusException : InvalidOperationException
    {
        public InvalidClientStatusException(string message) : base(message) { }
        public InvalidClientStatusException() { }
    }
}
