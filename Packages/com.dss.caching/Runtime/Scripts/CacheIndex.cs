using System;
using System.Collections.Generic;
using UnityEngine;

namespace DSS.Caching
{

[Serializable]
public class CacheIndex
{
    [SerializeField] private List<CacheEntry> index;

    public bool ContainsKey(string key)
    {
        if (index == null)
        {
            index = new List<CacheEntry>();
            return false;
        }

        foreach(CacheEntry entry in index)
        {
            if (entry.url.Equals(key))
            {
                return true;
            }
        }
        return false;
    }

    // Gross workaround because dictionaries aren't serializable :(
    public CacheEntry this[string url]
    {
        get
        {
            if (index == null)
            {
                index = new List<CacheEntry>();
                throw new KeyNotFoundException();
            }

            foreach (CacheEntry entry in index)
            {
                if (entry.url.Equals(url))
                {
                    return entry;
                }
            }
            throw new KeyNotFoundException();
        }
        set
        {
            if (index == null)
            {
                index = new List<CacheEntry>();
            }

            CacheEntry toReplace = null;
            foreach (CacheEntry entry in index)
            {
                if (entry.url.Equals(url))
                {
                    toReplace = entry;
                }
            }
            
            if (toReplace != null)
            {
                index.Remove(toReplace);
            }
            index.Add(value);
        }
    }
}

}  // namespace DSS.Caching
