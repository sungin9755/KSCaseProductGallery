using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;

namespace KSCaseProductGallery
{
    public class Product
    {
        public string? id { get; set; }
        public string? productName { get; set; }
        public string? codeName { get; set; }
        public string? type { get; set; }
        public string? capacity { get; set; } // ex: "8 ml"
        public string? size { get; set; }     // ex: "75.0*18.8" (추천: dimensions, dimension, sizeInfo 등도 가능)
        public string? description { get; set; }
        public string? image { get; set; }
        public string? category { get; set; } // 시트(카테고리)명 추가

        public string LocalImagePath =>
            Path.Combine(FileSystem.AppDataDirectory, "img", Path.GetFileName(image ?? string.Empty));
    }
}
