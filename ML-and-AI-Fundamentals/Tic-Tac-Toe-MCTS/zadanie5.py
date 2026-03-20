# Zadanie 3
# Mateusz Wizner

import random
import math
from copy import deepcopy


# 1 = gracz, -1 = komputer, 0 = puste pole

class Node:
    def __init__(self, board, player, parent=None, move=None):
        self.board = deepcopy(board)
        self.player = player
        self.parent = parent
        self.move = move

        self.children = []
        self.visits = 0
        self.value = 0

        self.untried_moves = get_available_moves(board)


def print_board(board):
    symbols = {1: 'X', -1: 'O', 0: '-'}
    print("   1 2 3")
    for i in range(3):
        print(i + 1, end="  ")
        for j in range(3):
            print(symbols[board[i][j]], end=" ")
        print()


def is_winner(board, player):
    for row in board:
        if all(x == player for x in row):
            return True

    for c in range(3):
        if all(board[r][c] == player for r in range(3)):
            return True

    if all(board[i][i] == player for i in range(3)):
        return True
    if all(board[i][2 - i] == player for i in range(3)):
        return True

    return False


def is_board_full(board):
    return all(cell != 0 for row in board for cell in row)


def get_available_moves(board):
    moves = []
    for i in range(3):
        for j in range(3):
            if board[i][j] == 0:
                moves.append((i, j))
    return moves


def get_result(board):
    if is_winner(board, -1):
        return 1     # wygrana komputera
    if is_winner(board, 1):
        return -1    # przegrana komputera
    if is_board_full(board):
        return 0     # remis
    return None


def simulate(board, player):
    board_copy = deepcopy(board)
    current = player

    while True:
        result = get_result(board_copy)
        if result is not None:
            return result

        move = random.choice(get_available_moves(board_copy))
        board_copy[move[0]][move[1]] = current
        current = -current


def select_child(node, c=1.4):
    best = None
    best_score = -1e9

    for child in node.children:
        exploitation = child.value / child.visits # jak dobry jest ten ruch
        exploration = c * math.sqrt(math.log(node.visits) / child.visits) # jak mało wiemy o tym ruchu
        score = exploitation + exploration

        if score > best_score:
            best_score = score
            best = child

    return best


def mcts(board, iterations=500):
    root = Node(board, -1)

    for i in range(iterations):
        node = root

        # Selection
        while node.untried_moves == [] and node.children:
            node = select_child(node)

        # Expansion
        if node.untried_moves:
            move = node.untried_moves.pop()
            new_board = deepcopy(node.board)
            new_board[move[0]][move[1]] = node.player

            child = Node(new_board, -node.player, node, move)
            node.children.append(child) #!!!!!!
            node = child

        # Simulation
        result = simulate(node.board, node.player)

        # Backpropagation
        while node is not None:
            node.visits += 1
            node.value += result
            node = node.parent

    #best_child = max(root.children, key=lambda c: c.visits)

    best_child = root.children[0]

    for c in root.children:
        if c.visits > best_child.visits:
            best_child = c

    return best_child.move


def play_game():
    board = [[0, 0, 0],
             [0, 0, 0],
             [0, 0, 0]]

    print("Tic-Tac-Toe z MCTS")
    print("Ty: X | Komputer: O")
    print()

    while True:
        print_board(board)
        print()

        while True:
            pos = input("Twój ruch (11-33): ").strip()
            if len(pos) == 2 and pos.isdigit():
                r = int(pos[0]) - 1
                c = int(pos[1]) - 1
                if 0 <= r <= 2 and 0 <= c <= 2 and board[r][c] == 0:
                    board[r][c] = 1
                    break
            print("Niepoprawny ruch")

        if is_winner(board, 1):
            print_board(board)
            print("Wygrałeś")
            break

        if is_board_full(board):
            print_board(board)
            print("Remis")
            break

        move = mcts(board)
        board[move[0]][move[1]] = -1

        if is_winner(board, -1):
            print_board(board)
            print("Komputer wygrał")
            break

        if is_board_full(board):
            print_board(board)
            print("Remis")
            break

        print()


if __name__ == "__main__":
    play_game()
