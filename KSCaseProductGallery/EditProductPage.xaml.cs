using KSCaseProductGallery.Services;
using Microsoft.Maui.Storage;

namespace KSCaseProductGallery;

[QueryProperty(nameof(Product), "product")]
public partial class EditProductPage : ContentPage
{
    private Product? _product;
    private FileResult? _selectedImage;
    private bool _isImagePicked = false; // 이미지가 새로 선택되었는지 여부
    private string? _originalImageUrl; // Firestore에 저장된 원래의 이미지 URL

    public Product? Product
    {
        get => _product;
        set
        {
            _product = value;
            if (_product != null && _isLoaded)
                LoadProduct();
        }
    }

    private bool _isLoaded = false;

    public EditProductPage()
    {
        InitializeComponent();
        BindingContext = this;
        _isLoaded = true;
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

        if (!string.IsNullOrEmpty(_product.image) && _product.image.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            // Firestore의 원본 이미지 URL
            ProductImagePreview.Source = _product.image;
            ImageFileNameLabel.Text = System.IO.Path.GetFileName(_product.image);
        }
        else if (!string.IsNullOrEmpty(_product.LocalImagePath) && File.Exists(_product.LocalImagePath))
        {
            // 로컬에 다운로드된 이미지
            ProductImagePreview.Source = _product.LocalImagePath;
            ImageFileNameLabel.Text = System.IO.Path.GetFileName(_product.LocalImagePath);
        }
        else
        {
            ProductImagePreview.Source = null;
            ImageFileNameLabel.Text = "";
        }

        _originalImageUrl = _product.image; // Firestore에 저장된 원래의 이미지 URL 저장
        _selectedImage = null; // 제품 로드시 초기화
        _isImagePicked = false; // 플래그도 초기화
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
            _isImagePicked = true; // 실제로 유저가 이미지를 선택한 경우만 true
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

        // 이미지 경로 처리
        if (!_isImagePicked)
        {
            // 유저가 이미지를 새로 선택하지 않았다면, Firestore에 저장할 값은 원래의 image URL만 유지
            _product.image = _originalImageUrl;
        }

        await new FirebaseServices().UpdateProductAsync(_product, _isImagePicked ? _selectedImage : null);
        _selectedImage = null;
        _isImagePicked = false;
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