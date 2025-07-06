using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;

namespace KSCaseProductGallery
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit() // 이 줄이 반드시 필요합니다.
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("NanumBarunGothic.ttf", "NanumGothic");
                    fonts.AddFont("NanumBarunGothicBold.ttf", "NanumGothicBold");
                    fonts.AddFont("NanumBarunGothicLight.ttf", "NanumGothicExtraBold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
