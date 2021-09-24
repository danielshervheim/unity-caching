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