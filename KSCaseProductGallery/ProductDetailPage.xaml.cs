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
            DismissArea.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => this.CloseAsync()) // 'this.CloseAsync()'로 수정
            });
        }

        private void BindProduct()
        {
            ProductImage.Source = _product.LocalImagePath;
            ProductNameLabel.Text = $"Name: { _product.productName}";
            CodeNameLabel.Text = $"{_product.codeName}";
            TypeLabel.Text = $"Type: {_product.type}";
            if(string.IsNullOrEmpty(_product.capacity))
            {
                CapacityLabel.Text = "Capacity: N/A"; // 용량이 없을 경우 표시
            }
            else
            {
                CapacityLabel.Text = $"Capacity: {_product.capacity} ml";
            }
            SizeLabel.Text = $"Size: {_product.size}";
            DescriptionLabel.Text = _product.description;
            // 필요하다면 카테고리도 표시
            // CategoryLabel.Text = $"카테고리: {_product.category}";
        }

        private void OnSizeChanged(object? sender, EventArgs e)
        {
            if (Width > Height)
            {
                // Landscape
                VisualStateManager.GoToState(RootGrid, "Landscape");
                Grid.SetRow(ProductImage, 0);
                Grid.SetColumn(ProductImage, 0);
                Grid.SetRow(InfoStack, 0);
                Grid.SetColumn(InfoStack, 1);
            }
            else
            {
                // Portrait
                VisualStateManager.GoToState(RootGrid, "Portrait");
                Grid.SetRow(ProductImage, 0);
                Grid.SetColumn(ProductImage, 0);
                Grid.SetRow(InfoStack, 1);
                Grid.SetColumn(InfoStack, 0);
            }
        }
    }
}
