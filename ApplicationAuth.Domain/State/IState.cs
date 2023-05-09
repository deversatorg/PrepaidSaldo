using ApplicationAuth.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationAuth.Domain.State
{
    public interface IState<T> where T : class
    {
        public long Id { get; set; }

        public T Model { get; set; }

        public DialogType Type { get; set; }
        public int CurrentStep { get; set; }
        public int CountOfSteps { get; set; }
        public bool InProccess { get; set; }
    }
}
