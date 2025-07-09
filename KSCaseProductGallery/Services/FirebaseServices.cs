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

namespace KSCaseProductGallery.Services
{
    public class FirebaseServices
    {
        private readonly string firestoreBaseUrl =
            "https://firestore.googleapis.com/v1/projects/ksproductgallery/databases/(default)/documents/products";
        private readonly string storageBucket =
            "ksproductgallery.firebasestorage.app";

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
                Console.WriteLine(json);
                using var doc = JsonDocument.Parse(json);

                var list = new List<Product>();
                foreach (var element in doc.RootElement.GetProperty("documents").EnumerateArray())
                {
                    var fields = element.GetProperty("fields");
                    var product = new Product
                    {
                        id = element.GetProperty("name").GetString()?.Split('/')[^1],
                        productName = fields.GetProperty("productName").GetProperty("stringValue").GetString(),
                        codeName = fields.GetProperty("codeName").GetProperty("stringValue").GetString(),
                        type = fields.GetProperty("type").GetProperty("stringValue").GetString(),
                        capacity = fields.GetProperty("capacity").GetProperty("stringValue").GetString(),
                        size = fields.GetProperty("size").GetProperty("stringValue").GetString(),
                        description = fields.GetProperty("description").GetProperty("stringValue").GetString(),
                        image = fields.GetProperty("image").GetProperty("stringValue").GetString(),
                        category = fields.GetProperty("category").GetProperty("stringValue").GetString() // ★ 추가
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
                Console.WriteLine($"[Upload] 파일명: {photo.FileName}, 경로: {photo.FullPath}");
                using var stream = await photo.OpenReadAsync();
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

                string uploadUrl = $"https://firebasestorage.googleapis.com/v0/b/{storageBucket}/o?uploadType=media&name=img/{photo.FileName}";
                Console.WriteLine($"[Upload] 업로드 URL: {uploadUrl}");
                var response = await httpClient.PostAsync(uploadUrl, fileContent);
                Console.WriteLine($"[Upload] 응답 코드: {response.StatusCode}");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[Upload] 응답 본문: {json}");
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
            {
                imageUrl = await UploadImageAsync(photo);
                if (string.IsNullOrEmpty(imageUrl))
                {
                    // 업로드 실패 시 Firestore 저장 중단 및 사용자 알림
                    await Application.Current.MainPage.DisplayAlert("오류", "이미지 업로드에 실패했습니다.", "확인");
                    return;
                }
            }

            var request = new
            {
                fields = new
                {
                    productName = new { stringValue = product.productName ?? "" },
                    codeName = new { stringValue = product.codeName ?? "" },
                    type = new { stringValue = product.type ?? "" },
                    capacity = new { stringValue = product.capacity ?? "" },
                    size = new { stringValue = product.size ?? "" },
                    description = new { stringValue = product.description ?? "" },
                    image = new { stringValue = imageUrl ?? "" },
                    category = new { stringValue = product.category ?? "" }
                }
            };

            var json = JsonSerializer.Serialize(request);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(firestoreBaseUrl, content);
            response.EnsureSuccessStatusCode();
            Console.WriteLine("[Firebase] AddProductAsync 성공");
        }

        /// <summary>
        /// Firestore의 제품 문서와 Firebase Storage의 이미지를 함께 삭제
        /// </summary>
        public async Task DeleteProductAsync(Product product)
        {
            try
            {
                // 1. Firestore 문서 삭제
                if (string.IsNullOrWhiteSpace(product.id))
                    throw new ArgumentException("제품 ID가 없습니다.");

                string docUrl = $"{firestoreBaseUrl}/{product.id}";
                var firestoreResponse = await httpClient.DeleteAsync(docUrl);
                firestoreResponse.EnsureSuccessStatusCode();

                // 2. Firebase Storage 이미지 삭제
                if (!string.IsNullOrWhiteSpace(product.image))
                {
                    // 업로드 시 name=img/{파일명}으로 저장했으므로, 파일명만 추출
                    var fileName = Path.GetFileName(product.image);
                    string storageUrl = $"https://firebasestorage.googleapis.com/v0/b/{storageBucket}/o/img%2F{Uri.EscapeDataString(fileName)}";
                    var storageRequest = new HttpRequestMessage(HttpMethod.Delete, storageUrl);
                    var storageResponse = await httpClient.SendAsync(storageRequest);
                    // Storage 삭제는 권한 설정에 따라 실패할 수 있으니, 실패해도 앱이 중단되지 않게 처리
                    if (!storageResponse.IsSuccessStatusCode)
                        Console.WriteLine($"[Warning] 이미지 파일 삭제 실패: {storageResponse.StatusCode}");
                }

                // 3. ProductStore에서 삭제
                var list = ProductStore.Instance.Products;
                var toRemove = list.FirstOrDefault(p => p.id == product.id);
                if (toRemove != null)
                {
                    list.Remove(toRemove);
                    // 캐시 갱신
                    var cacheJson = JsonSerializer.Serialize(list);
                    File.WriteAllText(cacheJsonPath, cacheJson);
                }

                Console.WriteLine("[Firebase] DeleteProductAsync 성공");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] DeleteProductAsync: {ex.Message}");
            }
        }
    }
}