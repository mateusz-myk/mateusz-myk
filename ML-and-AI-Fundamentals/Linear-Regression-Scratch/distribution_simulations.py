import numpy as np
import matplotlib.pyplot as plt

def generate_linear_distribiution(n, t):
    rng = np.random.default_rng()
    X = rng.uniform(0,1,(t,n))
    mu = 0.5
    sigma = np.sqrt(1/12)
    S = np.sum(X, axis=1)
    Z = (S - n*mu) / (sigma * np.sqrt(n))
    return Z

def generate_Cauchy_distribiution(n, t):
    rng = np.random.default_rng()
    X = rng.standard_cauchy(size = (t,n))
    sigma = X.std(axis=1)
    S = np.sum(X, axis=1)
    Z = S / (sigma * np.sqrt(n))
    return Z


#data = generate_linear_distribiution(n, t)

n = 1000
t = 10000

data = generate_Cauchy_distribiution(n, t)
 
plt.figure()
plt.hist(data, bins=60, density=True)
plt.xlabel('x')
plt.ylabel('y')
plt.show()
