using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationAuth.Models.ResponseModels.Saldo
{
    public class TransactionResponseModel
    {
        public string Date { get; set; }
        public string Company { get; set; }
        public string Description { get; set; }
        public string TransactionType { get; set; }
        public string DebitCredit { get; set; }
        public string Amount { get; set; }
    }
}
