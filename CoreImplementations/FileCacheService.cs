using ArqanumCore.Interfaces;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage;

namespace Arqanum.CoreImplementations
{
    public class FileCacheService(HttpClient httpClient) : IFileCacheService
    {
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        private readonly StorageFolder _cacheFolder = ApplicationData.Current.LocalCacheFolder;

        public async Task<string?> GetOrDownloadFilePathAsync(string url, string fileName)
        {
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("URL cannot be null or empty", nameof(url));
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("FileName cannot be null or empty", nameof(fileName));

            try
            {
                var file = await _cacheFolder.TryGetItemAsync(fileName) as StorageFile;
                if (file != null)
                {
                    return file.Path;
                }

                var bytes = await _httpClient.GetByteArrayAsync(url);

                file = await _cacheFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

                await FileIO.WriteBytesAsync(file, bytes);

                return file.Path;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> RefreshCacheAsync(string url, string fileName)
        {
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("URL cannot be null or empty", nameof(url));
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("FileName cannot be null or empty", nameof(fileName));

            try
            {
                var bytes = await _httpClient.GetByteArrayAsync(url);

                var file = await _cacheFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteBytesAsync(file, bytes);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteCachedFileAsync(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("FileName cannot be null or empty", nameof(fileName));

            try
            {
                var file = await _cacheFolder.TryGetItemAsync(fileName) as StorageFile;
                if (file != null)
                {
                    await file.DeleteAsync();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task ClearCacheAsync()
        {
            try
            {
                var files = await _cacheFolder.GetFilesAsync();
                foreach (var file in files)
                {
                    await file.DeleteAsync();
                }
            }
            catch
            {
                // Игнорируем ошибки очистки кеша
            }
        }

        public async Task<Stream?> GetFileStreamAsync(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("FileName cannot be null or empty", nameof(fileName));

            try
            {
                var file = await _cacheFolder.TryGetItemAsync(fileName) as StorageFile;
                if (file == null)
                    return null;

                var stream = await file.OpenStreamForReadAsync();
                return stream;
            }
            catch
            {
                return null;
            }
        }

        public async Task<long> GetCacheSizeAsync()
        {
            try
            {
                var files = await _cacheFolder.GetFilesAsync();
                long totalSize = 0;
                foreach (var file in files)
                {
                    var properties = await file.GetBasicPropertiesAsync();
                    totalSize += (long)properties.Size;
                }
                return totalSize;
            }
            catch
            {
                return 0;
            }
        }
        public string GetFileNameFromUrl(string url, string? fallbackName = null)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("URL cannot be null or empty", nameof(url));

            try
            {
                var uri = new Uri(url);
                var ext = Path.GetExtension(uri.LocalPath);
                if (string.IsNullOrEmpty(ext))
                    ext = ".img"; 

                string name = fallbackName ?? Guid.NewGuid().ToString("N");
                return name + ext;
            }
            catch
            {
                string name = fallbackName ?? Guid.NewGuid().ToString("N");
                return name + ".img";
            }
        }

    }
}
