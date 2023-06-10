using ApplicationAuth.Domain.Entities.Identity;
using ApplicationAuth.Models.Enums;
using ApplicationAuth.Models.RequestModels;
using ApplicationAuth.Models.RequestModels.Saldo;
using ApplicationAuth.Models.ResponseModels;
using ApplicationAuth.Models.ResponseModels.Saldo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationAuth.Services.Interfaces
{
    public interface ISaldoService
    {
        Task<SaldoResponseModel> Get(ApplicationUser user);
        Task<string> DeleteSaldo(ApplicationUser user);
        #region table
        Task<PaginationResponseModel<TransactionTableRowResponseModel>> GetTransactionsHistory(SaldoPaginationRequestModel<SaldoTableColumn> model, ApplicationUser user);
        Task<List<string>> GetHistoryPeriods(ApplicationUser user);
        Task<TransactionResponseModel> GetTransaction(ApplicationUser user, string transaction, int page, string period);
        #endregion
    }
}
