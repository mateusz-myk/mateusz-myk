#zaimplementować algorytm alpha-beta, Monte Carlo Tree Search lub tablice Nalimova (jedno do wyboru) dla wybranej przez siebie gry
#(nie każda się do tego nadaje). Propozycje w miarę prostych gier: pan dla 2 osób, czwórki, kółko i krzyżyk: 3x3 lub gomoku, warcaby,
#młynek. W miarę możliwości najlepiej wybrać grę, z którą jest się jakoś emocjonalnie związanym, wtedy jest znacznie więcej radości z
#pisania takiego programu ;)
#
#kółko i krzyżyk i alpha-beta:

# Zadanie 4
# Mateusz Wizner
# numer indeksu 277508

def print_board(board):
    symbol_map = {1: 'X', -1: 'O', 0: '-'}
    print("   1 2 3")
    for i in range(3):
        row_num = i + 1
        row = board[i]
        print(row_num, end="  ")
        for j in range(3):
            cell_symbol = symbol_map[row[j]]
            print(cell_symbol, end=" ")
        print()



def is_winner(board, player):
    # rows
    for row in board:
        if all(cell == player for cell in row):
            return True
    
    # columns
    for col in range(3):
        if all(board[row][col] == player for row in range(3)):
            return True
    
    # diagonals
    if all(board[i][i] == player for i in range(3)):
        return True
    if all(board[i][2-i] == player for i in range(3)):
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


def evaluate(board):
    if is_winner(board, -1):  # komputer wygrywa
        return 10
    elif is_winner(board, 1):  # gracz wygrywa
        return -10
    else:
        return 0


def alpha_beta(board, depth, alpha, beta, is_maximizing):
    score = evaluate(board)
    
    if score == 10:
        return score - depth  # preferuj szybszą wygraną
    if score == -10:
        return score + depth  # unikaj szybiej przegranej
    if is_board_full(board):
        return 0
    
    available_moves = get_available_moves(board)
    
    if is_maximizing:  # Computer
        max_score = -1000
        for move in available_moves:
            row, col = move
            board[row][col] = -1
            
            score = alpha_beta(board, depth + 1, alpha, beta, False)
            max_score = max(score, max_score)
            
            board[row][col] = 0
            
            alpha = max(alpha, score)
            if beta <= alpha: # bez tego to zwyczajny minimax, 
                break
        
        return max_score
    
    else:  # Player
        min_score = 1000
        for move in available_moves:
            row, col = move
            board[row][col] = 1
            
            score = alpha_beta(board, depth + 1, alpha, beta, True)
            min_score = min(score, min_score)
            
            board[row][col] = 0
            
            beta = min(beta, score)
            if beta <= alpha:
                break
        
        return min_score


def find_best_move(board):
    best_score = -1000
    best_move = None
    available_moves = get_available_moves(board)
    
    for move in available_moves:
        row, col = move
        board[row][col] = -1
        
        score = alpha_beta(board, 0, -1000, 1000, False)
        
        board[row][col] = 0
        
        if score > best_score:
            best_score = score
            best_move = move
    
    return best_move


def play_game():
    board = [
        [0, 0, 0],
        [0, 0, 0],
        [0, 0, 0]
    ]
    
    print("Tic-Tac-Toe z Alpha-Beta")
    print("Ty: X | Komputer: O")
    print("Wpisz: 11, 12, 13, 21, 22, 23, 31, 32, 33")
    print()
    
    while True:
        print_board(board)
        print()
        
        while True:
            try:
                pos = input("Twój ruch (11-33): ").strip()
                if len(pos) != 2 or not pos.isdigit():
                    print("Zły format. Użyj: 11, 12, 13, itp.")
                    continue
                row = int(pos[0]) - 1
                col = int(pos[1]) - 1
                if row < 0 or row > 2 or col < 0 or col > 2:
                    print("Współrzędne muszą być 1-3")
                    continue
                if board[row][col] != 0:
                    print("To pole jest już zajęte")
                    continue
                board[row][col] = 1
                break
            except Exception as e:
                print(f"Błąd: {e}")
        
        if is_winner(board, 1):
            print_board(board)
            print("Wygrałeś")
            break
        if is_board_full(board):
            print_board(board)
            print("Remis")
            break
        
        # komputer
        move = find_best_move(board)
        if move:
            row, col = move
            board[row][col] = -1
        
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
