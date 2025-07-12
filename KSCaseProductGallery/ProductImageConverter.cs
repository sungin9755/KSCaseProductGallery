using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using Microsoft.Maui.Controls;
using System.IO;

namespace KSCaseProductGallery
{
    public class ProductImageConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Product product)
            {
                if (!string.IsNullOrEmpty(product.LocalImagePath) && File.Exists(product.LocalImagePath))
                    return ImageSource.FromFile(product.LocalImagePath);
                if (!string.IsNullOrEmpty(product.image))
                    return ImageSource.FromUri(new Uri(product.image));
            }
            return null;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
