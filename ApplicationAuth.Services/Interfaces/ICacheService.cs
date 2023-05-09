using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationAuth.Services.Interfaces
{
    public interface ICacheService : IDisposable
    {
        public T Get<T>(string key);

        public void Set<T>(string key, T value, DateTimeOffset absoluteExpiration);

        public void Remove(string key);

        List<KeyValuePair<object, object>> GetAllKeyValuePairs();
    }
}
