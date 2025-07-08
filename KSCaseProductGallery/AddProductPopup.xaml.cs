using KSCaseProductGallery.Services;
using Microsoft.Maui.Storage;
using CommunityToolkit.Maui.Views;

namespace KSCaseProductGallery
{
    public partial class AddProductPopup : Popup
    {
        private FileResult? _selectedImage;

        // 커스텀 알림을 위한 필드 추가
        private Label? _alertLabel;

        public AddProductPopup()
        {
            InitializeComponent();

            var displayInfo = DeviceDisplay.MainDisplayInfo;
            double width = displayInfo.Width / displayInfo.Density;
            double height = displayInfo.Height / displayInfo.Density;

            this.WidthRequest = width * 0.8;
            this.HeightRequest = height * 0.8;

            if (width > height)
                SetHorizontalLayout();
            else
                SetVerticalLayout();

            this.SizeChanged += OnSizeChanged;

            // 커스텀 알림 Label만 코드에서 추가
            _alertLabel = new Label
            {
                Text = "",
                TextColor = Colors.White,
                BackgroundColor = Colors.Black,
                Padding = new Thickness(16),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                IsVisible = false,
                ZIndex = 100
            };
            RootGrid.Children.Add(_alertLabel);
        }

        private void OnSizeChanged(object? sender, EventArgs e)
        {
            if (Width > Height)
            {
                SetHorizontalLayout();
            }
            else
            {
                SetVerticalLayout();
            }
        }

        // 레이아웃을 가로로 설정
        private void SetHorizontalLayout()
        {
            RootGrid.RowDefinitions.Clear();
            RootGrid.ColumnDefinitions.Clear();
            RootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
            RootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
            RootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

            Grid.SetRow(ImageStack, 0);
            Grid.SetColumn(ImageStack, 0);
            Grid.SetRow(InfoStack.Parent as View, 0);
            Grid.SetColumn(InfoStack.Parent as View, 1);
        }

        // 레이아웃을 세로로 설정
        private void SetVerticalLayout()
        {
            RootGrid.RowDefinitions.Clear();
            RootGrid.ColumnDefinitions.Clear();
            RootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            RootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
            RootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

            Grid.SetRow(ImageStack, 0);
            Grid.SetColumn(ImageStack, 0);
            Grid.SetRow(InfoStack.Parent as View, 1);
            Grid.SetColumn(InfoStack.Parent as View, 0);
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

        // 커스텀 알림 메서드
        private async Task ShowCustomAlert(string message)
        {
            if (_alertLabel == null) return;
            _alertLabel.Text = message;
            _alertLabel.IsVisible = true;
            await Task.Delay(1500); // 1.5초 표시
            _alertLabel.IsVisible = false;
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
                category = CategoryEntry?.Text // 카테고리 입력값 바인딩
            };

            await new FirebaseServices().AddProductAsync(product, _selectedImage);

            await ShowCustomAlert("제품이 등록되었습니다.");
            await this.CloseAsync();
        }
    }
}