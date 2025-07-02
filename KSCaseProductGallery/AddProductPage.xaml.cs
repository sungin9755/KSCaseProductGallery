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
                description = DescriptionEditor.Text
            };

            await new FirebaseServices().AddProductAsync(product, _selectedImage);
            await DisplayAlert("완료", "제품이 등록되었습니다.", "확인");
            await Shell.Current.GoToAsync("..");
        }
    }
}