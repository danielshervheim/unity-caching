using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CacheTester : MonoBehaviour
{
    private DSS.Caching.Cache cache
    {
        get { return DSS.Caching.Cache.Instance; }
    }

    void Start()
    {
        // Image
     
        cache.RequestSprite("https://upload.wikimedia.org/wikipedia/commons/0/01/Milky_way.png",
        (sprite) =>
        {
            Debug.Log("success 1");
        },
        (error) =>
        {
            Debug.LogError("error 1: " + error);
        }); 

        cache.RequestSprite("https://upload.wikimedia.org/wikipedia/commons/0/01/Milky_way.png",
        (sprite) =>
        {
            Debug.Log("success 2");
        },
        (error) =>
        {
            Debug.LogError("error 2: " + error);
        }); 

        // Audio

        cache.RequestAudioClip("https://file-examples-com.github.io/uploads/2017/11/file_example_MP3_2MG.mp3",
        (clip) =>
        {
            Debug.Log("success 3");
        },
        (error) =>
        {
            Debug.LogError("error 3: " + error);
        }); 

        cache.RequestAudioClip("https://file-examples-com.github.io/uploads/2017/11/file_example_MP3_2MG.mp3",
        (clip) =>
        {
            Debug.Log("success 4");
        },
        (error) =>
        {
            Debug.LogError("error 4: " + error);
        });

        // Text

        cache.RequestString("https://norvig.com/big.txt",
        (text) =>
        {
            Debug.Log("success 5");
        },
        (error) =>
        {
            Debug.LogError("error 5: " + error);
        }); 

        cache.RequestString("https://norvig.com/big.txt",
        (text) =>
        {
            Debug.Log("success 6");
        },
        (error) =>
        {
            Debug.LogError("error 6: " + error);
        });

        // These should fail

        cache.RequestString("https://norvig.com/gib.txt",
        (text) =>
        {

        },
        (error) =>
        {
            Debug.Log("success 7: " + error);
        }); 

        cache.RequestString("https://norvig.com/gib.txt",
        (text) =>
        {

        },
        (error) =>
        {
            Debug.Log("success 8: " + error);
        });
    }
}
