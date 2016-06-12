using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApi
{
    public static class DictionaryExtensions
    {
        public static TValue GetRandom<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            var random = new Random();
            return dictionary.ElementAt(random.Next(dictionary.Count)).Value;
        }
    }
}
