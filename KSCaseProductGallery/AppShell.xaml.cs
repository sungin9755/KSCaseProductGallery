namespace KSCaseProductGallery
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(AddProductPage), typeof(AddProductPage));
        }

        private async void OnAdminLoginClicked(object sender, EventArgs e)
        {
            string result = await DisplayPromptAsync("관리자 로그인", "비밀번호를 입력하세요", "확인", "취소", "비밀번호", -1, Keyboard.Text);
            if (AdminState.Instance.Login(result))
            {
                await DisplayAlert("성공", "관리자 로그인 성공", "확인");
                AdminLoginMenu.IsVisible = false;
                AdminLogoutMenu.IsVisible = true;
                AddProductMenu.IsVisible = true;
            }
            else
            {
                await DisplayAlert("실패", "비밀번호가 틀렸습니다.", "확인");
            }
        }

        private void OnAdminLogoutClicked(object sender, EventArgs e)
        {
            AdminState.Instance.Logout();
            AdminLoginMenu.IsVisible = true;
            AdminLogoutMenu.IsVisible = false;
            AddProductMenu.IsVisible = false;
        }
    }
}