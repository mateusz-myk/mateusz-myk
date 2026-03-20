# Manager Plików - Dokumentacja 2.0

## 📋 Opis
Nowy moduł "Manager plików" umożliwia organizowanie folderów, notatek i strzałek w sposób podobny do pulpitu komputera z rozszerzonymi możliwościami wizualizacji.

## 🚀 Jak uruchomić
1. W głównym menu aplikacji kliknij przycisk **"Manager plików"**
2. Otworzy się nowy widok z pustym obszarem roboczym 2000x1500 pikseli

## ✨ Funkcje

### 📁 **Foldery**

#### Tworzenie
- Kliknij przycisk **"Nowy folder"** w górnym pasku
- Folder pojawi się w lewym górnym rogu (pozycja 20,20)
- Domyślne ustawienia: 120x100 px, kolor niebieski, ikona 📁

#### Operacje
- **Przesuwanie**: Złap folder myszką i przeciągnij w wybrane miejsce (drag & drop)
- **Otwieranie**: Dwukrotnie kliknij folder LUB prawy przycisk myszy → Otwórz
- **Edycja**: Prawy przycisk myszy → Edytuj
  - ✏️ Zmiana nazwy
  - 🎨 Zmiana koloru (40+ kolorów do wyboru)
  - 🖼️ **NOWOŚĆ: Zmiana tła na obraz z dysku** (PNG, JPG, BMP, GIF)
  - 😀 Zmiana ikonki (emoji, np. 📚 🎬 🎓 🏫 💼 📂)
  - 📏 Zmiana rozmiaru (min. 50x50 px)
- **Usuwanie**: Prawy przycisk myszy → Usuń (z potwierdzeniem)

#### Tło obrazkowe
- W oknie edycji folderu kliknij **"Wybierz obraz"**
- Wybierz plik obrazu z dysku (obsługiwane: PNG, JPG, JPEG, BMP, GIF)
- Obraz zostanie dopasowany jako tło folderu (Stretch: UniformToFill)
- Aby usunąć obraz, kliknij **"Usuń obraz"**
- Jeśli brak obrazu, używany jest kolor tła

### ➡️ **Strzałki (NOWOŚĆ!)**

#### Tworzenie
1. Kliknij przycisk **"Rysuj strzałkę"** w górnym pasku (przycisk zostanie podświetlony)
2. Kliknij w miejscu **początku** strzałki (na canvas)
3. Kliknij w miejscu **końca** strzałki
4. Strzałka zostanie narysowana automatycznie z grotem na końcu
5. Tryb rysowania wyłącza się automatycznie

#### Właściwości
- Domyślny kolor: czarny (#FF000000)
- Domyślna grubość: 2 piksele
- Automatyczny grot strzałki na końcu (trójkąt)
- Strzałki są rysowane pod folderami i notatkami

#### Operacje
- **Usuwanie**: Prawy przycisk myszy na strzałce → Usuń strzałkę (z potwierdzeniem)
- Strzałki są zapisywane wraz z innymi elementami w pliku JSON

## 💡 Wskazówki

1. **Organizacja**: Twórz hierarchię folderów
2. **Emoji**: Używaj emoji jako ikon (📁 📚 🎬 💼 🏫 🎓 📂)
3. **Tła obrazkowe**: Używaj zdjęć jako tła folderów
4. **Strzałki**: Łącz elementy aby pokazać zależności
5. **Drag & Drop**: Wszystkie pozycje są automatycznie zapisywane
