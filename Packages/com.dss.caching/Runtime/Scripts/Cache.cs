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

    public void RequestSprite(string url, Action<Sprite> onSuccess = null, Action<string> onError = null)
    {
        Request<Sprite>(url,
        CacheUtilities.DownloadSprite,
        CacheUtilities.WriteSprite,
        CacheUtilities.ReadSprite,
        onSuccess,
        onError,
        ".png");
    }

    public void RequestAudioClip(string url, Action<AudioClip> onSuccess = null, Action<string> onError = null)
    {
        Request<AudioClip>(url,
        CacheUtilities.DownloadAudioClip,
        CacheUtilities.WriteAudioClip,
        CacheUtilities.ReadAudioClip,
        onSuccess,
        onError);
    }

    public void RequestString(string url, Action<string> onSuccess = null, Action<string> onError = null)
    {
        string extension = Path.GetExtension(url);
        if (extension.Equals(string.Empty))
        {
            extension = ".txt";
        }

        Request<string>(url,
        CacheUtilities.DownloadString,
        CacheUtilities.WriteString,
        CacheUtilities.ReadString,
        onSuccess,
        onError,
        extension);
    }

    private void Request<T>(
        string url,
        DownloadObject downloader,
        WriteObject writer,
        ReadObject reader,
        Action<T> onSuccess = null,
        Action<string> onError = null,
        string extension=".bin")
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
                        onSuccess?.Invoke((T)obj);
                        // onSuccess((T)obj);
                    });
                    m_downloadsInProgress[url].onDownloadFailed.AddListener((error) =>
                    {
                        onError?.Invoke(error);
                        // onError(error);
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
                    (obj) =>
                    {
                        // Store a copy on disc
                        string relativePath = Path.Combine(Guid.NewGuid().ToString(), Path.GetFileNameWithoutExtension(url) + extension);
                        string path = Path.Combine(cacheDirectoryPath, relativePath);
                        writer(path, obj);

                        // And add a reference to it to the index
                        CacheEntry entry = new CacheEntry();
                        entry.url = url;
                        entry.path = relativePath;
                        entry.data = obj;
                        m_index[url] = entry;

                        onSuccess?.Invoke((T)obj);
                        download.onDownloadComplete.Invoke(obj);
                        m_downloadsInProgress.Remove(url);
                    },
                    (error) =>
                    {
                        onError?.Invoke(error);
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
                    onSuccess?.Invoke((T)entry.data);
                }
                // Stored on disc, so load from disc
                else
                {
                    string fullPath = Path.Combine(cacheDirectoryPath, entry.path);
                    object obj = reader(fullPath);
    
                    if (obj == null)
                    {
                        throw new ArgumentException("Failed to deserialize file: " + fullPath);
                    }
    
                    entry.data = obj;  // save a copy in-memory
                    onSuccess?.Invoke((T)obj);
                }
            }
        }
        catch (Exception exception)
        {
            onError?.Invoke(exception.Message);
        }
    }

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
