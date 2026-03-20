# Analiza Przeżycia (Survival Analysis) w języku R

Ten folder zawiera jeden z projektów wykonanych w ramach przedmiotu **Analiza Przeżycia**. Skupiają się one na modelowaniu czasu do wystąpienia zdarzenia z uwzględnieniem danych cenzurowanych.

## Zawartość folderu
* `Survial_analysis_R.Rmd`: Kod źródłowy w formacie R Markdown. Zawiera pełną implementację obliczeń, symulacji Monte Carlo oraz generowanie wykresów.
* `Survial_analysis_R.pdf`: Wygenerowany raport końcowy, zawierający interpretację wyników, tabele i wizualizacje.

## Zakres analizy
Projekt obejmuje następujące zagadnienia:
1.  **Analiza teoretyczna:** Wyznaczanie funkcji hazardu, gęstości oraz przeżycia dla różnych rozkładów (m.in. wykładniczy, Weibulla).
2.  **Symulacje:** Badanie mocy asymptotycznego testu ilorazu wiarygodności dla danych cenzurowanych.
3.  **Analiza danych klinicznych:** Porównanie czasu do remisji dwóch grup pacjentów (Lek A vs Lek B) przy użyciu metod estymacji największej wiarygodności (MLE).
4.  **Wnioskowanie statystyczne:** Testowanie hipotez dotyczących parametrów skali i kształtu w modelach trwania.

## Technologia
* **Język:** R
* **Biblioteki:** `ggplot2` (wizualizacja), `patchwork` (składanie wykresów), `knitr` (raportowanie).
* **Silnik składu:** XeLaTeX (dla poprawnego renderowania polskiej czcionki i wzorów matematycznych).
