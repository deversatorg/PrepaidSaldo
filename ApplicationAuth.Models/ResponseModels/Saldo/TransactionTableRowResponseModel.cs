using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationAuth.Models.ResponseModels.Saldo
{
    public class TransactionTableRowResponseModel
    {
        public string Date { get; set; }
        public string Company { get; set; }
        public string TransactionType { get; set; }
        public string DebitOrCredit { get; set; }
        public string Amount { get; set; }
    }
}
