using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DownloadManagerTest : MonoBehaviour
{
    public string[] spriteUrls;

    public RectTransform container;

    // Start is called before the first frame update
    void Start()
    {
        DSS.Caching.DownloadManager dm = new DSS.Caching.DownloadManager();

        foreach (string url in spriteUrls)
        {
            dm.AddSprite(url,
            (sprite) =>
            {
                Debug.Log("downloaded " + url);
                GameObject go = new GameObject(url, typeof(RectTransform));
                go.transform.SetParent(container.transform);

                UnityEngine.UI.Image i = go.AddComponent<UnityEngine.UI.Image>();
                i.sprite = sprite;
            },
            (error) =>
            {
                Debug.LogError("failed " + url);
            });
        }    

        dm.Download(() =>
        {
            Debug.Log("finished!");
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
