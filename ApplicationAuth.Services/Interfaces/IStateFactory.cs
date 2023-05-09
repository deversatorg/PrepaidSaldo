using ApplicationAuth.Domain.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationAuth.Services.Interfaces
{
    public interface IStateFactory 
    {
        public IState<T> State<T>() where T : class, new();
    }
}
