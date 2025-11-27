namespace CyberScanner.Views;

public partial class AboutPage : ContentPage
{
	public AboutPage()
	{
		InitializeComponent();
	}

    private async void LinkedIn_Tapped(object sender, TappedEventArgs e)
    {
        await Launcher.OpenAsync("https://www.linkedin.com/in/abdelhaleem-swelam/");
    }

    private async void Facebook_Tapped(object sender, TappedEventArgs e)
    {
        await Launcher.OpenAsync("https://web.facebook.com/abdelhaleem.swelam");
    }
}