# Caching

## 1.0.0

- Initial release.

## 1.0.1

- Added singleton functionality to avoid having to keep track of references
    - Access via `DSS.Caching.Cache.Instance`

## 1.0.2

- Added more robust url checking

## 1.0.3

- Added robust checks for avoiding race conditions / duplicated downloads
    - Now only a single file is ever downloaded from the same url

## 1.0.4

- Fixed incorrectly saving `.png` files as `.bin` files

## 1.1.0

- Added `DownloadManager` class.

## 1.1.1

- Added an `IEnumerator` version of the `DownloadManager.Download` function, called `DownloadRoutine`.

## 1.1.2

- Fixed error where the saved path references would be invalid due to changing `Application.peristantDataPath` locations.

## 1.1.3

- Fixed a potential "illegal character in path" exception