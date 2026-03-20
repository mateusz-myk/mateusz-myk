# Sokoban Console Game (C++)

Klasyczna gra logiczna Sokoban zrealizowana w konsoli przy użyciu języka C++. Projekt koncentruje się na efektywnym zarządzaniu tablicami dwuwymiarowymi oraz implementacji mechaniki poruszania się obiektami w ograniczonej przestrzeni.

### Funkcje projektu:
* **Dynamiczne ładowanie poziomów:** Gra wczytuje mapy bezpośrednio z zewnętrznych plików tekstowych (`1.txt`, `2.txt`, itd.).
* **System kolizji:** Zaawansowana logika sprawdzająca interakcje między graczem (`P`), skrzyniami (`C`) oraz ścianami (`#`).
* **Zarządzanie stanem gry:** Obsługa limitu ruchów, liczników skrzyń oraz resetowania poziomu w czasie rzeczywistym.
* **ASCII Interface:** Wizualizacja gry w oknie konsoli z wykorzystaniem znaków rozszerzonych ASCII.

### Technologie:
* **Język:** C++
* **Biblioteki:** `iostream`, `fstream`, `cstdlib` (zarządzanie strumieniami i systemem).
