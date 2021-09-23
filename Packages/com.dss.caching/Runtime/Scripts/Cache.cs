using DSS.CoreUtils;
using System;
using System.IO;
using UnityEngine;

namespace DSS.Caching
{

public class Cache : AutomaticSingleton<Cache>
{
    public string cacheDirectoryPath
    {
        get { return Path.Combine(Application.persistentDataPath, "com.dss.caching", "cache"); }
    }

    public string cacheIndexFilePath
    {
        get { return Path.Combine(Application.persistentDataPath, "com.dss.caching", "cache.json"); }
    }

    private CacheIndex index;

    private void Awake()
    {
        // Try to load the cache from disc when the app starts
        if (File.Exists(cacheIndexFilePath))
        {
            Debug.Log("Cache: loading index from disc");

            string indexJson = File.ReadAllText(cacheIndexFilePath);
            index = JsonUtility.FromJson<CacheIndex>(indexJson);
        }
        else
        {
            Debug.Log("Cache: creating new index");
    
            index = new CacheIndex();
        }
    }

    private void OnDestroy()
    {
        // Save cache index to disc when the app quits
        SaveCache();
    }

    public void SaveCache()
    {
        Debug.Log("Cache: saving index to disc");

        string indexJson = JsonUtility.ToJson(index);
        
        FileInfo info = new FileInfo(cacheIndexFilePath);
        info.Directory.Create();

        File.WriteAllText(cacheIndexFilePath, indexJson);
    }

    public void ClearCache()
    {
        Debug.Log("Cache: clearing cache");

        // Delete the cached files
        if (Directory.Exists(cacheDirectoryPath))
        {
            Directory.Delete(cacheDirectoryPath, true);
        }

        // Delete the on-disc index
        if (File.Exists(cacheIndexFilePath))
        {
            File.Delete(cacheIndexFilePath);
        }

        // Recreate a new in-memory index
        index = new CacheIndex();
    }

    // ------- //
    // GETTERS //
    // ------- //

    public void RequestSprite(string url, Action<Sprite> onSuccess, Action<string> onError)
    {
        try
        {
            // Not on disc or in memory, so download from source
            if (!index.ContainsKey(url))
            {
                StartCoroutine(CacheUtilities.DownloadSprite(url, 
                (sprite) =>
                {
                    // Store a copy on disc
                    string path = Path.Combine(cacheDirectoryPath, Guid.NewGuid().ToString(), Path.GetFileNameWithoutExtension(url) + ".png");
                    CacheUtilities.WriteSprite(path, sprite);

                    // And add a reference to it to the index
                    CacheEntry entry = new CacheEntry();
                    entry.url = url;
                    entry.path = path;
                    entry.data = sprite;
                    index[url] = entry;

                    onSuccess(sprite);
                }, onError));
            }
            else
            {
                CacheEntry entry = index[url];

                // If in memory, just use in-memory copy
                if (entry.data != null)
                {
                    Sprite sprite = (Sprite)entry.data;
                    onSuccess(sprite);
                }
                // Stored on disc, so load from disc
                else
                {
                    Sprite sprite = CacheUtilities.ReadSprite(entry.path);
    
                    if (sprite == null)
                    {
                        throw new ArgumentException("Failed to deserialize file: " + entry.path);
                    }
    
                    entry.data = sprite;  // save a copy in-memory
                    onSuccess(sprite);
                }
            }
        }
        catch (Exception exception)
        {
            onError(exception.Message);
        }
    }

    public void RequestAudioClip(string url, Action<AudioClip> onSuccess, Action<string> onError)
    {
        try
        {
            // Not on disc or in memory, so download from source
            if (!index.ContainsKey(url))
            {
                StartCoroutine(CacheUtilities.DownloadAudioClip(url, 
                (audioClip) =>
                {
                    // Store a copy on disc
                    string path = Path.Combine(cacheDirectoryPath, Guid.NewGuid().ToString(), Path.GetFileNameWithoutExtension(url) + ".bin");
                    CacheUtilities.WriteAudioClip(path, audioClip);

                    // And add a reference to it to the index
                    CacheEntry entry = new CacheEntry();
                    entry.url = url;
                    entry.path = path;
                    entry.data = audioClip;
                    index[url] = entry;

                    onSuccess(audioClip);
                }, onError));
            }
            else
            {
                CacheEntry entry = index[url];

                // If in memory, just use in-memory copy
                if (entry.data != null)
                {
                    AudioClip audioClip = (AudioClip)entry.data;
                    onSuccess(audioClip);
                }
                // Stored on disc, so load from disc
                else
                {
                    AudioClip audioClip = CacheUtilities.ReadAudioClip(entry.path);
    
                    if (audioClip == null)
                    {
                        throw new ArgumentException("Failed to deserialize file: " + entry.path);
                    }
    
                    entry.data = audioClip;  // save a copy in-memory
                    onSuccess(audioClip);
                }
            }
        }
        catch (Exception exception)
        {
            onError(exception.Message);
        }
    }

    public void RequestString(string url, Action<string> onSuccess, Action<string> onError)
    {
        try
        {
            // Not on disc or in memory, so download from source
            if (!index.ContainsKey(url))
            {
                StartCoroutine(CacheUtilities.DownloadString(url, 
                (message) =>
                {
                    // Store a copy on disc
                    string path = Path.Combine(cacheDirectoryPath, Guid.NewGuid().ToString(), Path.GetFileNameWithoutExtension(url) + ".bin");
                    CacheUtilities.WriteString(path, message);

                    // And add a reference to it to the index
                    CacheEntry entry = new CacheEntry();
                    entry.url = url;
                    entry.path = path;
                    entry.data = message;
                    index[url] = entry;

                    onSuccess(message);
                }, onError));
            }
            else
            {
                CacheEntry entry = index[url];

                // If in memory, just use in-memory copy
                if (entry.data != null)
                {
                    string message = (string)entry.data;
                    onSuccess(message);
                }
                // Stored on disc, so load from disc
                else
                {
                    string message = CacheUtilities.ReadString(entry.path);
    
                    if (message == null)
                    {
                        throw new ArgumentException("Failed to deserialize file: " + entry.path);
                    }
    
                    entry.data = message;  // save a copy in-memory
                    onSuccess(message);
                }
            }
        }
        catch (Exception exception)
        {
            onError(exception.Message);
        }
    }
    
    // // Debugging
    // void Start()
    // {
    //     RequestSprite("https://umn-latis.github.io/virtual-fort-snelling-data/assets/round-tower/1.jpg",
    //     (sprite) =>
    //     {
    //         Debug.Log("1 loaded");
    //     },
    //     (error) =>
    //     {
    //         Debug.LogError(error);
    //     });

    //     RequestAudioClip("https://download.samplelib.com/mp3/sample-3s.mp3",
    //     (audioClip) =>
    //     {
    //         AudioSource source = gameObject.AddComponent<AudioSource>();
    //         source.clip = audioClip;
    //         source.Play();
    //     },
    //     (error) =>
    //     {
    //         Debug.LogError(error);
    //     });

    //     RequestString("https://umn-latis.github.io/virtual-fort-snelling-data/pages.xml",
    //     (message)=>
    //     {
    //         Debug.Log(message);
    //     },
    //     (error) =>
    //     {
    //         Debug.LogError(error);
    //     });
    // }
}

}  // namespace DSS.Caching
