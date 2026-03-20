import pulp

problem2 = pulp.LpProblem("Minimalizacja kosztów zasobów", pulp.LpMinimize)

koszt_maka = pulp.LpVariable("koszt_maka", lowBound=0, cat='Continuous')
koszt_maslo = pulp.LpVariable("koszt_maslo", lowBound=0, cat='Continuous')
koszt_cukier = pulp.LpVariable("koszt_cukier", lowBound=0, cat='Continuous')

problem2 += 2000 * koszt_maka + 1000 * koszt_maslo + 1000 * koszt_cukier, "Koszt calkowity"

problem2 += 250 * koszt_maka + 200 * koszt_maslo + 100 * koszt_cukier >= 30, "Ograniczenie dla ciastek 1"
problem2 += 200 * koszt_maka + 150 * koszt_maslo + 250 * koszt_cukier >= 35, "Ograniczenie dla ciastek 2"

problem2.solve()

print("Status:", pulp.LpStatus[problem2.status])
print(f"koszt_maka: {koszt_maka.varValue}")
print(f"koszt_maslo: {koszt_maslo.varValue}")
print(f"koszt_cukier: {koszt_cukier.varValue}")
