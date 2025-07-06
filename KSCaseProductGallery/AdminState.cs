namespace KSCaseProductGallery
{
    public class AdminState
    {
        private static AdminState? _instance;
        public static AdminState Instance => _instance ??= new AdminState();

        public bool IsAdmin { get; private set; }

        private const string AdminPassword = "1234"; // 원하는 비밀번호로 변경

        public bool Login(string password)
        {
            if (password == AdminPassword)
            {
                IsAdmin = true;
                return true;
            }
            return false;
        }

        public void Logout()
        {
            IsAdmin = false;
        }
    }
}