using ApplicationAuth.Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationAuth.Domain.Entities.Saldo
{
    public class SaldoProfile
    {
        [ForeignKey("User")]
        public int Id { get; set; }
        public string AccountNumber { get; set; }

        public string SecureCode { get; set; }

        public double Balance { get; set; }

        public bool Status { get; set; }

        #region Navigation propeties
        [InverseProperty("Saldo")]
        public virtual ApplicationUser User { get; set; }
        #endregion 
    }
}
