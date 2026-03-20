#include <iostream>
#include <iomanip>
using namespace std;

struct Wezel {
    int wartosc;
    Wezel* lewy;
    Wezel* prawy;
    int kod;

    int operator[](int indeks);
};

Wezel* stworzWezel(int wartosc) {
    Wezel* wezel = new Wezel();
    wezel->wartosc = wartosc;
    wezel->lewy = nullptr;
    wezel->prawy = nullptr;
    wezel->kod = 0;
    return wezel;
}

void zakodujWezel(Wezel* wezel, int kod) {
    wezel->kod = kod;
}

int dekodujWezel(Wezel* wezel) {
    return wezel->kod;
}

Wezel* dodajWezly(Wezel* wezel1, Wezel* wezel2) {
    if (wezel1 == nullptr) return wezel2;
    if (wezel2 == nullptr) return wezel1;
    Wezel* nowyWezel = stworzWezel(wezel1->wartosc + wezel2->wartosc);
    zakodujWezel(nowyWezel, nowyWezel->kod + 1);
    nowyWezel->lewy = dodajWezly(wezel1->lewy, wezel2->lewy);
    nowyWezel->prawy = dodajWezly(wezel1->prawy, wezel2->prawy);
    return nowyWezel;
}

Wezel* odejmijWezly(Wezel* wezel1, Wezel* wezel2) {
    if (wezel1 == nullptr) return nullptr;
    if (wezel2 == nullptr) return wezel1;
    Wezel* nowyWezel = stworzWezel(wezel1->wartosc - wezel2->wartosc);
    zakodujWezel(nowyWezel, nowyWezel->kod + 1);
    nowyWezel->lewy = odejmijWezly(wezel1->lewy, wezel2->lewy);
    nowyWezel->prawy = odejmijWezly(wezel1->prawy, wezel2->prawy);
    return nowyWezel;
}

bool czyPuste(Wezel* wezel) {
    return wezel == nullptr;
}

void usunDrzewo(Wezel* wezel) {
    if (czyPuste(wezel)) return;
    usunDrzewo(wezel->lewy);
    usunDrzewo(wezel->prawy);
    delete wezel;
}

void wypiszDrzewo(Wezel* wezel, int wciecie = 0) {
    if (czyPuste(wezel)) return;

    wypiszDrzewo(wezel->prawy, wciecie + 4);

    cout << setw(wciecie) << " " << wezel->wartosc << endl;

    wypiszDrzewo(wezel->lewy, wciecie + 4);
}

Wezel* wstawWezel(Wezel* korzen, int wartosc) {
    if (korzen == nullptr) {
        korzen = stworzWezel(wartosc);
        return korzen;
    }

    if (wartosc < korzen->wartosc) {
        korzen->lewy = wstawWezel(korzen->lewy, wartosc);
    }

    else {
        korzen->prawy = wstawWezel(korzen->prawy, wartosc);
    }

    return korzen;
}

Wezel* stworzDrzewo(int k) {
    Wezel* korzen = nullptr;
    for (int i = 0; i < k; i++) {
        int wartosc;
        cin >> wartosc;
        korzen = wstawWezel(korzen, wartosc);
    }
    return korzen;
}

int Wezel::operator[](int indeks) {
    if (czyPuste(this)) return -1;
    if (indeks < 0) return -1;
    if (this->kod == indeks) return this->wartosc;
    else if (indeks < this->kod) {
        return this->lewy->operator[](indeks);
    } else {
        return this->prawy->operator[](indeks - 1);
    }
}

int main() {
    int n, k;
    cin >> n >> k;

    Wezel* drzewo1 = stworzDrzewo(n);
    Wezel* drzewo2 = stworzDrzewo(k);

    Wezel* drzewoDod = dodajWezly(drzewo1, drzewo2);
    Wezel* drzewoOde = odejmijWezly(drzewo1, drzewo2);

    cout << "Drzewo 1:\n";
    wypiszDrzewo(drzewo1);
    cout << endl;

    cout << "Drzewo 2:\n";
    wypiszDrzewo(drzewo2);
    cout << endl;

    cout << "Drzewo 1 + Drzewo 2:\n";
    wypiszDrzewo(drzewoDod);
    cout << endl;

    cout << "Drzewo 1 - Drzewo 2:\n";
    wypiszDrzewo(drzewoOde);
    cout << endl;

    cout << "Wartosci drzewa t1: ";
    for (int i = 0; i < n; i++) {
        cout << drzewo1->operator[](i) << " ";
    }
    cout << endl;

    usunDrzewo(drzewo1);
    usunDrzewo(drzewo2);
    usunDrzewo(drzewoDod);
    usunDrzewo(drzewoOde);

    getchar();
    getchar();
    return 0;
}
