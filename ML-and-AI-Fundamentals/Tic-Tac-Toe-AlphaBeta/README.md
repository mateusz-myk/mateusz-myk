Tic-Tac-Toe Alpha-Beta Pruning

Implementacja klasycznego algorytmu przeszukiwania drzewa gry w kółko i krzyżyk, stworzona w ramach kursu Uczenie Maszynowe.

Kluczowe cechy:

-Algorytm: Minimax z optymalizacją Alpha-Beta Pruning.

-Efektywność: Znaczne ograniczenie liczby sprawdzanych gałęzi drzewa bez utraty jakości ruchu.

-Testy: Zawiera testy jednostkowe porównujące decyzje algorytmu z oczekiwanymi wynikami.

Od zera: Czysty Python i NumPy, skupienie na logice rekurencyjnej i optymalizacji.

W przeciwieństwie do MCTS, ten algorytm jest deterministyczny i gwarantuje wybór najlepszego możliwego ruchu (perfect play) poprzez inteligentne "odcinanie" gałęzi drzewa, które nie wpłyną na ostateczną decyzję.
