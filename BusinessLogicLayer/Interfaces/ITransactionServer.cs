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
        Transaction TransferMoney(long userId, decimal amount, string senderBankAccountNumber, string recieverBankAccountNumber, TransactionType type = TransactionType.PeerToPeer, CurrencyType currency = CurrencyType.BYN);
    }
}
