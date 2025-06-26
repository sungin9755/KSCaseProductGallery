using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KSCaseProductGallery
{
    public class Product
    {
        public string? id { get; set; }
        public string? name { get; set; }
        public string? year { get; set; }
        public string? material { get; set; }
        public string? feature { get; set; }
        public string? description { get; set; }
        public string? image { get; set; }

        public string LocalImagePath =>
            Path.Combine(FileSystem.AppDataDirectory, "img", Path.GetFileName(image ?? string.Empty));
    }

    //public class ProductDownloader
    //{
    //    //private static readonly HttpClient httpClient = new HttpClient();

    //    //public async Task DownloadProductImagesAsync(string jsonUrl)
    //    //{
    //    //    Console.WriteLine("[Info] 제품 데이터 다운로드 시작...");
    //    //    var json = await httpClient.GetStringAsync(jsonUrl);

    //    //    var jsonPath = Path.Combine(FileSystem.AppDataDirectory, "product.json");
    //    //    File.WriteAllText(jsonPath, json);

    //    //    var products = JsonSerializer.Deserialize<List<Product>>(json);

    //    //    if (products == null)
    //    //    {
    //    //        Console.WriteLine("[Error] 제품 데이터가 비어 있습니다.");
    //    //        return;
    //    //    }

    //    //    foreach (var product in products)
    //    //    {
    //    //        if (!string.IsNullOrWhiteSpace(product.image))
    //    //        {
    //    //            Console.WriteLine($"[Info] 제품 이미지 다운로드: {product.name} ({product.image})");
    //    //            var imageUrl = $"https://raw.githubusercontent.com/sungin9755/cosmetic_catalog/main/{product.image}";
    //    //            var imageFileName = Path.GetFileName(product.image); // "001.jpg"
    //    //            var imgPath = Path.Combine(FileSystem.AppDataDirectory, "img");
    //    //            Directory.CreateDirectory(imgPath);
    //    //            var localPath = Path.Combine(imgPath, imageFileName);

    //    //            if (File.Exists(localPath))
    //    //                continue;

    //    //            try
    //    //            {
    //    //                var imageData = await httpClient.GetByteArrayAsync(imageUrl);
    //    //                File.WriteAllBytes(localPath, imageData);

    //    //            }
    //    //            catch (Exception ex)
    //    //            {
    //    //                // 네트워크 오류 등 처리
    //    //                Console.WriteLine($"[Error] 이미지 다운로드 실패: {imageUrl} / {ex.Message}");
    //    //            }
    //    //        }
    //    //    }

    //    //    ProductStore.Instance.SetProducts(products);
    //    //}
    //}
}
