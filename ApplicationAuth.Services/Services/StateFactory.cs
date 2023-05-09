using ApplicationAuth.Domain.State;
using ApplicationAuth.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationAuth.Services.Services
{
    public class StateFactory : IStateFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public StateFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IState<T> State<T>() where T : class, new()
        {
            IState<T> state = _serviceProvider.GetRequiredService<IState<T>>();
            return state;
        }

    }
}
