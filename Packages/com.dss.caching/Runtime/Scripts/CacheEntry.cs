using System;

namespace DSS.Caching
{

[Serializable]
public class CacheEntry
{
    public string url;  // the original source url
    public string path;  // on-disc location
    [NonSerialized] public object data;  // in-memory representation
}

}  // namespace DSS.Caching
