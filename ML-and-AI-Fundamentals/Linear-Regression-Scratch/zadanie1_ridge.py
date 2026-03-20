#!/usr/bin/env python3

import numpy as np
import pytest
from sklearn.linear_model import Ridge

class RidgeRegr:
    def __init__(self, alpha = 0.0):
        self.alpha = alpha

    def fit(self, X, Y):
        # wejscie:
        #  X = np.array, shape = (n, m)
        #  Y = np.array, shape = (n)
        # Znajduje theta (w przyblizeniu) minimalizujace kwadratowa funkcje kosztu L uzywajac metody iteracyjnej.
        n, m = X.shape
        X_ = np.hstack([np.ones((n, 1)), X])
        self.theta = np.zeros(m + 1)
        for _ in range(10000):
            y_pred = X_ @ self.theta
            # gradient błędu średniokwadratowego: 1/n * X^T * (y - Y)
            # gradient regularyzacji L2: α​/n * θ
            # funkcja kosztu Ridge: J(θ) = 1/n * ||Xθ - Y||^2 + α​/n ||θ{1:}||^2
            grad = (X_.T @ (y_pred - Y)) / n + (self.alpha / n) * self.theta
            grad[0] -= (self.alpha / n) * self.theta[0]
            self.theta -= 0.05 * grad

        return self
    
    def predict(self, X):
        # wejscie
        #  X = np.array, shape = (k, m)
        # zwraca
        #  Y = wektor(f(X_1), ..., f(X_k))
        k, m = X.shape
        X = np.hstack([np.ones((k, 1)), X])
        return X @ self.theta


def test_RidgeRegressionInOneDim():
    X = np.array([1,3,2,5]).reshape((4,1))
    Y = np.array([2,5, 3, 8])
    X_test = np.array([1,2,10]).reshape((3,1))
    alpha = 0.3
    expected = Ridge(alpha).fit(X, Y).predict(X_test)
    actual = RidgeRegr(alpha).fit(X, Y).predict(X_test)
    print(expected)
    print(actual)
    #assert list(actual) == pytest.approx(list(expected), rel=1e-5)

def test_RidgeRegressionInThreeDim():
    X = np.array([1,2,3,5,4,5,4,3,3,3,2,5]).reshape((4,3))
    Y = np.array([2,5, 3, 8])
    X_test = np.array([1,0,0, 0,1,0, 0,0,1, 2,5,7, -2,0,3]).reshape((5,3))
    alpha = 0.4
    expected = Ridge(alpha).fit(X, Y).predict(X_test)
    actual = RidgeRegr(alpha).fit(X, Y).predict(X_test)
    print(expected)
    print(actual)
    assert list(actual) == pytest.approx(list(expected), rel=1e-3)

#test_RidgeRegressionInOneDim()
test_RidgeRegressionInThreeDim()