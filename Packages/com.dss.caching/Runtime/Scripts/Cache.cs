using DSS.CoreUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

namespace DSS.Caching
{

public class Cache : AutomaticSingleton<Cache>
{
    // --------------- //
    // ENUMS & CLASSES //
    // --------------- //

    private class DownloadCompleteEvent : UnityEvent<object> { }
    private class DownloadFailedEvent : UnityEvent<string> { }

    private class Download
    {
        public DownloadCompleteEvent onDownloadComplete = new DownloadCompleteEvent();
        public DownloadFailedEvent onDownloadFailed = new DownloadFailedEvent();
    }

    // --------- //
    // VARIABLES //
    // --------- //

    private CacheIndex m_index = null;

    private Dictionary<string, Download> m_downloadsInProgress = new Dictionary<string, Download>();

    // ---------- //
    // PROPERTIES //
    // ---------- //

    public string cacheDirectoryPath
    {
        get { return Path.Combine(Application.persistentDataPath, "com.dss.caching", "cache"); }
    }

    public string cacheIndexFilePath
    {
        get { return Path.Combine(Application.persistentDataPath, "com.dss.caching", "cache.json"); }
    }

    // --------- //
    // DELEGATES //
    // --------- //
    
    private delegate IEnumerator DownloadObject(string url, Action<object> onSuccess, Action<string> onError);
    private delegate void WriteObject(string path, object obj);
    private delegate object ReadObject(string path);

    // ------- //
    // METHODS //
    // ------- //

    public void SaveCache()
    {
        Debug.Log("Cache: Attempting to save index to disc");

        string json = JsonUtility.ToJson(m_index);
        
        FileInfo info = new FileInfo(cacheIndexFilePath);
        info.Directory.Create();

        File.WriteAllText(cacheIndexFilePath, json);
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
        m_index = new CacheIndex();
    }

    // ------- //
    // GETTERS //
    // ------- //

    public void RequestSprite(string url, Action<Sprite> onSuccess, Action<string> onError)
    {
        Request(url,
        (obj) =>
        {
            onSuccess((Sprite)obj);
        },
        onError,
        CacheUtilities.DownloadSprite,
        CacheUtilities.WriteSprite,
        CacheUtilities.ReadSprite);
    }

    public void RequestAudioClip(string url, Action<AudioClip> onSuccess, Action<string> onError)
    {
        Request(url,
        (obj) =>
        {
            onSuccess((AudioClip)obj);
        },
        onError,
        CacheUtilities.DownloadAudioClip,
        CacheUtilities.WriteAudioClip,
        CacheUtilities.ReadAudioClip);
    }

    public void RequestString(string url, Action<string> onSuccess, Action<string> onError)
    {
        Request(url,
        (obj) =>
        {
            onSuccess((string)obj);
        },
        onError,
        CacheUtilities.DownloadString,
        CacheUtilities.WriteString,
        CacheUtilities.ReadString);
    }

    private void Request(string url, Action<object> onSuccess, Action<string> onError, DownloadObject downloader, WriteObject writer, ReadObject reader)
    {
        try
        {
            // Not on disc or in memory, so download from source
            if (!m_index.ContainsKey(url))
            {
                // First check if we are already downloading. If we are, then wait for the download to finish
                if (m_downloadsInProgress.ContainsKey(url))
                {
                    m_downloadsInProgress[url].onDownloadComplete.AddListener((obj) =>
                    {
                        onSuccess(obj);
                    });
                    m_downloadsInProgress[url].onDownloadFailed.AddListener((error) =>
                    {
                        onError(error);
                    });
                }
                // If were not, then download it
                else
                {
                    Download download = new Download();
                    download.onDownloadComplete = new DownloadCompleteEvent();
                    download.onDownloadFailed = new DownloadFailedEvent();

                    m_downloadsInProgress[url] = download;

                    StartCoroutine(downloader(url, 
                    (audioClip) =>
                    {
                        // Store a copy on disc
                        string path = Path.Combine(cacheDirectoryPath, Guid.NewGuid().ToString(), Path.GetFileNameWithoutExtension(url) + ".bin");
                        writer(path, audioClip);

                        // And add a reference to it to the index
                        CacheEntry entry = new CacheEntry();
                        entry.url = url;
                        entry.path = path;
                        entry.data = audioClip;
                        m_index[url] = entry;

                        onSuccess(audioClip);
                        download.onDownloadComplete.Invoke(audioClip);
                        m_downloadsInProgress.Remove(url);
                    },
                    (error) =>
                    {
                        onError(error);
                        download.onDownloadFailed.Invoke(error);
                        m_downloadsInProgress.Remove(url);
                    }));
                }
            }
            else
            {
                CacheEntry entry = m_index[url];

                // If in memory, just use in-memory copy
                if (entry.data != null)
                {
                    onSuccess(entry.data);
                }
                // Stored on disc, so load from disc
                else
                {
                    object obj = reader(entry.path);
    
                    if (obj == null)
                    {
                        throw new ArgumentException("Failed to deserialize file: " + entry.path);
                    }
    
                    entry.data = obj;  // save a copy in-memory
                    onSuccess(obj);
                }
            }
        }
        catch (Exception exception)
        {
            onError(exception.Message);
        }
    }

    /*
    public void RequestSprite(string url, Action<Sprite> onSuccess, Action<string> onError)
    {
        try
        {
            // Not on disc or in memory, so download from source
            if (!m_index.ContainsKey(url))
            {
                // First check if we are already downloading. If we are, then wait for the download to finish
                if (m_downloadsInProgress.ContainsKey(url))
                {
                    m_downloadsInProgress[url].onDownloadComplete.AddListener((obj) =>
                    {
                        onSuccess((Sprite)obj);
                    });
                    m_downloadsInProgress[url].onDownloadFailed.AddListener((error) =>
                    {
                        onError(error);
                    });
                }
                // If were not, then download it
                else
                {
                    Download download = new Download();
                    download.onDownloadComplete = new DownloadCompleteEvent();
                    download.onDownloadFailed = new DownloadFailedEvent();

                    m_downloadsInProgress[url] = download;

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
                        m_index[url] = entry;

                        onSuccess(sprite);
                        download.onDownloadComplete.Invoke(sprite);
                        m_downloadsInProgress.Remove(url);
                    }, 
                    (error) =>
                    {
                        onError(error);
                        download.onDownloadFailed.Invoke(error);
                        m_downloadsInProgress.Remove(url);
                    }));
                }
            }
            else
            {
                CacheEntry entry = m_index[url];

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
            if (!m_index.ContainsKey(url))
            {
                // First check if we are already downloading. If we are, then wait for the download to finish
                if (m_downloadsInProgress.ContainsKey(url))
                {
                    m_downloadsInProgress[url].onDownloadComplete.AddListener((obj) =>
                    {
                        onSuccess((AudioClip)obj);
                    });
                    m_downloadsInProgress[url].onDownloadFailed.AddListener((error) =>
                    {
                        onError(error);
                    });
                }
                // If were not, then download it
                else
                {
                    Download download = new Download();
                    download.onDownloadComplete = new DownloadCompleteEvent();
                    download.onDownloadFailed = new DownloadFailedEvent();

                    m_downloadsInProgress[url] = download;

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
                        m_index[url] = entry;

                        onSuccess(audioClip);
                        download.onDownloadComplete.Invoke(audioClip);
                        m_downloadsInProgress.Remove(url);
                    },
                    (error) =>
                    {
                        onError(error);
                        download.onDownloadFailed.Invoke(error);
                        m_downloadsInProgress.Remove(url);
                    }));
                }
            }
            else
            {
                CacheEntry entry = m_index[url];

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
            if (!m_index.ContainsKey(url))
            {
                // First check if we are already downloading. If we are, then wait for the download to finish
                if (m_downloadsInProgress.ContainsKey(url))
                {
                    m_downloadsInProgress[url].onDownloadComplete.AddListener((obj) =>
                    {
                        onSuccess((string)obj);
                    });
                    m_downloadsInProgress[url].onDownloadFailed.AddListener((error) =>
                    {
                        onError(error);
                    });
                }
                // If were not, then download it
                else
                {
                    Download download = new Download();
                    download.onDownloadComplete = new DownloadCompleteEvent();
                    download.onDownloadFailed = new DownloadFailedEvent();

                    m_downloadsInProgress[url] = download;
                    
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
                        m_index[url] = entry;

                        onSuccess(message);
                        download.onDownloadComplete.Invoke(message);
                        m_downloadsInProgress.Remove(url);
                    },
                    (error) =>
                    {
                        onError(error);
                        download.onDownloadFailed.Invoke(error);
                        m_downloadsInProgress.Remove(url);
                    }));
                }
            }
            else
            {
                CacheEntry entry = m_index[url];

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
    */

    private void Awake()
    {
        try
        {
            Debug.Log("Cache: Attempting to load index from disc");

            string json = File.ReadAllText(cacheIndexFilePath);
            m_index = JsonUtility.FromJson<CacheIndex>(json);
        }
        catch (Exception exception)
        {
            Debug.Log($"Cache: creating new index ({exception.Message})");
            m_index = new CacheIndex();
        }
    }

    private void OnDestroy()
    {
        // Save cache index to disc when the app quits
        SaveCache();
    }
}

}  // namespace DSS.Caching
