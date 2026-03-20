import pulp

problem = pulp.LpProblem("Największy rabat", pulp.LpMaximize)

wagi = [250, 300, 450, 100, 240, 340, 160, 260, 360, 190]
wartosci = [3.99, 4.75, 7.00, 1.50, 3.75, 5.30, 2.50, 4.00, 5.60, 3.00]

#0 - nie wybieramy, 1 - wybieramy
wybor = [pulp.LpVariable(f"produkt_{i}", lowBound=0, upBound=1, cat='Integer') for i in range(10)]

problem += pulp.lpSum([wartosci[i] * wybor[i] for i in range(10)]), "Wartość zakupów"
problem += pulp.lpSum([wagi[i] * wybor[i] for i in range(10)]) <= 1500, "Max waga"

problem.solve()

print("\nWybrane produkty:")
suma_wartosci = 0
for i in range(10):
    if wybor[i].value() == 1:
        print(f"Produkt {i+1}: wartość = {wartosci[i]} zł")
        suma_wartosci += wartosci[i]

print(f"Rabat: {suma_wartosci/2} zł")
print("Status:", pulp.LpStatus[problem.status])
