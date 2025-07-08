using System.ComponentModel;

namespace KSCaseProductGallery
{
    public class AdminState : INotifyPropertyChanged
    {
        private static AdminState? _instance;
        public static AdminState Instance => _instance ??= new AdminState();

        private bool _isAdmin;
        public bool IsAdmin
        {
            get => _isAdmin;
            private set
            {
                if (_isAdmin != value)
                {
                    _isAdmin = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsAdmin)));
                }
            }
        }

        private const string AdminPassword = "1234"; // 실제 비밀번호로 변경

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

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}