using global::KSCaseProductGallery.Models;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


//
namespace KSCaseProductGallery.Services
{
    public class FirebaseServices
    {
        private readonly string firestoreBaseUrl =
            "https://firestore.googleapis.com/v1/projects/ksproductgallery/databases/(default)/documents/products";
        private readonly string storageBucket =
            "YOUR_BUCKET_NAME.appspot.com";

        private readonly string cacheJsonPath =
            Path.Combine(FileSystem.AppDataDirectory, "product_cache.json");

        private readonly HttpClient httpClient = new HttpClient();

        /// <summary>
        /// 온라인 상태에서 Firestore 데이터를 가져와 로컬에 캐시하고 ProductStore에 반영
        /// </summary>
        public async Task FetchAndCacheProductsAsync()
        {
            try
            {
                var json = await httpClient.GetStringAsync(firestoreBaseUrl);
                using var doc = JsonDocument.Parse(json);

                var list = new List<Product>();
                foreach (var element in doc.RootElement.GetProperty("documents").EnumerateArray())
                {
                    var fields = element.GetProperty("fields");
                    var product = new Product
                    {
                        id = element.GetProperty("name").GetString()?.Split('/')[^1],
                        name = fields.GetProperty("name").GetProperty("stringValue").GetString(),
                        year = fields.GetProperty("year").GetProperty("stringValue").GetString(),
                        material = fields.GetProperty("material").GetProperty("stringValue").GetString(),
                        feature = fields.GetProperty("feature").GetProperty("stringValue").GetString(),
                        description = fields.GetProperty("description").GetProperty("stringValue").GetString(),
                        image = fields.GetProperty("image").GetProperty("stringValue").GetString()
                    };

                    // 이미지 로컬 다운로드
                    if (!string.IsNullOrWhiteSpace(product.image))
                    {
                        var uri = new Uri(product.image);
                        var fileName = Path.GetFileName(uri.LocalPath);
                        var imgDir = Path.Combine(FileSystem.AppDataDirectory, "img");
                        Directory.CreateDirectory(imgDir);
                        var localPath = Path.Combine(imgDir, fileName);

                        if (!File.Exists(localPath))
                        {
                            var imgData = await httpClient.GetByteArrayAsync(product.image);
                            File.WriteAllBytes(localPath, imgData);
                        }
                        product.image = localPath;
                    }

                    list.Add(product);
                }

                // 캐시 저장
                var cacheJson = JsonSerializer.Serialize(list);
                File.WriteAllText(cacheJsonPath, cacheJson);
                ProductStore.Instance.SetProducts(list);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] FetchAndCacheProductsAsync: {ex.Message}");
            }
        }

        /// <summary>
        /// 오프라인 시 로컬 캐시에서 데이터를 로드
        /// </summary>
        public void LoadProductsFromCache()
        {
            try
            {
                if (!File.Exists(cacheJsonPath))
                {
                    Console.WriteLine("[Info] 로컬 캐시 없음");
                    return;
                }
                var json = File.ReadAllText(cacheJsonPath);
                var list = JsonSerializer.Deserialize<List<Product>>(json);
                if (list != null)
                    ProductStore.Instance.SetProducts(list);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] LoadProductsFromCache: {ex.Message}");
            }
        }

        /// <summary>
        /// Firebase Storage에 이미지 업로드 후 다운로드 URL 반환
        /// </summary>
        public async Task<string?> UploadImageAsync(FileResult photo)
        {
            try
            {
                using var stream = await photo.OpenReadAsync();
                using var content = new MultipartFormDataContent();
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                content.Add(fileContent, "file", photo.FileName);

                string uploadUrl = $"https://firebasestorage.googleapis.com/v0/b/{storageBucket}/o?uploadType=media&name=img/{photo.FileName}";
                var response = await httpClient.PostAsync(uploadUrl, content);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var token = doc.RootElement.GetProperty("downloadTokens").GetString();
                return $"https://firebasestorage.googleapis.com/v0/b/{storageBucket}/o/img%2F{photo.FileName}?alt=media&token={token}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] UploadImageAsync: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 신규 제품 추가: 이미지 업로드 후 Firestore에 문서 생성
        /// </summary>
        public async Task AddProductAsync(Product product, FileResult? photo)
        {
            string? imageUrl = null;
            if (photo != null)
                imageUrl = await UploadImageAsync(photo);

            var request = new
            {
                fields = new
                {
                    name = new { stringValue = product.name },
                    year = new { stringValue = product.year },
                    material = new { stringValue = product.material },
                    feature = new { stringValue = product.feature },
                    description = new { stringValue = product.description },
                    image = new { stringValue = imageUrl }
                }
            };

            var json = JsonSerializer.Serialize(request);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(firestoreBaseUrl, content);
            response.EnsureSuccessStatusCode();
            Console.WriteLine("[Firebase] AddProductAsync 성공");
        }
    }
}