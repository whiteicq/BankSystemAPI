using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace BusinessLogicLayer.Interfaces
{
    public interface IEmployeeService
    {
        // актвировать клиента
        void ActivateClient(long clientId);

        // заблокировать клиента
        void BlockClient(long clientId);

        // активировать счет
        void ActivateBankAccount(long bankAccountId);

        // заморозить счет
        void FreezeBankAccount(long bankAccountId);

        // закрыть счет
        void CloseBankAccount(long bankAccountId);

        // заблокировать счет
        void BlockBankAccount(long bankAccountId);

        // активировать(одобрить) кредит
        void ActivateCredit(long clientId, long creditId, long bankAccountRecieverId);

        // отклонить (не одобрить) кредит
        void RejectCredit(long creditId);

        // активировать(одобрить) депозит
        void ActivateDeposit(long clientId, long depositId, long bankAccountSenderId);

        // отклонить (не одобрить) депозит
        void RejectDeposit(long depositId);

        // отменить транзакцию
        void CancelTransaction(long transactionId);
    }
}
