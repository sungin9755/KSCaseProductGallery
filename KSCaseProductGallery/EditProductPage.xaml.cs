using KSCaseProductGallery.Services;
using Microsoft.Maui.Storage;

namespace KSCaseProductGallery;

[QueryProperty(nameof(Product), "product")]
public partial class EditProductPage : ContentPage
{
    private Product? _product;
    private FileResult? _selectedImage;

    public Product? Product
    {
        get => _product;
        set
        {
            _product = value;
            if (_product != null && IsLoaded)
                LoadProduct();
        }
    }

    private bool IsLoaded = false;

    public EditProductPage()
    {
        InitializeComponent();
        BindingContext = this;
        IsLoaded = true;
        if (_product != null)
            LoadProduct();
    }

    private void LoadProduct()
    {
        if (_product == null) return;

        ProductNameEntry.Text = _product.productName;
        CodeNameEntry.Text = _product.codeName;
        TypeEntry.Text = _product.type;
        CapacityEntry.Text = _product.capacity;
        SizeEntry.Text = _product.size;
        CategoryEntry.Text = _product.category;
        DescriptionEditor.Text = _product.description;

        if (!string.IsNullOrEmpty(_product.LocalImagePath))
        {
            ProductImagePreview.Source = _product.LocalImagePath;
            ImageFileNameLabel.Text = System.IO.Path.GetFileName(_product.LocalImagePath);
        }
        else if (!string.IsNullOrEmpty(_product.image))
        {
            ProductImagePreview.Source = _product.image;
            ImageFileNameLabel.Text = System.IO.Path.GetFileName(_product.image);
        }
        else
        {
            ProductImagePreview.Source = null;
            ImageFileNameLabel.Text = "";
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
            ProductImagePreview.Source = ImageSource.FromFile(result.FullPath);
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (_product == null) return;

        _product.productName = ProductNameEntry.Text;
        _product.codeName = CodeNameEntry.Text;
        _product.type = TypeEntry.Text;
        _product.capacity = CapacityEntry.Text;
        _product.size = SizeEntry.Text;
        _product.category = CategoryEntry.Text;
        _product.description = DescriptionEditor.Text;

        await new FirebaseServices().UpdateProductAsync(_product, _selectedImage);
        await DisplayAlert("완료", "제품 정보가 수정되었습니다.", "확인");
        await Shell.Current.GoToAsync("..");
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (_product == null) return;

        bool first = await DisplayAlert("제품 삭제", "정말 이 제품을 삭제하시겠습니까?", "네", "아니오");
        if (!first) return;

        bool second = await DisplayAlert("최종 확인", "삭제된 제품은 복구할 수 없습니다. 정말 삭제할까요?", "삭제", "취소");
        if (!second) return;

        await new FirebaseServices().DeleteProductAsync(_product);
        await DisplayAlert("삭제 완료", "제품이 삭제되었습니다.", "확인");
        await Shell.Current.GoToAsync("..");
    }
}