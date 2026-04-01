# 🗑️ Iš trinti Users ir Groups - Instrukcijos

## ⚠️ DĖMESIO: Ši operacija negrąžinama!

Šios instrukcijos leidžia ištrinti **VISUS users ir grupes** iš jūsų aplikacijos (tiek iš cloud, tiek iš local duomenų bazės).

---

## 📍 Iš trinti visus Users ir Groups (Cloud + Local)

### Naudojimas kode:

```csharp
// Gauti HybridGroupService iš Dependency Injection
var hybridGroupService = ServiceHelper.Services.GetService<HybridGroupService>();

// IŠTRINIMAS - BE ATGAL!
bool success = await hybridGroupService.DeleteAllDataAsync();

if (success)
{
    Debug.WriteLine("✅ Visi duomenys sėkmingai ištrinti!");
}
else
{
    Debug.WriteLine("❌ Klaida trinant duomenis");
}
```

---

## 📍 Iš trinti tik Groups

```csharp
var hybridGroupService = ServiceHelper.Services.GetService<HybridGroupService>();

bool success = await hybridGroupService.DeleteAllGroupsAsync();
```

---

## 📍 Iš trinti tik Users

```csharp
var hybridGroupService = ServiceHelper.Services.GetService<HybridGroupService>();

bool success = await hybridGroupService.DeleteAllUsersAsync();
```

---

## 🔧 Žemesnio lygio API (jei reikalinga)

### Ištrinti local duomenis (Windows):

```csharp
var userDatabase = ServiceHelper.Services.GetService<UserDatabase>();

// Ištrinti tik users
await userDatabase.DeleteAllUsersAsync();

// Ištrinti tik grupes
await userDatabase.DeleteAllGroupsAsync();

// Ištrinti VISĄ local duomenų bazę
await userDatabase.ClearAllDataAsync();
```

### Ištrinti cloud duomenis:

```csharp
var cloudAuthService = ServiceHelper.Services.GetService<CloudAuthService>();
var cloudGroupService = ServiceHelper.Services.GetService<CloudGroupService>();

// Ištrinti visus cloud users
await cloudAuthService.DeleteAllUsersAsync();

// Ištrinti visus cloud groups
await cloudGroupService.DeleteAllGroupsAsync();
```

---

## 📊 Duomenų saugojimas

| Vieta | Users | Groups | Saugojimo vieta |
|---|---|---|---|
| **Windows** | ✅ Local SQLite | ✅ Local SQLite | `users.db3` |
| **Android** | ☁️ Cloud tik | ☁️ Cloud tik | API Backend |

---

## 🔍 Patikrinimai po ištrinimo

**Linux/Mac/Windows:**
```bash
# Pažiūrėti local DB lokacija
find ~/.local/share -name "users.db3" 2>/dev/null

# Android emulatoriuje:
adb shell "ls -la /data/data/com.eventmatch/files/"
```

---

## 📝 Prieinamos metodai

### `HybridGroupService` (Hibridinis režimas - Cloud + Local)
- `DeleteAllDataAsync()` - Ištrinti visus users ir grupes
- `DeleteAllUsersAsync()` - Ištrinti tik users
- `DeleteAllGroupsAsync()` - Ištrinti tik grupes

### `UserDatabase` (Local tik)
- `DeleteAllUsersAsync()` - Ištrinti visus local users
- `DeleteAllGroupsAsync()` - Ištrinti visas local grupes
- `ClearAllDataAsync()` - Ištrinti VISĄ DB

### `CloudAuthService` (Cloud users)
- `DeleteUserAsync(email)` - Ištrinti vieną user
- `DeleteAllUsersAsync()` - Ištrinti visus users

### `CloudGroupService` (Cloud groups)
- `DeleteGroupAsync(id)` - Ištrinti vieną grupę
- `DeleteAllGroupsAsync()` - Ištrinti visas grupes

---

## ⚡ Greitasis mygtukas (jei priėmėte PageProfile)

Jei norite pridėti mygtuką PageProfile.xaml, kad vartotojas galėtų patrinti duomenis:

```xaml
<Button 
    Text="🗑️ Clear All Data"
    Clicked="OnDeleteAllDataClicked"
    BackgroundColor="#ff4444"
    TextColor="White"
    Margin="20"/>
```

```csharp
private async void OnDeleteAllDataClicked(object sender, EventArgs e)
{
    bool confirm = await DisplayAlert("⚠️ Patvirtinti", 
        "Ištrinti VISUS users ir grupes? Tai NEGRĄŽINAMA!", 
        "Ištrinti", "Atšaukti");

    if (!confirm) return;

    try
    {
        var hybridGroupService = this.Handler?.MauiContext?.Services.GetService<HybridGroupService>();
        if (hybridGroupService != null)
        {
            bool success = await hybridGroupService.DeleteAllDataAsync();
            await DisplayAlert(
                success ? "✅ Sėkmė" : "❌ Klaida",
                success ? "Duomenys ištrinti!" : "Klaida trinant duomenis",
                "OK"
            );
        }
    }
    catch (Exception ex)
    {
        await DisplayAlert("❌ Klaida", ex.Message, "OK");
    }
}
```

---

## 🛑 Saugos pastabos

1. **Android**: Naudoja **tik Cloud** (EVENTMATCH_CLOUD_ONLY=1)
2. **Windows**: Naudoja **Cloud + Local** hibridinis režimas
3. **Grąžinti duomenis** nėra galima - šis veiksmas yra **permanentus**!

---

## 📞 Klaidų sprendimas

Jei matote klaidas:

```
[CloudAuthService] Delete all users error: 401 Unauthorized
```

**Priežastis**: API reikalinga autorizacija arba serveris negrąžina vartotojų sąrašo.

```
[UserDatabase] Error deleting local data: Database is locked
```

**Priežastis**: Kita vieta naudoja duomenų bazę. Uždarkite aplikaciją ir bandykite vėl.
