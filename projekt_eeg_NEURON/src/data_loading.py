from pathlib import Path
import pandas as pd
import numpy as np



# KONFIGURACJA

EEG_CHANNELS = [
    "Fp1", "Fp2", "F3", "F4",
    "C3", "C4", "P3", "P4", "O1",
    "O2", "F7", "F8", "T7", "T8",
    "P7", "P8", "Fz", "Cz", "Pz"
]

TARGET_COLUMN = "Class"
PATIENT_COLUMN = "ID"




# WCZYTANIE

def load_csv(path: str):
    path = Path(path)

    if not path.exists():
        raise FileNotFoundError(f"Nie znaleziono pliku: {path}")

    df = pd.read_csv(path)

    df[TARGET_COLUMN] = df[TARGET_COLUMN].astype(str)
    df[PATIENT_COLUMN] = df[PATIENT_COLUMN].astype(str)

    return df




# PROSTA WALIDACJA CZY STRUKTURA JEST OK

def validate_structure(df):
    missing = [col for col in EEG_CHANNELS if col not in df.columns]
    if missing:
        raise ValueError(f"Brakujące kanały EEG: {missing}")
    if TARGET_COLUMN not in df.columns:
        raise ValueError("Brak kolumny Class")
    if PATIENT_COLUMN not in df.columns:
        raise ValueError("Brak kolumny ID")




# POBRANIE SYGNAŁU PACJENTA

def get_patient_signal(df, patient_id):

    patient_df = df[df[PATIENT_COLUMN] == patient_id]
    # df[PATIENT_COLUMN] == patient_id, zwraca wektor bool z True i False
    # potem patient_df = df[wektor_bool], czyli tylko te z True

    if patient_df.empty:
        raise ValueError(f"Brak pacjenta o ID {patient_id}")

    X = patient_df[EEG_CHANNELS].values # czyli mamy tablice numPy z wartościami EEG dla tego pacjenta
    y = patient_df[TARGET_COLUMN].iloc[0] # bierzemy pierwszą wartość z kolumny Class (wszystkie powinny być takie same dla tego pacjenta)

    return X, y

def get_patient_ids(df):
    return df[PATIENT_COLUMN].unique().astype(str).to_numpy() # na numpy array, bo inaczej shuffle nie działa poprawnie




# PODZIAŁ PO PACJENTACH

def split_by_patient(df, test_size = 0.2, seed = 123):

    rng = np.random.default_rng(seed)
    patient_ids = get_patient_ids(df)
    rng.shuffle(patient_ids)

    n_test = int(len(patient_ids) * test_size)

    test_ids = patient_ids[0:n_test]
    train_ids = patient_ids[n_test:len(patient_ids)]

    train_df = df[df[PATIENT_COLUMN].isin(train_ids)]
    test_df = df[df[PATIENT_COLUMN].isin(test_ids)]

    return train_df, test_df




# GENEROWANIE OSI CZASU

def generate_time_vector(n_samples):
    return np.arange(n_samples) / 128 # Hz



# NORMALIZACJA

def standardize_per_patient(X): # X to tablica numPy z wartościami EEG dla jednego pacjenta, o kształcie (n_samples, n_channels)
    mean = np.mean(X, axis=0)
    std = np.std(X, axis=0)

    std = np.where(std == 0, 1.0, std) # ochorna przed dzieleniem przez zero
    return (X - mean) / std

def standardize_all(df):
    patient_ids = get_patient_ids(df)

    for pid in patient_ids:
        X, _ = get_patient_signal(df, pid)
        df.loc[df[PATIENT_COLUMN] == pid, EEG_CHANNELS] = standardize_per_patient(X)
        # wybieramy tylko te wiersze, które należą do tego pacjenta, i tylko kolumny z EEG, i nadpisujemy je znormalizowanymi wartościami

    return df




# TEST

if __name__ == "__main__":
    path = r"C:\Users\MSC\OneDrive\Pulpit\projekt na eeg\data\adhd-data.csv"

    df = load_csv(path)
    validate_structure(df)
    standardize_all(df)

    print("Liczba pacjentów:", len(get_patient_ids(df)))

    train_df, test_df = split_by_patient(df)

    print("Liczebność grupy treningowej:", len(get_patient_ids(train_df)))
    print("Liczebność grupy testowej:", len(get_patient_ids(test_df)))