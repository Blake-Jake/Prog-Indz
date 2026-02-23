using Microsoft.Maui.Controls;
using EventMatch.Models;
using EventMatch.Services;

namespace EventMatch;

public partial class SignUpPage : ContentPage
{
    private readonly UserDatabase _userDb;

    public SignUpPage()
    {
        InitializeComponent();
        _userDb = Application.Current?.Handler?.MauiContext?.Services.GetService<UserDatabase>()!;
    }


    private async void OnSignInTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("//LoginPage");
    }

    private async void OnSignUpClicked(object? sender, EventArgs e)
    {
        var email = EmailEntry.Text?.Trim() ?? string.Empty;
        var password = PasswordEntry.Text ?? string.Empty;
        var confirm = ConfirmPasswordEntry.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlertAsync("Error", "Please fill all fields.", "OK");
            return;
        }
        if (password != confirm)
        {
            await DisplayAlertAsync("Error", "Passwords do not match.", "OK");
            return;
        }
        var existing = await _userDb.GetUserByEmailAsync(email);
        if (existing != null)
        {
            await DisplayAlertAsync("Error", "User already exists.", "OK");
            return;
        }
        await _userDb.AddUserAsync(new User { Email = email, Password = password });
        await DisplayAlertAsync("Success", "Account created!", "OK");
        await Shell.Current.GoToAsync("//LoginPage");
    }
}
