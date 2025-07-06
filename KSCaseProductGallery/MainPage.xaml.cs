using KSCaseProductGallery.Services;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Maui.Extensions;
using System.Windows.Input;

namespace KSCaseProductGallery
{
    public partial class MainPage : ContentPage
    {
        public ICommand ShowAdminPromptCommand { get; }

        private int _adminTapCount = 0;
        private DateTime _lastTapTime = DateTime.MinValue;

        public MainPage()
        {
            InitializeComponent();
            ShowAdminPromptCommand = new Command(async () =>
            {
                string result = await DisplayPromptAsync("관리자 로그인", "비밀번호를 입력하세요", "확인", "취소", "비밀번호", -1, Keyboard.Text);
                if (AdminState.Instance.Login(result))
                    await DisplayAlert("성공", "관리자 로그인 성공", "확인");
                else
                    await DisplayAlert("실패", "비밀번호가 틀렸습니다.", "확인");
            });
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            var firebaseService = new FirebaseServices();

            if (ConnectivityHelper.IsInternetAvailable())
            {
                await firebaseService.FetchAndCacheProductsAsync();
            }
            else
            {
                firebaseService.LoadProductsFromCache();
                await DisplayAlert("오프라인", "로컬 캐시에서 데이터를 불러옵니다.", "확인");
            }

            ProductCollection.ItemsSource = ProductStore.Instance.Products;
        }

        private void OnSizeChanged(object? sender, EventArgs e)
        {
            var span = this.Width > this.Height ? 3 : 2;
            ProductCollection.ItemsLayout = new GridItemsLayout(span, ItemsLayoutOrientation.Vertical);
        }

        private void OnProductSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Product selected)
            {
                // 팝업으로 상세 페이지 표시
                var popup = new ProductDetailPage(selected);
                this.ShowPopup(popup);
                ProductCollection.SelectedItem = null;
            }
        }

        private void OnAdminAreaTapped(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            if ((now - _lastTapTime).TotalSeconds < 1)
            {
                _adminTapCount++;
            }
            else
            {
                _adminTapCount = 1;
            }
            _lastTapTime = now;

            if (_adminTapCount >= 3)
            {
                _adminTapCount = 0;
                ShowAdminPromptCommand.Execute(null);
            }
        }
    }

}
