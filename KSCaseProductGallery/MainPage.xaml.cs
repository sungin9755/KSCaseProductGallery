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

        // 탭 선택에 따라 제품 목록 필터링 예시
        private List<string> _categories = new List<string>(); // 시트(카테고리) 목록
        private string _selectedCategory = "";

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

            // 카테고리 목록 추출 및 UI 갱신
            _categories = ProductStore.Instance.Products
                .Select(p => p.category)
                .Where(c => !string.IsNullOrEmpty(c))
                .Select(c => c!) // null 아님을 명시
                .Distinct()
                .ToList();

            if (_categories.Count > 0)
                _selectedCategory = _categories[0];

            UpdateProductList();
            UpdateCategoryTabs();

            Console.WriteLine($"[DEBUG] 제품 개수: {ProductStore.Instance.Products.Count}");
            foreach (var p in ProductStore.Instance.Products)
                Console.WriteLine($"[DEBUG] {p.productName} / {p.category}");

            Console.WriteLine($"[DEBUG] 카테고리 개수: {_categories.Count}");
            foreach (var c in _categories)
                Console.WriteLine($"[DEBUG] 카테고리: {c}");
        }

        private void UpdateProductList()
        {
            ProductCollection.ItemsSource = ProductStore.Instance.Products
                .Where(p => p.category == _selectedCategory)
                .OrderBy(p => p.codeName) // 코드명 기준 정렬
                .ToList();
        }

        private void UpdateCategoryTabs()
        {
            CategoryTabBar.Children.Clear();
            foreach (var category in _categories)
            {
                var isSelected = category == _selectedCategory;
                var btn = new Button
                {
                    Text = category,
                    BackgroundColor = isSelected ? Colors.Gray : Colors.Black,
                    TextColor = Colors.White,
                    Padding = new Thickness(12, 4),
                    CornerRadius = 12
                };
                btn.Clicked += (s, e) =>
                {
                    _selectedCategory = category;
                    UpdateCategoryTabs();
                    UpdateProductList();
                };
                CategoryTabBar.Children.Add(btn);
            }
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
                ProductCollection.SelectedItem = null;

                if (AdminState.Instance.IsAdmin)
                {
                    // 관리자: 수정 페이지로 이동
                    Shell.Current.GoToAsync(nameof(EditProductPage), true, new Dictionary<string, object>
                    {
                        { "product", selected }
                    });
                }
                else
                {
                    // 일반 사용자: 상세 팝업
                    var popup = new ProductDetailPage(selected);
                    this.ShowPopup(popup);
                }
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

            if (_adminTapCount >= 7)
            {
                _adminTapCount = 0;
                ShowAdminPromptCommand.Execute(null);
            }
        }

        private void OnAddProductClicked(object sender, EventArgs e)
        {
            // 기존 팝업 대신 페이지로 이동
            Shell.Current.GoToAsync(nameof(AddProductPage));
        }

        private void OnLogoutClicked(object sender, EventArgs e)
        {
            AdminState.Instance.Logout();
            DisplayAlert("로그아웃", "관리자에서 로그아웃되었습니다.", "확인");
            // 버튼 상태 갱신을 위해 BindingContext를 갱신
            OnPropertyChanged(nameof(AdminState.Instance.IsAdmin));
        }
    }
}
