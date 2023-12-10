using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CourseManagement
{
    public class ImprovedDictionary<TKey, TValue> : Dictionary<TKey, List<TValue>>
    {
        public void Add(TKey key, TValue value)
        {
            if (ContainsKey(key))
                this[key].Add(value);
            else
                Add(key, new List<TValue> { value });
        }
    }
}