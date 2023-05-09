using ApplicationAuth.Domain.Entities.Identity;
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
    }
}
