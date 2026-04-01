// 🗑️ Delete All Data - Kod snippetas PageProfile.xaml.cs

// Pridėti šį metodą į PageProfile.xaml.cs:

private async void OnDeleteAllDataClicked(object sender, EventArgs e)
{
    // Parodyti patvirtinimo dialogo
    bool confirm = await DisplayAlert(
        "⚠️ PATVIRTINTI", 
        "Ištrinti VISUS users ir grupes iš Cloud ir Local?\n\nŠI OPERACIJA NEGRĄŽINAMA!",
        "IŠTRINTI", 
        "ATŠAUKTI"
    );

    if (!confirm) return;

    try
    {
        // Gauti HybridGroupService iš DI
        var hybridGroupService = Application.Current?.Handler?.MauiContext?.Services.GetService<HybridGroupService>();
        
        if (hybridGroupService == null)
        {
            await DisplayAlert("❌ Klaida", "HybridGroupService nepasiekiamas", "OK");
            return;
        }

        // Rodyti loading
        var loadingAlert = await DisplayAlert("⏳ Ištrinimas...", "Prašome palaukti...", "", "");

        // Atlikti ištrinimą
        bool success = await hybridGroupService.DeleteAllDataAsync();
        
        await DisplayAlert(
            success ? "✅ SĖKMĖ" : "❌ KLAIDA",
            success 
                ? "Visi duomenys (Users ir Groups) sėkmingai ištrinti iš Cloud ir Local!" 
                : "Klaida trinant duomenis. Peržiūrėkite Debug logs.",
            "OK"
        );

        // Grąžinti į login puslapį jei sėkmė
        if (success)
        {
            // Išvalyti session
            Session.CurrentUserEmail = "";
            await Shell.Current.GoToAsync("login");
        }
    }
    catch (Exception ex)
    {
        await DisplayAlert("❌ Klaida", $"Nenumatyta klaida:\n{ex.Message}", "OK");
        System.Diagnostics.Debug.WriteLine($"[PageProfile] Delete error: {ex}");
    }
}

// ==========================================
// PageProfile.xaml - pridėti šį mygtuką:
// ==========================================

/*
<VerticalStackLayout Padding="20" Spacing="10">
    
    <!-- Kiti UI elementai... -->
    
    <!-- DELETE ALL DATA button (Red warning button) -->
    <Button 
        x:Name="DeleteAllDataButton"
        Text="🗑️ Ištrinti VISUS Users ir Groups"
        Clicked="OnDeleteAllDataClicked"
        BackgroundColor="#ff4444"
        TextColor="White"
        Margin="20,10"
        Padding="20,15"
        FontSize="14"
        FontAttributes="Bold"/>
    
</VerticalStackLayout>
*/

// ==========================================
// Alternatyvus variantas - tik local DB:
// ==========================================

private async void OnDeleteLocalDataClicked(object sender, EventArgs e)
{
    bool confirm = await DisplayAlert(
        "⚠️ PATVIRTINTI", 
        "Ištrinti TIKAI lokalios duomenų bazės duomenis?",
        "IŠTRINTI", 
        "ATŠAUKTI"
    );

    if (!confirm) return;

    try
    {
        var userDb = Application.Current?.Handler?.MauiContext?.Services.GetService<UserDatabase>();
        if (userDb == null)
        {
            await DisplayAlert("❌ Klaida", "UserDatabase nepasiekiama", "OK");
            return;
        }

        // Ištrinti tik lokalius duomenis
        await userDb.ClearAllDataAsync();

        await DisplayAlert("✅ SĖKMĖ", "Lokali duomenų bazė sušvalyta!", "OK");
    }
    catch (Exception ex)
    {
        await DisplayAlert("❌ Klaida", $"{ex.Message}", "OK");
    }
}
