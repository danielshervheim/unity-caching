using System;
using System.Collections.Generic;
using UnityEngine;

namespace DSS.Caching
{

public class DownloadManager
{
    // --------------- //
    // CLASSES & ENUMS //
    // --------------- //

    private enum DownloadType { Sprite, AudioClip, String };

    private struct Downloadable
    {
        public string url;
        public DownloadType type;
        public Action<object> onSuccess;
        public Action<string> onError;
    }

    // --------- //
    // VARIABLES //
    // --------- //

    private List<Downloadable> m_downloadables;
    private bool m_downloading;

    // ---------- //
    // PROPERTIES //
    // ---------- //

    private Cache cache
    {
        get { return Cache.Instance; }
    }

    // ------- //
    // METHODS //
    // ------- //
    
    public DownloadManager()
    {
        m_downloadables = new List<Downloadable>();    
        m_downloading = false;
    }

    public void AddSprite(string url, Action<Sprite> onSuccess, Action<string> onError)
    {
        Downloadable d = new Downloadable();
        d.url = url;
        d.type = DownloadType.Sprite;
        d.onSuccess = ((object obj) => { onSuccess((Sprite)obj); });
        d.onError = onError;

        m_downloadables.Add(d);
    }

    public void AddAudioClip(string url, Action<AudioClip> onSuccess, Action<string> onError)
    {
        Downloadable d = new Downloadable();
        d.url = url;
        d.type = DownloadType.AudioClip;
        d.onSuccess = ((object obj) => { onSuccess((AudioClip)obj); });
        d.onError = onError;

        m_downloadables.Add(d);
    }

    public void AddString(string url, Action<string> onSuccess, Action<string> onError)
    {
        Downloadable d = new Downloadable();
        d.url = url;
        d.type = DownloadType.String;
        d.onSuccess = ((object obj) => { onSuccess((string)obj); });
        d.onError = onError;

        m_downloadables.Add(d);
    }

    public void Download(Action onComplete)
    {
        if (m_downloading)
        {
            return;
        }

        int finished = 0;

        for (int i = 0; i < m_downloadables.Count; i++)
        {
            Downloadable d = m_downloadables[i];

            if (d.type == DownloadType.Sprite)
            {
                cache.RequestSprite(d.url,
                (sprite) =>
                {
                    d.onSuccess(sprite);

                    finished += 1;
                    if (finished == m_downloadables.Count)
                    {
                        onComplete();
                        m_downloading = false;
                    }
                },
                (error) =>
                {
                    d.onError(error);
                    
                    finished += 1;
                    if (finished == m_downloadables.Count)
                    {
                        onComplete();
                        m_downloading = false;
                    }
                });
            }
            else if (d.type == DownloadType.AudioClip)
            {
                cache.RequestAudioClip(d.url,
                (audioClip) =>
                {
                    d.onSuccess(audioClip);

                    finished += 1;
                    if (finished == m_downloadables.Count)
                    {
                        onComplete();
                        m_downloading = false;
                    }
                },
                (error) =>
                {
                    d.onError(error);

                    finished += 1;
                    if (finished == m_downloadables.Count)
                    {
                        onComplete();
                        m_downloading = false;
                    }
                });
            }
            else if (d.type == DownloadType.String)
            {
                cache.RequestString(d.url,
                (text) =>
                {
                    d.onSuccess(text);

                    finished += 1;
                    if (finished == m_downloadables.Count)
                    {
                        onComplete();
                        m_downloading = false;
                    }
                },
                (error) =>
                {
                    d.onError(error);

                    finished += 1;
                    if (finished == m_downloadables.Count)
                    {
                        onComplete();
                        m_downloading = false;
                    }
                });
            }
            else
            {
                finished += 1;
                if (finished == m_downloadables.Count)
                {
                    onComplete();
                    m_downloading = false;
                }
            }
        }
    }
}

}  // namespace DSS.Caching