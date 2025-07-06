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
            ProductNameLabel.Text = _product.productName;
            CodeNameLabel.Text = $"코드명: {_product.codeName}";
            TypeLabel.Text = $"타입: {_product.type}";
            CapacityLabel.Text = $"용량: {_product.capacity}";
            SizeLabel.Text = $"사이즈: {_product.size}";
            DescriptionLabel.Text = _product.description;
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
