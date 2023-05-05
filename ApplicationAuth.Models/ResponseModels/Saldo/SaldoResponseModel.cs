using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationAuth.Models.ResponseModels.Saldo
{
    public class SaldoResponseModel
    {
        public string AccountNumber { get; set; }
        public double Balance { get; set; }
        public bool Status { get; set; }
    }
}
