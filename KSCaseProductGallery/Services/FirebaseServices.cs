using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using global::KSCaseProductGallery.Models;
using Newtonsoft.Json;
using System.Net.Http;

namespace KSCaseProductGallery.Services
{
    public class FirebaseService
    {
        private readonly string baseUrl = "https://firestore.googleapis.com/v1/projects/ksproductgallery/databases/(default)/documents/products";

        public async Task AddProductAsync(Product product)
        {
            var request = new
            {
                fields = new
                {
                    name = new { stringValue = product.name },
                    year = new { stringValue = product.year },
                    material = new { stringValue = product.material },
                    feature = new { stringValue = product.feature },
                    description = new { stringValue = product.description },
                    image = new { stringValue = product.image }
                }
            };

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await new HttpClient().PostAsync(baseUrl, content);
            var result = await response.Content.ReadAsStringAsync();

            Console.WriteLine("[Firebase] Add result: " + result);
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            var json = await new HttpClient().GetStringAsync(baseUrl);
            var data = JsonConvert.DeserializeObject<FirestoreListResponse>(json);

            var list = new List<Product>();
            if (data == null || data.documents == null)
            {
                Console.WriteLine("[Error] No products found in Firestore.");
                return list;
            }
            foreach (var doc in data.documents)
            {
                var f = doc.fields;
                list.Add(new Product
                {
                    name = f.name.stringValue,
                    year = f.year.stringValue,
                    material = f.material.stringValue,
                    feature = f.feature.stringValue,
                    description = f.description.stringValue,
                    image = f.image.stringValue
                });
            }

            return list;
        }

        public async Task DeleteProductAsync(string documentId)
        {
            string deleteUrl = $"{baseUrl}/{documentId}";

            var response = await new HttpClient().DeleteAsync(deleteUrl);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[Firebase] 삭제 성공: {documentId}");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[Firebase] 삭제 실패: {documentId} / {error}");
            }
        }
    }
}