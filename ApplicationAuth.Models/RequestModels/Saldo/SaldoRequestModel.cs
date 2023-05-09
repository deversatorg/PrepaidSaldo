using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationAuth.Models.RequestModels.Saldo
{
    public class SaldoRequestModel
    {
        public string CardNumber { get; set; }
        public string SecretCode { get; set; }
    }
}
