# 3-Body Problem Simulator

Interaktywna symulacja fizyczna problemu trzech ciał, wykorzystująca **prawo powszechnego ciążenia Newtona** do modelowania skomplikowanych orbit oraz kolizji.

### Kluczowe cechy:
* **Silnik Fizyczny:** Autorska implementacja obliczeń sił grawitacyjnych, wektorów prędkości oraz zasad zachowania pędu.
* **Interaktywne GUI:** Możliwość wprowadzania masy, pozycji i prędkości początkowej ciał w czasie rzeczywistym dzięki bibliotece **Pygame**.
* **Zaawansowane Funkcje:** * Mechanizm **łączenia ciał (merging)** przy kolizjach.
    * Wizualizacja śladu (**orbital trail**) ułatwiająca analizę trajektorii.
    * Dynamiczna zmiana prędkości symulacji oraz obsługa "ścian" (odbicia).

### Dlaczego ten projekt?
W przeciwieństwie do statycznych wykresów, projekt ten pozwala na eksperymentalne badanie **chaosu w układach grawitacyjnych** poprzez wizualną, płynną symulację w 60 FPS. Pozwala to na intuicyjne zrozumienie, jak minimalne zmiany parametrów początkowych wpływają na stabilność całego układu.
