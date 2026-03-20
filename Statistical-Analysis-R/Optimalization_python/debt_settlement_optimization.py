import pulp
problem = pulp.LpProblem("Minimalizaja ilosci przelanych pieniedzy", pulp.LpMinimize)

ania_do_bartek = pulp.LpVariable("ania_do_bartek", lowBound=0, cat='Continuous')
ania_do_celina = pulp.LpVariable("ania_do_celina", lowBound=0, cat='Continuous')

celina_do_bartek = pulp.LpVariable("celina_do_bartek", lowBound=0, cat='Continuous')

dominika_do_ania = pulp.LpVariable("dominika_do_ania", lowBound=0, cat='Continuous')
dominika_do_bartek = pulp.LpVariable("dominika_do_bartek", lowBound=0, cat='Continuous')
dominika_do_celina = pulp.LpVariable("dominika_do_celina", lowBound=0, cat='Continuous')

# Bilans dla każdej osoby
problem += -ania_do_bartek - ania_do_celina + dominika_do_ania == -30, "Bilans Ani"
problem += dominika_do_bartek + celina_do_bartek + ania_do_bartek == 60, "Bilans Bartka"
problem += dominika_do_celina + ania_do_celina - celina_do_bartek == 20, "Bilans Celiny"
problem += -dominika_do_ania -dominika_do_bartek -dominika_do_celina == -50, "Bilans Dominiki"

problem.solve()
print("Status:",pulp.LpStatus[problem.status])

print(f"Anna powinna wysłać Bartkowi: {ania_do_bartek.varValue} zł")
print(f"Anna powinna wysłać Celinie: {ania_do_celina.varValue} zł")
print(f"Celina powinna wysłać Bartkowi: {celina_do_bartek.varValue} zł")
print(f"Dominika powinna wysłać Ani: {dominika_do_ania.varValue} zł")
print(f"Dominika powinna wysłać Bartkowi: {dominika_do_bartek.varValue} zł")
print(f"Dominika powinna wysłać Celinie: {dominika_do_celina.varValue} zł")