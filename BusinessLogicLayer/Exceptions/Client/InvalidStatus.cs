using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.Exceptions.Client
{
    internal class InvalidStatus : InvalidOperationException
    {
        public InvalidStatus(string message) : base(message) { }
        public InvalidStatus() { }
    }
}
