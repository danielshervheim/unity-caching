using System.IO;
using System.IO.Compression;
using UnityEngine;

namespace DSS.Caching
{

public class Encoding
{
    // ----------- //
    // COMPRESSION //
    // ----------- //

    // https://stackoverflow.com/questions/40909052/using-gzip-to-compress-decompress-an-array-of-bytes
    private static byte[] Compress(byte[] data)
    {
        using (var compressedStream = new MemoryStream())
        using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
        {
            zipStream.Write(data, 0, data.Length);
            zipStream.Close();
            return compressedStream.ToArray();
        }
    }

    // https://stackoverflow.com/questions/40909052/using-gzip-to-compress-decompress-an-array-of-bytes
    private static byte[] Decompress(byte[] data)
    {
        using (var compressedStream = new MemoryStream(data))
        using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
        using (var resultStream = new MemoryStream())
        {
            zipStream.CopyTo(resultStream);
            return resultStream.ToArray();
        }
    }

    // ---------------- //
    // TEXTURE ENCODING //
    // ---------------- //

    public static byte[] TextureToByteArray(Texture texture)
    {
        return ImageConversion.EncodeToPNG((Texture2D)texture);
    }

    public static Texture TextureFromByteArray(byte[] byteArray)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.LoadImage(byteArray);
        return texture;
    }

    // --------------- //
    // SPRITE ENCODING //
    // --------------- //

    public static byte[] SpriteToByteArray(Sprite sprite)
    {
        return TextureToByteArray(sprite.texture);
    }


    public static Sprite SpriteFromByteArray(byte[] byteArray)
    {
        Texture2D texture = (Texture2D)TextureFromByteArray(byteArray);
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0), 100f);
        return sprite;
    }

    // --------------- //
    // STRING ENCODING //
    // --------------- //

    public static byte[] StringToByteArray(string message)
    {
        byte[] uncompressed = System.Text.Encoding.ASCII.GetBytes(message);
        byte[] compressed = Compress(uncompressed);
        return compressed;
    }

    public static string StringFromByteArray(byte[] byteArray)
    {
        byte[] uncompressed = Decompress(byteArray);
        string message = System.Text.Encoding.ASCII.GetString(uncompressed);
        return message;
    }

    // ------------------- //
    // AUDIO CLIP ENCODING //
    // ------------------- //

    public static byte[] AudioClipToByteArray(AudioClip audioClip)
    {
        AudioClipDescription description = AudioClipToDescription(audioClip);
        string json = JsonUtility.ToJson(description);
        return StringToByteArray(json);
    }

    public static AudioClip AudioClipFromByteArray(byte[] byteArray)
    {
        string json = StringFromByteArray(byteArray);
        AudioClipDescription description = JsonUtility.FromJson<AudioClipDescription>(json);
        return AudioClipFromDescription(description);
    }

    [System.Serializable]
    private class AudioClipDescription
    {
        public string name;
        public int samples;
        public int channels;
        public int frequency;
        public bool stream;
        public float[] data;
    }

    private static AudioClipDescription AudioClipToDescription(AudioClip clip)
    {
        AudioClipDescription description = new AudioClipDescription();

        description.name = clip.name;
        description.samples = clip.samples;
        description.channels = clip.channels;
        description.frequency = clip.frequency;
        description.stream = clip.loadType == AudioClipLoadType.Streaming;
        description.data = new float[clip.samples * clip.channels];
        clip.GetData(description.data, 0);

        return description;
    }

    private static AudioClip AudioClipFromDescription(AudioClipDescription description)
    {
        AudioClip clip = AudioClip.Create(description.name, description.samples, description.channels, description.frequency, description.stream);
        clip.SetData(description.data, 0);
        return clip;
    }
}

}  // namespace DSS.Caching
