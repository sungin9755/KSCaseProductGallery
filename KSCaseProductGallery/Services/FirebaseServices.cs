using global::KSCaseProductGallery.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace KSCaseProductGallery.Services
{
    public class FirebaseServices
    {
        // Firestore REST endpoint
        private readonly string firestoreBaseUrl =
            "https://firestore.googleapis.com/v1/projects/ksproductgallery/databases/(default)/documents/products";
        // 로컬 캐시 JSON 파일 경로
        private readonly string cacheJsonPath =
            Path.Combine(FileSystem.AppDataDirectory, "product_cache.json");
        private readonly HttpClient httpClient = new HttpClient();

        /// <summary>
        /// 온라인 상태에서 Firestore 데이터를 가져와 로컬에 캐시하고 ProductStore에 반영합니다.
        /// </summary>
        public async Task FetchAndCacheProductsAsync()
        {
            try
            {
                // 1) Firestore에서 문서 목록 가져오기
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

                    // 2) 이미지 다운로드 및 로컬 경로 변경
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

                        product.image = localPath; // LocalImagePath로 사용
                    }

                    list.Add(product);
                }

                // 3) 로컬 JSON 캐시 저장
                var cacheJson = JsonSerializer.Serialize(list);
                File.WriteAllText(cacheJsonPath, cacheJson);

                // 4) ProductStore에 반영
                ProductStore.Instance.SetProducts(list);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] FetchAndCacheProductsAsync failed: {ex.Message}");
            }
        }

        /// <summary>
        /// 오프라인 상태에서 로컬 캐시를 불러와 ProductStore에 반영합니다.
        /// </summary>
        public void LoadProductsFromCache()
        {
            try
            {
                if (!File.Exists(cacheJsonPath))
                {
                    Console.WriteLine("[Info] 로컬 캐시 파일이 없습니다.");
                    return;
                }

                var json = File.ReadAllText(cacheJsonPath);
                var list = JsonSerializer.Deserialize<List<Product>>(json);

                if (list != null)
                    ProductStore.Instance.SetProducts(list);
                else
                    Console.WriteLine("[Warning] 캐시된 제품 데이터가 비어 있습니다.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] LoadProductsFromCache failed: {ex.Message}");
            }
        }
    }
}