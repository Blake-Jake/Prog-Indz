# 🚀 DataScraper - TL;DR (Trumpas Aprašymas)

## **KĄ DARO DATASCRAPER?**

Importuoja renginius iš **Facebook** ir kituose šaltinių į jūsų EventMatch aplikaciją automatiškai.

---

## **KAIP NAUDOTI?**

### **Greičiausia būdas (BE Facebook)**:
1. Atidarykite aplikaciją
2. Spauskite **"Data Scraper"** meniu
3. Spauskite **"📝 Load Demo Events (Testas)"**
4. Grįžtate į **"Events Preview"** ir matysite 5 pavyzdinius renginius 🎉

### **Su Facebook**:
1. Gaukite Access Token iš https://developers.facebook.com/
2. Atidarykite **"Data Scraper"** meniu
3. Įveskite **Facebook Page ID** ir **Access Token**
4. Spauskite **"Scrape Facebook Events"**
5. Renginiai automatiškai importuojami ✅

---

## **FAILAI KURIE BUVO SUKURTI/PATAISYTI**

| Failas | Aprašymas |
|--------|-----------|
| `DataScraperService.cs` | Facebook API integracija |
| `DemoEventService.cs` | Demo renginiai testuoti |
| `DataScraperPage.xaml` | UI forma |
| `DataScraperPage.xaml.cs` | Importavimo logika |
| `AppShell.xaml` | Pridėtas "Data Scraper" meniu |
| `Tag.cs` | Žymų modelis |
| `Event.cs` | Atnaujintas su Tags |
| `FACEBOOK_SETUP.md` | Detali Facebook instrukcija |
| `DATASCRAPER_USAGE.md` | Naudojimo vadovas |

---

## **TESTAVIMAS**

```bash
cd "C:\Users\PC\Documents\GitHub\Prog.-Indz"
dotnet build
dotnet run
```

Tada:
- Atidarykite **"Data Scraper"** 
- Spauskite **"Load Demo Events"** button
- Pamatysite 5 Lietuvos renginius 🇱🇹

---

## **GIT COMMIT**

```bash
git add .
git commit -m "Add Facebook DataScraper and Tags functionality (SCRUM-22, SCRUM-14)"
git push
```

---

## **KĄ TOLIAU?**

- ✅ Tags funkcionalumas - **BAIGTA**
- ✅ DataScraper - **BAIGTA** 
- 🔄 Galima pridėti: Eventbrite, Ticketmaster, Google Calendar...

**Sėkmės!** 🎉
