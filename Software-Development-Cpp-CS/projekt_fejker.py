from faker import Faker
import random
import mysql.connector
from datetime import timedelta

# Inicjalizacja Faker
fake = Faker('pl_PL')

try:
    connection = mysql.connector.connect(
        host='giniewicz.it',       # Adres serwera bazy danych
        database='team23',         # Nazwa bazy danych
        user='team23',             # Nazwa użytkownika
        password='te@mzaze'        # Hasło
    )

    print("Połączono z bazą danych")
    cursor = connection.cursor()

    tables = ["Klienci", "Oferty", "Pracownicy", "Transakcje", "Wycieczki", "Pracownicy_Wycieczki", "Adresy", "Miasta"]
    cursor.execute("SET foreign_key_checks = 0;")

    for table in tables:
        cursor.execute(f"TRUNCATE TABLE {table}")
        connection.commit()

    cursor.execute("SET foreign_key_checks = 1;")
    connection.commit()


    # Liczba rekordów do wygenerowania
    liczba_pracownikow = 12 # conajmniej 6
    liczba_klientow = 50
    liczba_ofert = 12
    liczba_wycieczek = 20
    liczba_transakcji = 60 # musi być większa od liczby klientów
    temp = random.randint(1, 15)

    # === Wypełnianie tabeli Pracownicy ===


    stanowiska = {
        "Przewodnik": (4000.0, 4500.0),
        "Kierownik": (5000.0, 5500.0),
        "Asystent": (3500.0, 3800.0)
    }

    conajmniej_zawod = {
        "Kierownik": 1,
        "Przewodnik": 3,
        "Asystent": 2
    }

    pracownicy = []

    for stanowisko, min_count in conajmniej_zawod.items():
        for _ in range(min_count):
            imie = fake.first_name()
            nazwisko = fake.last_name()
            data_zatrudnienia = fake.date_between(start_date='-5y', end_date='-1y')
            wynagrodzenie = round(random.uniform(*stanowiska[stanowisko]), 2)
            pracownicy.append((imie, nazwisko, data_zatrudnienia, stanowisko, wynagrodzenie))

    for _ in range(len(pracownicy) + 1, liczba_pracownikow + 1):
        imie = fake.first_name()
        nazwisko = fake.last_name()
        data_zatrudnienia = fake.date_between(start_date='-5y', end_date='-1y')
        stanowisko = random.choice(list(stanowiska.keys()))
        wynagrodzenie = round(random.uniform(*stanowiska[stanowisko]), 2)
        pracownicy.append((imie, nazwisko, data_zatrudnienia, stanowisko, wynagrodzenie))

    for i, (imie, nazwisko, data_zatrudnienia, stanowisko, wynagrodzenie) in enumerate(pracownicy, start=1):
        query = """
        INSERT INTO Pracownicy (ID, Imie, Nazwisko, DataZatrudnienia, Stanowisko, Wynagrodzenie)
        VALUES (%s, %s, %s, %s, %s, %s);
        """
        values = (i, imie, nazwisko, data_zatrudnienia, stanowisko, wynagrodzenie)
        cursor.execute(query, values)


    # === Wypełnianie tabeli Klienci ===
    for i in range(1, liczba_klientow + 1):
        imie = fake.first_name()
        nazwisko = fake.last_name()
        email = imie + "." + nazwisko + "@wp.pl"
        telefon = fake.phone_number()
        if i < liczba_klientow - temp:
            Adres_ID = i
        else:
            Adres_ID = random.randint(1, liczba_klientow - temp)

        TelefonBliskiego = fake.phone_number()

        query = """
        INSERT INTO Klienci (KlientID, Imie, Nazwisko, Email, Telefon, Adres_ID, TelefonBliskiego)
        VALUES (%s, %s, %s, %s, %s, %s, %s);
        """
        values = (i, imie, nazwisko, email, telefon, Adres_ID, TelefonBliskiego)
        cursor.execute(query, values)

    # === Wypełnianie tabeli Adresy ===
    liczba_miast = int(0.8 * (liczba_klientow + 1 - temp))

    for i in range(1, liczba_klientow-temp):
        Ulica = fake.street_name()
        Numer = random.randint(1, 100)
        if i < liczba_miast:
            Miasto_ID = i
        else:
            Miasto_ID = random.randint(1, liczba_miast-1)

        query = """
        INSERT INTO Adresy (Adres_ID, Ulica, Numer, Miasto_ID)
        VALUES (%s, %s, %s, %s);
        """
        values = (i, Ulica, Numer, Miasto_ID)
        cursor.execute(query, values)

    # === Wypełnianie tabeli Miasta ===
    for i in range(1, liczba_miast):
        miasto = fake.city()

        query = """
        INSERT INTO Miasta (Miasto_ID, miasto)
        VALUES (%s, %s);
        """
        values = (i, miasto)
        cursor.execute(query, values)


    # === Wypełnianie tabeli Oferty ===
    fake = Faker()

    oferta_nazwy = [
        "Wyprawa w Góry", 
        "Wieczór Kawalerski", 
        "Koncert Rockowy", 
        "Zwiedzanie Miasta", 
        "Obóz Przetrwania",
        "Kulinarna Podróż",
        "Rejs po Rzece",
        "Wizyta w Winiarni",
        "Agroturystyka",
        "Luksusowy Relaks w SPA"
    ]

    oferta_opis = [
        "Wzbogać swoje życie o niezapomniane wspomnienia, które zostaną z Tobą na zawsze. Czas na wyjątkową przygodę, pełną niespodzianek!",
        "Przekrocz granice codzienności, otwórz się na nowe doświadczenia i poczuj dreszczyk emocji. Czas na coś wyjątkowego!",
        "Zrób krok ku przygodzie, której nigdy nie zapomnisz. Odkryj miejsca, które wyzwolą w Tobie energię i radość życia!",
        "Poczuj adrenalinę i odkryj miejsca, które otworzą przed Tobą zupełnie nowe horyzonty. Czas na podróż, która zmieni Twoje spojrzenie na świat!",
        "Wyjątkowa okazja, aby oderwać się od rutyny i przeżyć coś, czego nigdy wcześniej nie doświadczyłeś. Czas na prawdziwą przygodę!",
        "Czekają na Ciebie niezwykłe miejsca, które pozwolą Ci odkryć świat w zupełnie nowy sposób. Pozwól, by podróż stała się Twoim najpiękniejszym wspomnieniem!",
        "Zasmakuj w chwili pełnej przygód i emocji, które będą towarzyszyć Ci jeszcze długo po powrocie. Przeżyj coś wyjątkowego!",
        "Pożegnaj nudę, witaj w nowym świecie! Czas na moment, który otworzy Ci oczy na niezwykłe możliwości!",
        "Wszystko, czego potrzebujesz, to odrobina odwagi, by dać się porwać nowym doświadczeniom. Sprawdź, co czeka na Ciebie!",
        "Poczuj magię chwil, które przekształcą każdy dzień w niezapomniane wspomnienie. Zrób pierwszy krok ku przygodzie!"
    ]

    kraje_euro = [
    "Austria", "Cypr", "Francja",
    "Grecja", "Hiszpania", "Portugalia", 
    "Słowacja", "Słowenia", "Włochy", "Niemcy", "Andora",
    "Turcja", "Polska", "Szwajcaria"
    ]

    ceny = [0] * (liczba_ofert + 1)

    # Generowanie ofert
    for i in range(1, liczba_ofert + 1):
        nazwa = random.choice(oferta_nazwy)
        cena = round(random.uniform(1200, 5000))
        ceny[i] = cena
        opis = random.choice(oferta_opis)
        kierunek = random.choice(kraje_euro)

        query = """
        INSERT INTO Oferty (OfertaID, Nazwa, Cena, Opis, Kierunek)
        VALUES (%s, %s, %s, %s, %s);
        """
        values = (i, nazwa, cena, opis, kierunek)

        cursor.execute(query, values)
        connection.commit() 

    lista_wycieczek = []

    Daty_Wyjazow = []

    # === Wypełnianie tabeli Wycieczki ===
    for i in range(1, liczba_wycieczek + 1):
        oferta_id = random.randint(1, liczba_ofert)
        lista_wycieczek.append(oferta_id)
        DataWyjazdu = fake.date_between(start_date='-1y', end_date='today')
        Daty_Wyjazow.append(DataWyjazdu)
        DataPowrotu = fake.date_between(start_date=DataWyjazdu + timedelta(days=3), end_date=DataWyjazdu + timedelta(days=7))
        koszt = ceny[oferta_id] - random.uniform(400, 600)

        query = """
        INSERT INTO Wycieczki (WycieczkaID, DataWyjazdu, DataPowrotu, OfertaID, Koszt)
        VALUES (%s, %s, %s, %s, %s);
        """
        values = (i, DataWyjazdu, DataPowrotu, oferta_id , koszt)
        cursor.execute(query, values)

    

    # === Wypełnianie tabeli Transakcje ===

    transakcje = []

    #każdy klient przynajmniej raz kupi wycieczkę
    for klient_id in range(1, liczba_klientow + 1):
        wycieczka_id = random.randint(1, liczba_wycieczek)
        kwota = ceny[lista_wycieczek[wycieczka_id - 1]] 
        data_transakcji = fake.date_between(start_date='-1y', end_date=Daty_Wyjazow[wycieczka_id - 1])

        transakcje.append((klient_id, wycieczka_id, round(kwota, 2), data_transakcji))

    # pozostałe transakcje
    for _ in range(liczba_transakcji - liczba_klientow):
        klient_id = random.randint(1, liczba_klientow)
        wycieczka_id = random.randint(1, liczba_wycieczek)
        kwota = ceny[lista_wycieczek[wycieczka_id - 1]]
        data_transakcji = fake.date_between(start_date='-1y', end_date=Daty_Wyjazow[wycieczka_id - 1])

        transakcje.append((klient_id, wycieczka_id, round(kwota, 2), data_transakcji))

    for i, (klient_id, wycieczka_id, kwota, data_transakcji) in enumerate(transakcje, start=1):
        query = """
        INSERT INTO Transakcje (TransakcjaID, KlientID, WycieczkaID, Kwota, DataTransakcji)
        VALUES (%s, %s, %s, %s, %s);
        """
        values = (i, klient_id, wycieczka_id, kwota, data_transakcji)
        cursor.execute(query, values)


    # === Wypełnianie tabeli Pracownicy_Wycieczki ===

    Rola = [
        "Przewodnik Główny",
        "Przewodnik Pomocniczy"
    ]

    # conajmniej jeden główny i pomocniczny
    for IDwycieczki in range(1, liczba_wycieczek + 1):
        przewodnicy = random.sample(range(1, liczba_pracownikow + 1), 2)  # Wybierz 2 różnych pracowników
        for i, rola in enumerate(Rola):
            IDpracownika = przewodnicy[i]
            query = """
            INSERT INTO Pracownicy_Wycieczki (PracownikID, WycieczkaID, Rola)
            VALUES (%s, %s, %s);
            """
            values = (IDpracownika, IDwycieczki, rola)
            cursor.execute(query, values)

    connection.commit()
    print("Wszystkie tabele zostały wypełnione danymi")

except mysql.connector.Error as e:
    print(f"Błąd podczas połączenia z MariaDB/MySQL: {e}")

finally:
    if connection.is_connected():
        cursor.close()
        connection.close()
        print("Połączenie z bazą danych zostało zamknięte.")