namespace KSCaseProductGallery
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Get raw data from the server
            GetRawData();
            SizeChanged += OnSizeChanged;
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
                // TODO: 상세 페이지 이동 또는 우측 정보 표시
                Console.WriteLine($"선택된 제품: {selected.name}");
            }
        }

        /*
        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }
        */

        private async void GetRawData()
        {
            Console.WriteLine("[Info] Entered GetRawData()");

            try
            {
                if (ConnectivityHelper.IsInternetAvailable())
                {
                    var url = "https://raw.githubusercontent.com/sungin9755/cosmetic_catalog/main/product.json";
                    await new ProductDownloader().DownloadProductImagesAsync(url);
                }
                else
                {
                    Console.WriteLine("[Error] 인터넷 연결이 없습니다.");
                    return;
                }

                var products = ProductStore.Instance.Products;

                if (products != null && products.Count > 0)
                {
                    ProductCollection.ItemsSource = ProductStore.Instance.Products;
                }
                else
                {
                    Console.WriteLine("[Error] 제품 데이터가 비어 있습니다.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] 제품 데이터 다운로드 중 오류 발생: {ex.Message}");
                await DisplayAlert("오류", "제품 데이터를 불러오는 중 오류가 발생했습니다.", "확인");
            }
        }
    }

}
