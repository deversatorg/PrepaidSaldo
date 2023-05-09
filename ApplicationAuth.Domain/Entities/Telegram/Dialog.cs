using ApplicationAuth.Domain.Entities.Identity;
using ApplicationAuth.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationAuth.Domain.Entities.Telegram
{
    public class Dialog : IEntity<int>
    {
        #region Properties
        public int Id { get; set; }
        public DialogType Type { get; set; }
        public int CurrentStep { get; set; }
        public int CountOfSteps { get; set; }
        public bool InProccess { get; set; }
        public int? UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        #endregion

        #region Navigation properties
        [ForeignKey("UserId")]
        [InverseProperty("Dialogs")]
        public virtual ApplicationUser User { get; set; }
        #endregion
    }
}
