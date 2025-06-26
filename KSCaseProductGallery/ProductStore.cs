using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSCaseProductGallery
{
    public class ProductStore
    {
        private static ProductStore? _instance;
        public static ProductStore Instance => _instance ??= new ProductStore();
        public List<Product> Products { get; private set; } = new();

        public void SetProducts(List<Product> products)
        {
            Products = products;
        }
    }
}
