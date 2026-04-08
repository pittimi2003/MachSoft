using Microsoft.OpenApi.Extensions;
using Mss.WorkForce.Code.Models.DataAccess;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Web.Common;

namespace Mss.WorkForce.Code.Web.Services
{
    public class TransactionService : ITransactionService
    {

        private readonly DataAccess _dataAccess;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(DataAccess dataAccess, ILogger<TransactionService> logger)
        {
            _logger = logger;
            _dataAccess = dataAccess;
        }

        public async Task DeleteTransactions(List<TransactionDto> lstTransaction)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TransactionDto> GetTransactions(Guid userId)
        {
            List<TransactionDto> transactions = new();

            try
            {
                foreach (var item in _dataAccess.GetTransactions(userId))
                {
                    transactions.Add(MapperTransaction(item));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TransactionService::GetTransactions => Error get list of transactions in database");
            }

            return transactions;
        }

        public TransactionDto GetTransactionById(Guid transactionId)
        {
            throw new NotImplementedException();
        }

        public async Task AddTransaction(TransactionDto dto)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateTransaction(List<TransactionDto> lstTransactionDto)
        {
            throw new NotImplementedException();
        }

        private TransactionDto MapperTransaction(Transaction transaction)
        {
            return new TransactionDto
            {
                Id = transaction.Id,
                CreationDate = transaction.CreationDate,
                WarehouseCode = transaction.WarehouseCode,
                Type = transaction.Type,
                Status = transaction.Status.GetDisplayName(),
                Content = transaction.Content,
                ContentDisplay = JsonTemplateBuilder.BuildContentText(transaction.Content),
                FailureMessage = transaction.FailureMessage,
                Sender = transaction.Sender,
                Recipient = transaction.Recipient,
                UserName = transaction.UserName,
            };
        }

        private Transaction MapDtoToTransaction(TransactionDto dto)
        {
            return new Transaction
            {
                Id = dto.Id,
                CreationDate = dto.CreationDate,
                WarehouseCode = dto.WarehouseCode,
                Type = dto.Type,
                Content = dto.Content,
                Status = getStatus(dto.Status),
                FailureMessage = dto.FailureMessage,
                Sender = dto.Sender,
                Recipient = dto.Recipient,
                UserName = dto.UserName,
            };
        }

        private MessageStatus getStatus(string status)
        {
            if (status == MessageStatus.Success.GetDisplayName())
                return MessageStatus.Success;
            if (status == MessageStatus.Failure.GetDisplayName())
                return MessageStatus.Failure;

            throw new Exception("Invalid status");
        }

    }
}
