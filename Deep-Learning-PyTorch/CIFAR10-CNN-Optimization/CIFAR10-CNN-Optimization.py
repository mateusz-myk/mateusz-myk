import torch
import torch.nn as nn
import torch.optim as optim
from torchvision import datasets, transforms
from torch.utils.data import DataLoader

BATCH_SIZE = 64  # ile obrazków naraz w treningu
EPOCHS = 10
LR = 1e-3
DEVICE = "cuda"

transform = transforms.Compose([
    transforms.ToTensor(), # z obrazka na np. (3, 32,32) z wartościami od 0 do 1
    transforms.Normalize(
        mean=(0.5, 0.5, 0.5),
        std=(0.5, 0.5, 0.5)
    )
])

train_dataset = datasets.CIFAR10(root="./data", train=True, download=True, transform=transform)

test_dataset = datasets.CIFAR10(root="./data", train=False, download=True, transform=transform)

train_loader = DataLoader(train_dataset, batch_size=BATCH_SIZE, shuffle=True) # dzieli dane na paczki po 64

test_loader = DataLoader(test_dataset, batch_size=BATCH_SIZE, shuffle=False)

# MODELE

# 1. BASELINE
class CNN_Base(nn.Module):
    def __init__(self):
        super().__init__()
        self.conv1 = nn.Conv2d(3, 32, 3, padding=1) # 32 detektory
        self.conv2 = nn.Conv2d(32, 64, 3, padding=1) # 32 detektory łączą się z 64 detektorami
        self.pool = nn.MaxPool2d(2, 2) # dzieli na kwadraty 2x2 i wybiera maksymalną wartość z każdego kwadratu
        self.fc1 = nn.Linear(64 * 8 * 8, 256) # z 4096 wejść dostaje 256 opisów
        self.fc2 = nn.Linear(256, 10)
        # self.fc3 = nn.Linear(4096, 10)

    def forward(self, x):
        x = self.pool(torch.relu(self.conv1(x))) # torch.relu zamienia wartości ujemne na 0
        x = self.pool(torch.relu(self.conv2(x)))
        x = x.view(x.size(0), -1) # (1,4096)
        x = torch.relu(self.fc1(x))
        return self.fc2(x)

# 2. DROPOUT
class CNN_Dropout(nn.Module):
    def __init__(self):
        super().__init__()
        self.conv1 = nn.Conv2d(3, 32, 3, padding=1)
        self.conv2 = nn.Conv2d(32, 64, 3, padding=1)
        self.pool = nn.MaxPool2d(2, 2)
        self.fc1 = nn.Linear(64 * 8 * 8, 256)
        self.dropout = nn.Dropout(p=0.5) # losowe wyłączanie neuronów
        self.fc2 = nn.Linear(256, 10)

    def forward(self, x):
        x = self.pool(torch.relu(self.conv1(x)))
        x = self.pool(torch.relu(self.conv2(x)))
        x = x.view(x.size(0), -1)
        x = torch.relu(self.fc1(x))
        x = self.dropout(x)
        return self.fc2(x)

# 3. BATCH NORMALIZATION
class CNN_BatchNorm(nn.Module): # znormalizowana_wartość = (wartość - średnia) / odchylenie_standardowe ###### wynik = y * znormalizowane + B
    def __init__(self):
        super().__init__()
        self.conv1 = nn.Conv2d(3, 32, 3, padding=1)
        self.bn1 = nn.BatchNorm2d(32) 
        self.conv2 = nn.Conv2d(32, 64, 3, padding=1)
        self.bn2 = nn.BatchNorm2d(64)
        self.pool = nn.MaxPool2d(2, 2)
        self.fc1 = nn.Linear(64 * 8 * 8, 256)
        self.fc2 = nn.Linear(256, 10)

    def forward(self, x):
        x = self.pool(torch.relu(self.bn1(self.conv1(x)))) # albo x = self.pool(self.bn1(torch.relu(self.conv1(x))))
        x = self.pool(torch.relu(self.bn2(self.conv2(x))))
        x = x.view(x.size(0), -1)
        x = torch.relu(self.fc1(x))
        return self.fc2(x)

# 4. BATCH NORM + DROPOUT
class CNN_BN_Dropout(nn.Module):
    def __init__(self):
        super().__init__()
        self.conv1 = nn.Conv2d(3, 32, 3, padding=1)
        self.bn1 = nn.BatchNorm2d(32)
        self.conv2 = nn.Conv2d(32, 64, 3, padding=1)
        self.bn2 = nn.BatchNorm2d(64)
        self.pool = nn.MaxPool2d(2, 2)
        self.fc1 = nn.Linear(64 * 8 * 8, 256)
        self.dropout = nn.Dropout(p=0.5)
        self.fc2 = nn.Linear(256, 10)

    def forward(self, x):
        x = self.pool(torch.relu(self.bn1(self.conv1(x))))
        x = self.pool(torch.relu(self.bn2(self.conv2(x))))
        x = x.view(x.size(0), -1)
        x = torch.relu(self.fc1(x))
        x = self.dropout(x)
        return self.fc2(x)


# FUNKCJE TRENINGU I TESTU

def train(model):
    model.to(DEVICE)
    criterion = nn.CrossEntropyLoss() # sprawdzanie jak bardzo przewidywania różnią się od rzeczywistości
    optimizer = optim.Adam(model.parameters(), lr=LR) # zmiejszanie wag aby zminimalizować błąd

    for epoch in range(EPOCHS):
        model.train()
        running_loss = 0.0
        correct = 0
        total = 0

        for x, y in train_loader:    # x obrazki, y co na nich jest
            x, y = x.to(DEVICE), y.to(DEVICE)

            optimizer.zero_grad() # zerowanie gradientów po iteracji
            outputs = model(x)
            loss = criterion(outputs, y)
            loss.backward() # obliczanie gradientów błędu
            optimizer.step()


            running_loss += loss.item()
            preds = outputs.argmax(dim=1)
            correct += (preds == y).sum().item()
            total += y.size(0)

        print(f"Epoch {epoch+1}/{EPOCHS}, loss = {running_loss/len(train_loader):.4f}")
        print(f"Training accuracy after epoch {epoch+1}: {100 * correct / total:.2f}%")
        print("Test accuracy after epoch {}: {:.2f}%".format(epoch+1, test(model)))

def test(model):
    model.eval()
    correct_2 = 0
    total_2 = 0

    with torch.no_grad(): # nie uczymy się
        for x, y in test_loader:
            x, y = x.to(DEVICE), y.to(DEVICE)

            outputs = model(x)
            _, predicted = torch.max(outputs, 1)
            
            total_2 += y.size(0)
            correct_2 += (predicted == y).sum().item()

    acc = 100 * correct_2 / total_2
    print(f"Test accuracy: {acc:.2f}%")
    return acc




# PROGRAM

models = {
    "Baseline": CNN_Base(),
    "Dropout": CNN_Dropout(),
    "BatchNorm": CNN_BatchNorm(),
    "BatchNorm + Dropout": CNN_BN_Dropout()
}

results = {}

for name, model in models.items():
    print("\n==============================")
    print(f"Model: {name}")
    print("==============================")
    train(model)
    acc = test(model)
    results[name] = acc

print("\nPODSUMOWANIE:")
for name, acc in results.items():
    print(f"{name}: {acc:.2f}%")
