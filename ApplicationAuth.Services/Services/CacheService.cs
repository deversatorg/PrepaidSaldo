using ApplicationAuth.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationAuth.Services.Services
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _cache;

        public CacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public T Get<T>(string key)
        {
            return _cache.Get<T>(key);
        }

        public void Set<T>(string key, T value, DateTimeOffset absoluteExpiration)
        {
            _cache.Set(key, value, absoluteExpiration);
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }

        public List<KeyValuePair<object, object>> GetAllKeyValuePairs()
        {
            var keyValuePairs = new List<KeyValuePair<object, object>>();
            var field = typeof(MemoryCache).GetProperty("EntriesCollection", BindingFlags.NonPublic | BindingFlags.Instance);
            var collection = field.GetValue(_cache) as ICollection;

            //var cacheEntriesCollection = typeof(MemoryCache)
            //.GetProperty("EntriesCollection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            //.GetValue(_cache) as ICollection<KeyValuePair<object, object>>;

            foreach (var cacheEntry in collection)
            {
                keyValuePairs.Add(new KeyValuePair<object, object>(cacheEntry.GetType().GetProperty("Key").GetValue(cacheEntry), cacheEntry.GetType().GetProperty("Value").GetValue(cacheEntry)));
            }

            return keyValuePairs;
        }

        #region IDisposable Support

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).                  
                    _cache.Dispose();
                }

                disposedValue = true;
            }
        }

        ~CacheService()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
