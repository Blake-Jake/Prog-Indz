using Microsoft.Maui.Controls;
using EventMatch.Services;

namespace EventMatch;

public partial class LoginPage : ContentPage
{
    private readonly UserDatabase _userDb;

    public LoginPage()
    {
        InitializeComponent();
        _userDb = Application.Current?.Handler?.MauiContext?.Services.GetService<UserDatabase>()!;
    }


    private async void OnSignUpTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("//SignUpPage");
    }

    private async void OnSignInClicked(object? sender, EventArgs e)
    {
        var email = EmailEntry.Text?.Trim() ?? string.Empty;
        var password = PasswordEntry.Text ?? string.Empty;

        var user = await _userDb.GetUserAsync(email, password);
        if (user != null)
        {
            await DisplayAlertAsync("Success", "Login successful!", "OK");
            await Shell.Current.GoToAsync("//MainPage");
        }
        else
        {
            await DisplayAlertAsync("Error", "Invalid email or password.", "OK");
        }
    }
}
