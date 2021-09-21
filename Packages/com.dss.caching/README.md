# caching

An image, string, and audio clip caching system for Unity.

The first time you "request" a url from the cache, it downloads it and stores a copy on-disc (in `Application.persistentDataPath` and in-memory). Future "requests" use the in-memory version (if it has already been loaded), or the on-disc version (which then gets stored in-memory for faster access in the future).

## How To Install

The caching package uses the [scoped registry](https://docs.unity3d.com/Manual/upm-scoped.html) feature to import
dependent packages. Please add the following sections to the package manifest
file (`Packages/manifest.json`).

To the `scopedRegistries` section:

```
{
    "name": "DSS",
    "url": "https://registry.npmjs.com",
    "scopes": [ "com.dss" ]
}
```

To the `dependencies` section:

```
"com.dss.core-utils": "1.6.4",
"com.dss.caching": "1.0.0"
```

After changes, the manifest file should look like below:

```
{
    "scopedRegistries": [
    {
        "name": "DSS",
        "url": "https://registry.npmjs.com",
        "scopes": [ "com.dss" ]
    }
    ],
    "dependencies": {
        "com.dss.core-utils": "1.6.4",
        "com.dss.caching": "1.0.0"
        ...
```

## Usage

Add a `Cache` component to any GameObject in the scene, and then use it through script like the following:

```csharp

// NOTE: assumes `cache` is a reference to the added `Cache` component

// Request a sprite from the given url
cache.RequestSprite("https://github.githubassets.com/images/modules/logos_page/GitHub-Mark.png",
(Sprite sprite) =>
{
    // Do something with the sprite once it loads
},
(string errorMessage) =>
{
    Debug.LogError(errorMessage);
});

// Request text content from the given url
cache.RequestString("https://raw.githubusercontent.com/github/gitignore/master/Unity.gitignore",
(string text) =>
{
    Debug.Log(text);
},
(string errorMessage) =>
{
    Debug.LogError(errorMessage);
});

// Request an audio clip from the given url
cache.RequestAudioClip("https://file-examples-com.github.io/uploads/2017/11/file_example_MP3_700KB.mp3",
(AudioClip clip) =>
{
    // Do something with the clip once it loads
},
(string errorMessage) =>
{
    Debug.LogError(errorMessage);
});


```