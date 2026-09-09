namespace BusinessLogicLayer.Exceptions.Transaction
{
    public class TransactionNotFoundException : KeyNotFoundException
    {
        public TransactionNotFoundException(string message) :base(message)
        {
            
        }

        public TransactionNotFoundException()
        {
            
        }
    }
}
