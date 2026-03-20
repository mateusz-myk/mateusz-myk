import pulp

problem = pulp.LpProblem("Minimializacja wartości produktów pozostawionych w lodówce", pulp.LpMinimize)

tortilla_z_kurczakiem_i_pomidorem = pulp.LpVariable("tortilla_z_kurczakiem_i_pomidorem", lowBound=0, cat='Integer')
tortilla_z_kurczakiem_i_mozzarellą = pulp.LpVariable("tortilla_z_kurczakiem_i_mozzarellą", lowBound=0, cat='Integer')
tortilla_z_pomidorem_i_mozzarellą = pulp.LpVariable("tortilla_z_pomidorem_i_mozzarellą", lowBound=0, cat='Integer')
sałatka_z_kurczakiem_mozzarella_i_pomidorem = pulp.LpVariable("sałatka_z_kurczakiem_mozzarella_i_pomidorem", lowBound=0, cat='Integer')

cena_tortilla = 6.99 / 4 # 1 sztuka
cena_kurczak = 19.99 / 1000 # 1 gram
cena_pomidor = 3.5 # 1 sztuka
cena_mozzarella = 3.99 / 125 # 1gram

problem += 200 * tortilla_z_kurczakiem_i_pomidorem + 250 * tortilla_z_kurczakiem_i_mozzarellą + 125 * sałatka_z_kurczakiem_mozzarella_i_pomidorem <= 1000, "Kurczak"
problem += tortilla_z_kurczakiem_i_pomidorem + tortilla_z_kurczakiem_i_mozzarellą + tortilla_z_pomidorem_i_mozzarellą <= 4, "Tortilla"
problem += tortilla_z_kurczakiem_i_pomidorem + tortilla_z_pomidorem_i_mozzarellą + sałatka_z_kurczakiem_mozzarella_i_pomidorem <= 5, "Pomidor"
problem += 125*tortilla_z_kurczakiem_i_mozzarellą + 125*tortilla_z_pomidorem_i_mozzarellą + 125*sałatka_z_kurczakiem_mozzarella_i_pomidorem <= 500, "Mozzarella"

pozostawiony_kurczak = 1000 - (200 * tortilla_z_kurczakiem_i_pomidorem + 250 * tortilla_z_kurczakiem_i_mozzarellą + 125 * sałatka_z_kurczakiem_mozzarella_i_pomidorem)
pozostawiony_pomidor = 5 - (tortilla_z_kurczakiem_i_pomidorem + tortilla_z_pomidorem_i_mozzarellą + sałatka_z_kurczakiem_mozzarella_i_pomidorem)
pozostawiona_mozzarella = 500 - (125*tortilla_z_kurczakiem_i_mozzarellą + 125*tortilla_z_pomidorem_i_mozzarellą + 125*sałatka_z_kurczakiem_mozzarella_i_pomidorem)
pozostawiona_tortilla = 4 - (tortilla_z_kurczakiem_i_pomidorem + tortilla_z_kurczakiem_i_mozzarellą + tortilla_z_pomidorem_i_mozzarellą)

problem += (
    cena_kurczak * pozostawiony_kurczak +
    cena_tortilla * pozostawiona_tortilla +
    cena_pomidor * pozostawiony_pomidor +
    cena_mozzarella * pozostawiona_mozzarella
), "Minimalizacja pozostawionej wartości"

problem.solve()
print("Status:", pulp.LpStatus[problem.status])

print(f"Tortilla z kurczakiem i pomidorem: {tortilla_z_kurczakiem_i_pomidorem.varValue}")
print(f"Tortilla z kurczakiem i mozzarellą: {tortilla_z_kurczakiem_i_mozzarellą.varValue}")
print(f"Sałatka z kurczakiem, mozzarellą i pomidorem: {sałatka_z_kurczakiem_mozzarella_i_pomidorem.varValue}")
print(f"Tortilla z pomidorem i mozzarellą: {tortilla_z_pomidorem_i_mozzarellą.varValue}")
print(f"Wartość pozostałych składników w lodówce: {pulp.value(problem.objective)} zł")