using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace DSS.Caching
{

public static class CacheUtilities
{
    // ---------- //
    // TEXTURE IO //
    // ---------- //

    public static void WriteTexture(string path, Texture texture)
    {
        byte[] byteArray = Encoding.TextureToByteArray(texture);

        FileInfo info = new FileInfo(path);
        info.Directory.Create();

        File.WriteAllBytes(path, byteArray);
    }

    public static Texture ReadTexture(string path)
    {
        byte[] byteArray = File.ReadAllBytes(path);
        return Encoding.TextureFromByteArray(byteArray);
    }

    // --------- //
    // SPRITE IO //
    // --------- //

    public static void WriteSprite(string path, Sprite sprite)
    {
        byte[] byteArray = Encoding.SpriteToByteArray(sprite);

        FileInfo info = new FileInfo(path);
        info.Directory.Create();

        File.WriteAllBytes(path, byteArray);
    }

    public static Sprite ReadSprite(string path)
    {
        byte[] byteArray = File.ReadAllBytes(path);
        return Encoding.SpriteFromByteArray(byteArray);
    }

    public static IEnumerator DownloadSprite(string url, Action<Sprite> onSuccess, Action<string> onError)
    {
        UnityWebRequest request = null;
        try
        {
            request = UnityWebRequestTexture.GetTexture(url);
        }
        catch (Exception exception)
        {
            onError(exception.Message);
            yield break;
        }

        if (request == null)
        {
            onError("Failed to make request to " + url);
            yield break;
        }
        
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success || request.error != null)
        {
            onError(request.error);    
        }
        else
        {
            try
            {
                Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero, 100f);
                onSuccess(sprite);
            }
            catch (Exception exception)
            {
                onError(exception.Message);
            }
        }
    }

    // --------- //
    // STRING IO //
    // --------- //

    public static void WriteString(string path, string message)
    {
        byte[] byteArray = Encoding.StringToByteArray(message);

        FileInfo info = new FileInfo(path);
        info.Directory.Create();

        File.WriteAllBytes(path, byteArray);
    }

    public static string ReadString(string path)
    {
        byte[] byteArray = File.ReadAllBytes(path);
        return Encoding.StringFromByteArray(byteArray);
    }

    public static IEnumerator DownloadString(string url, Action<string> onSuccess, Action<string> onError)
    {
        UnityWebRequest request = null;
        try
        {
            request = UnityWebRequest.Get(url);
        }
        catch (Exception exception)
        {
            onError(exception.Message);
            yield break;
        }

        if (request == null)
        {
            onError("Failed to make request to " + url);
            yield break;
        }

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success || request.error != null)
        {
            onError(request.error);
        }
        else
        {
            onSuccess(request.downloadHandler.text);
        }
    }

    // ------------- //
    // AUDIO CLIP IO //
    // ------------- //
    
    public static void WriteAudioClip(string path, AudioClip audioClip)
    {
        byte[] byteArray = Encoding.AudioClipToByteArray(audioClip);

        FileInfo info = new FileInfo(path);
        info.Directory.Create();

        File.WriteAllBytes(path, byteArray);
    }

    public static AudioClip ReadAudioClip(string path)
    {
        byte[] byteArray = File.ReadAllBytes(path);
        return Encoding.AudioClipFromByteArray(byteArray);
    }

    public static IEnumerator DownloadAudioClip(string url, Action<AudioClip> onSuccess, Action<string> onError)
    {
        string extension = Path.GetExtension(url);
        AudioType type = AudioTypeFromExtension(extension);

        UnityWebRequest request = null;
        try
        {
            request = UnityWebRequestMultimedia.GetAudioClip(url, type);
        }
        catch (Exception exception)
        {
            onError(exception.Message);
            yield break;
        }

        if (request == null)
        {
            onError("Failed to make request to " + url);
            yield break;
        }

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success || request.error != null)
        {
            onError(request.error);
        }
        else
        {
            
            try
            {
                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(request);
                onSuccess(audioClip);
            }
            catch (Exception exception)
            {
                onError(exception.Message);
            }
        }
    }

    private static AudioType AudioTypeFromExtension(string extension)
    {
        switch (extension)
        {
            case "ogg":
                return AudioType.OGGVORBIS;
            case "mp3":
                return AudioType.MPEG;
            case "wav":
                return AudioType.WAV;
            default:
                return AudioType.UNKNOWN;
        }
    }
}

}  // namespace DSS.Caching
