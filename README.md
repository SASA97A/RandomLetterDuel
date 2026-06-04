# # Random Letter Duel

### 3. Starta applikationen

För att kunna spela måste både **API-projektet** och **UI-projektet** köras samtidigt.

#### Via Visual Studio 

1. Öppna solution-filen (`RandomLetterDuel.sln`) i Visual Studio.
2. Högerklicka på **Solution** längst upp i Solution Explorer och välj **Configure Startup Projects...**.
3. Välj **Multiple startup projects**.
4. Sätt *Action* till **Start** för både `RandomLetterDuel.API` och `RandomLetterDuel.UI`.
5. Tryck **Apply** och sedan **OK**.
6. Starta genom att trycka på den gröna **Start**-pilen (eller `F5`).

*UI-appen startar och lyssnar på: `https://localhost:7287*`

---

## Hur man spelar

1. Öppna en webbläsare och gå till **`https://localhost:7287`**.
2. **Spelare 1:** Skriv in ditt namn under "Starta nytt spel" och klicka på **Skapa rum**. Du presenteras nu med en unik **Rumskod**.
3. **Spelare 2:** Öppna ett nytt webbläsarfönster (gärna i Incognito/Inprivate-läge), ange ditt namn och klistra in rumskoden under "Gå med i spel", klicka sedan på **Gå med**.
4. Spelet startar automatiskt i båda fönstren så fort Spelare 2 har anslutit.

---

##  Köra Testerna

Projektet tillämpar **Behaviour-Driven Development (BDD)** och enhetstester för att verifiera affärslogik, poängberäkning och ordvalidering. Testerna ligger i testprojektet (`RandomLetterDuel.BLL.Tests` och `RandomLetterDuel.DAL.Tests`).

### Köra tester via Visual Studio

1. Gå till toppmenyn och välj **Test** -> **Test Explorer**.
2. Klicka på knappen **Run All Tests** (Bort se från UI test då det finns inga).

---
