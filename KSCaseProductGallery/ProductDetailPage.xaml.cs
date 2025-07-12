using CommunityToolkit.Maui.Views;
using System;
using Microsoft.Maui.Controls;

namespace KSCaseProductGallery
{
    public partial class ProductDetailPage : Popup
    {
        private Product _product;

        public ProductDetailPage(Product product)
        {
            InitializeComponent();
            _product = product;
            BindProduct();
            this.SizeChanged += OnSizeChanged;

            // BoxView(투명영역) 클릭 시 팝업 닫기
            //DismissArea.GestureRecognizers.Add(new TapGestureRecognizer
            //{
            //    Command = new Command(() => this.CloseAsync())
            //});
        }

        private void BindProduct()
        {
            if (!string.IsNullOrEmpty(_product.LocalImagePath) && File.Exists(_product.LocalImagePath))
                ProductImage.Source = _product.LocalImagePath;
            else if (!string.IsNullOrEmpty(_product.image))
                ProductImage.Source = _product.image;
            else
                ProductImage.Source = null;

            ProductNameLabel.Text = $"Name: { _product.productName}";
            CodeNameLabel.Text = $"{_product.codeName}";
            TypeLabel.Text = $"Type: {_product.type}";
            CapacityLabel.Text = string.IsNullOrEmpty(_product.capacity)
                ? "Capacity: N/A"
                : $"Capacity: {_product.capacity} ml";
            SizeLabel.Text = $"Size: {_product.size}";
            DescriptionLabel.Text = _product.description;
        }

        private void OnSizeChanged(object? sender, EventArgs e)
        {
            // 가로가 세로보다 크면 Landscape, 아니면 Portrait
            var orientation = this.Width > this.Height ? "Landscape" : "Portrait";
            VisualStateManager.GoToState(ContentGrid, orientation);
        }
    }
}
