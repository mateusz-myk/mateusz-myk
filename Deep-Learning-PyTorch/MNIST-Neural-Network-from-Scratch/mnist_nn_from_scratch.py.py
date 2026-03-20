import numpy as np
import mnist_loader
import urllib.request

# Pobieranie danych MNIST
url = 'https://github.com/MichalDanielDobrzanski/DeepLearningPython35/raw/master/mnist.pkl.gz'
filename = 'mnist.pkl.gz'
urllib.request.urlretrieve(url, filename)

# Funkcja aktywacji (sigmoid) i jej pochodna
def sigmoid(z):
    return 1.0 / (1.0 + np.exp(-z))

def sigmoid_prime(z):
    return sigmoid(z) * (1 - sigmoid(z))

class Warstwa:
    def __init__(self, input_size, output_size):
        self.weights = np.random.randn(input_size, output_size) * 0.1
        self.biases = np.random.randn(1, output_size) * 0.1
        
    def forward(self, input_data):
        """
        Oblicza wyjście warstwy dla danego wejścia.
        """
        self.input = input_data
        self.z = np.dot(self.input, self.weights) + self.biases  # z = xW + b
        self.output = sigmoid(self.z)    # y = sigma(z)
        
        return self.output
        
    def backward(self, output_error, learning_rate):
        delta = output_error * sigmoid_prime(self.z)   # delta = ∂L/∂y * sigma'(z) , sigma'(z) = sigma(z)*[1 - sigma(z)]
        input_error = np.dot(delta, self.weights.T)   # ∂L/∂x = delta * W^T
        d_weights = np.dot(self.input.T, delta)
        self.weights -= learning_rate * d_weights
        self.biases -= learning_rate * delta
        return input_error

class Siec:
    def __init__(self):
        self.warstwy = []

    def dodaj_warstwe(self, warstwa):
        self.warstwy.append(warstwa)

    def predict(self, input_data):
        result = input_data
        for warstwa in self.warstwy:
            result = warstwa.forward(result)
        return result

    def fit(self, training_data, epochs, learning_rate):
        """
        training_data: lista krotek (x, y) z mnist_loader
        """
        for epoch in range(epochs):
            total_error = 0
            for x, y in training_data:
                output = x.reshape(1, 784)
                target = y.reshape(1, 10)
                output = self.predict(output)

                output_error = 2 * (output - target) / output.shape[1]
                total_error += np.mean((target - output) ** 2)
                for warstwa in reversed(self.warstwy):
                    output_error = warstwa.backward(output_error, learning_rate)
            
            print(f"Epoka {epoch+1}/{epochs}, średni błąd: {total_error/len(training_data):.6f}")

def sprawdz_skutecznosc(siec, test_data):
    poprawne = 0
    for x, y in test_data:
        wynik = siec.predict(x.reshape(1, 784))
        wybrana_cyfra = np.argmax(wynik)
        
        if wybrana_cyfra == y:
            poprawne += 1
            
    skutecznosc = (poprawne / len(test_data)) * 100
    print(f"Sieć rozpoznała poprawnie {poprawne} z {len(test_data)} cyfr.")
    print(f"Skuteczność: {skutecznosc:.2f}%")

if __name__ == "__main__":
    # Wczytywanie danych
    train_data, val_data, test_data = mnist_loader.load_data_wrapper()
    
    train_data = list(train_data)
    test_data = list(test_data)
    print("Dane wczytane. Buduję sieć...")
    
    # Tworzenie sieci
    net = Siec()
    net.dodaj_warstwe(Warstwa(784, 30)) 
    net.dodaj_warstwe(Warstwa(30, 10))
    
    # Trening
    print("Start treningu...")
    net.fit(train_data, epochs=5, learning_rate=0.5)
    
    # Testowanie
    print("Sprawdzam wiedzę sieci na egzaminie (dane testowe)...")
    sprawdz_skutecznosc(net, test_data)
    
    # Eksperymenty
    print("\n--- EKSPERYMENT 1: Mała liczba neuronów (10) ---")
    net_mala = Siec()
    net_mala.dodaj_warstwe(Warstwa(784, 10))
    net_mala.dodaj_warstwe(Warstwa(10, 10))
    net_mala.fit(train_data, epochs=3, learning_rate=0.5)
    sprawdz_skutecznosc(net_mala, test_data)
    
    print("\n--- EKSPERYMENT 2: Duża liczba neuronów (100) ---")
    net_duza = Siec()
    net_duza.dodaj_warstwe(Warstwa(784, 100))
    net_duza.dodaj_warstwe(Warstwa(100, 10))
    net_duza.fit(train_data, epochs=3, learning_rate=0.5)
    sprawdz_skutecznosc(net_duza, test_data)
