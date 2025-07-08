using KSCaseProductGallery.Services;
using Microsoft.Maui.Storage;

namespace KSCaseProductGallery
{
    public partial class AddProductPage : ContentPage
    {
        private FileResult? _selectedImage;

        public AddProductPage()
        {
            InitializeComponent();
            this.SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged(object? sender, EventArgs e)
        {
            if (Width > Height)
            {
                RootGrid.RowDefinitions.Clear();
                RootGrid.ColumnDefinitions.Clear();
                RootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
                RootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
                RootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

                Grid.SetRow(ImageStack, 0);
                Grid.SetColumn(ImageStack, 0);
                Grid.SetRow(InfoStack, 0);
                Grid.SetColumn(InfoStack, 1);
            }
            else
            {
                RootGrid.RowDefinitions.Clear();
                RootGrid.ColumnDefinitions.Clear();
                RootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
                RootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
                RootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

                Grid.SetRow(ImageStack, 0);
                Grid.SetColumn(ImageStack, 0);
                Grid.SetRow(InfoStack, 1);
                Grid.SetColumn(InfoStack, 0);
            }
        }

        private async void OnPickImageClicked(object sender, EventArgs e)
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "이미지 선택",
                FileTypes = FilePickerFileType.Images
            });
            if (result != null)
            {
                _selectedImage = result;
                ImageFileNameLabel.Text = result.FileName;
                // 이미지 미리보기 갱신
                ProductImagePreview.Source = ImageSource.FromFile(result.FullPath);
            }
        }

        private async void OnAddProductClicked(object sender, EventArgs e)
        {
            var product = new Product
            {
                productName = ProductNameEntry.Text,
                codeName = CodeNameEntry.Text,
                type = TypeEntry.Text,
                capacity = CapacityEntry.Text,
                size = SizeEntry.Text,
                description = DescriptionEditor.Text,
                category = CategoryEntry?.Text
            };

            await new FirebaseServices().AddProductAsync(product, _selectedImage);
            await DisplayAlert("완료", "제품이 등록되었습니다.", "확인");
            await Shell.Current.GoToAsync("..");
        }

        private async void OnCloseClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}