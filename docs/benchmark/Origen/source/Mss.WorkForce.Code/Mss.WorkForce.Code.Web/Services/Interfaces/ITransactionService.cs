using Mss.WorkForce.Code.Models.DTO;

namespace Mss.WorkForce.Code.Web.Services
{
    public interface ITransactionService
    {

        #region Methods

        Task DeleteTransactions(List<TransactionDto> lstTransaction);
        IEnumerable<TransactionDto> GetTransactions(Guid userId);
        TransactionDto GetTransactionById(Guid transactionId);
        Task UpdateTransaction(List<TransactionDto> lstTransaction);
        Task AddTransaction(TransactionDto transaction);

        #endregion

    }
}
