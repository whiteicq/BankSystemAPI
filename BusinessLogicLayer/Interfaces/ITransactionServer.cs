using DataAccessLayer.Enums.Common;
using DataAccessLayer.Enums.Transaction;
using DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.Interfaces
{
    public interface ITransactionService
    {
        void TransferMoney(decimal amount, long senderId, long recieverId, TransactionType type, CurrencyType currency);
    }
}
