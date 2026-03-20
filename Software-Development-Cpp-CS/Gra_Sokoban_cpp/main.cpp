#include <iostream>
#include <fstream>
#include <cstdlib>

using namespace std;

#define MAX_ROWS 12
#define MAX_COLS 19

void setBoardFromFile(int input, int *moves, int *playerRow, int *playerCol, char board[][MAX_COLS])
{
    if(input == 1){
        ifstream file("1.txt");
        *playerRow = 3;
        *playerCol = 3;
        *moves = 30;

        for (int row = 0; row < MAX_ROWS; row++)
        {
            for (int col = 0; col < MAX_COLS; col++)
            {
                file >> board[row][col];
            }
            cout << endl;
        }
        file.close();
    }

    if(input == 2){
        ifstream file("2.txt");
        *playerRow = 4;
        *playerCol = 4;
        *moves = 70;

        for (int row = 0; row < MAX_ROWS; row++)
        {
            for (int col = 0; col < MAX_COLS; col++)
            {
                file >> board[row][col];
            }
        }
        file.close();
    }

    if(input == 3){
        ifstream file("3.txt");
        *playerRow = 8;
        *playerCol = 11;
        *moves = 120;

        for (int row = 0; row < MAX_ROWS; row++)
        {
            for (int col = 0; col < MAX_COLS; col++)
            {
                file >> board[row][col];
            }
        }

        file.close();
    }

}

void menu(int *level, int *moves, int *win, int *win1, int *win2, int *n, int *playerRow, int *playerCol, char board[][MAX_COLS]){
    while(true){
        system("cls");
        *win = 0;
        *n = 0;

        cout << "Mateusz Wizner s277508" << endl;
        cout <<" _____      _         _                 "<< endl;
        cout <<"/  ___|    | |       | |                "<< endl;
        cout <<"\\ `--.  ___| | _____ | |__   __ _ _ __  "<< endl;
        cout <<"`--.  \\/ _\\| |/  / _\\| '_ \\/ _` | '_ \\"<< endl;
        cout <<"/\\__/ / (_)|    < (_)| |_) | (_|| | | |"<< endl;
        cout <<"\\____/\\___/|_|\\_\\___/|_.__/\\__,_|_| |_|"<< endl << endl;             
        cout << "Choose your level:" << endl;
        cout << " ------------------ " << endl;    
        cout << "|--> 1             |" << endl;
        if(*win1 != 0)
            cout << "|--> 2             |" << endl;
        if(*win1 != 0 && *win2 !=0)
            cout << "|--> 3             |" << endl;
        cout << " ------------------ " << endl;
        cout << "or EXIT by typing e" << endl;
        int input;
        cin >> input;
        *level = input;
        if(input == 1){
            setBoardFromFile(input, moves, playerRow, playerCol, board);
            break;
        }
        if(input == 2 && *win1 != 0){
            setBoardFromFile(input, moves, playerRow, playerCol, board);
            break;
        }
        if(input == 3 && *win2 != 0 && *win1 != 0){
            setBoardFromFile(input, moves, playerRow, playerCol, board);
            break;
        }
        else if(input > 1) continue;
        else{
            exit(0);
        }
    }
}

void movePlayer(int row, int col, char input, int *moves, int *playerRow, int *playerCol, char board [][MAX_COLS])
{
    if (row < 0 || row >= MAX_ROWS || col < 0 || col >= MAX_COLS)
    {
        return;
    }

    else if (board[row][col] == '#' || board[row][col] == '%')
    {  
        return;
    }

    *moves -= 1;

    if (board[row][col] == 'C'){
        if(input == 'w' || input == 'W'){
            if(board[row-1][col] == '#' || board[row-1][col] == 'C' || board[row-1][col] == '%'){
                (*moves)++;
                return;
            }
            if(board[row-1][col] == 'P'){         
                board[row-1][col] = '%';
                board[row][col] = '_';
                *playerRow = row;
                *playerCol = col;
            }
            else{
                board[row-1][col] = 'C';
                board[row][col] = '_';
                *playerRow = row;
                *playerCol = col;
            }
        }
        else if(input == 'a' || input == 'A'){
            if(board[row][col-1] == '#' || board[row][col-1] == 'C' || board[row][col-1] == '%'){
                (*moves) ++;
                return;
            }
            if(board[row][col-1] == 'P'){         
                board[row][col-1] = '%';
                board[row][col] = '_';
                *playerRow = row;
                *playerCol = col;
            }
            else{
                board[row][col-1] = 'C';
                board[row][col] = '_';
                *playerRow = row;
                *playerCol = col;
            }
        }
        else if(input == 's' || input == 'S'){
            if(board[row+1][col] == '#' || board[row+1][col] == 'C' || board[row+1][col] == '%'){
                (*moves) ++;
                return;
            }
            if(board[row+1][col] == 'P'){         
                board[row+1][col] = '%';
                board[row][col] = '_';
                *playerRow = row;
                *playerCol = col;
            }
            else{
                board[row+1][col] = 'C';
                board[row][col] = '_';
                *playerRow = row;
                *playerCol = col;
            }
        }
        else if(input == 'd' || input == 'D'){
            if(board[row][col+1] == '#' || board[row][col+1] == 'C' || board[row][col+1] == '%'){
                (*moves)++;
                return;
            }
            if(board[row][col+1] == 'P'){         
                board[row][col+1] = '%';
                board[row][col] = '_';
                *playerRow = row;
                *playerCol = col;
            }
            else{
                board[row][col+1] = 'C';
                board[row][col] = '_';
                *playerRow = row;
                *playerCol = col;
            }
        }
    }

    else{
        *playerRow = row;
        *playerCol = col;
    }
}

void updateGame(int *level, int *moves, int *win, int *win1, int *win2, int *n, int *playerRow, int *playerCol, char board[][MAX_COLS], char previous_move[])
{
    char input;
    cin >> input;

    if(input != 'q' && input != 'Q' && input != 'u' && input != 'U' && input != 'i' && input != 'I' && input != 'o' && input != 'O'){
        previous_move[*n] = input;
        (*n)++;
    }

    if (input == 'w' || input == 'W')
    {
        movePlayer(*playerRow - 1, *playerCol, input, moves, playerRow, playerCol, board);
    }
    else if (input == 'a' || input == 'A')
    {
        movePlayer(*playerRow, *playerCol - 1, input, moves, playerRow, playerCol, board);
    }
    else if (input == 's' || input == 'S')
    {
        movePlayer(*playerRow + 1, *playerCol, input, moves, playerRow, playerCol, board);
    }
    else if (input == 'd' || input == 'D')
    {
        movePlayer(*playerRow, *playerCol + 1, input, moves, playerRow, playerCol, board);
    }
    else if (input == 'u' || input == 'U'){
        if(previous_move[*n] == 'w' || previous_move[*n] == 'W') movePlayer(*playerRow + 1, *playerCol, input, moves, playerRow, playerCol, board);
        else if(previous_move[*n] == 'a' || previous_move[*n] == 'A') movePlayer(*playerRow, *playerCol + 1, input, moves, playerRow, playerCol, board);
        else if(previous_move[*n] == 's' || previous_move[*n] == 'S') movePlayer(*playerRow - 1, *playerCol, input, moves, playerRow, playerCol, board);
        else if(previous_move[*n] == 'd' || previous_move[*n] == 'D') movePlayer(*playerRow, *playerCol - 1, input, moves, playerRow, playerCol, board);

        previous_move[*n] = 0;

        if(previous_move[*n] != 'a' && previous_move[*n] != 'A' && previous_move[*n] != 'w' && previous_move[*n] != 'W' && 
        previous_move[*n] != 's' && previous_move[*n] != 'S' && previous_move[*n] != 'd' && previous_move[*n] != 'D' && previous_move[*n] != 0) n++;

        if(*n<0) *n = 0;
        else *n -= 1;

        if(*n != 0) *moves +=2;
    }

    else if (input == 'q' || input == 'Q')
    {
        if(*win > 0){
            if(*level == 1) *win1=1;
            else if(*level == 2) *win2=1;          
        }
        for(int i=0; i<100; i++) previous_move[i] = '0';
        menu(level, moves, win, win1, win2, n, playerRow, playerCol, board);
    }

    else if (input == 'i' || input == 'I'){
        setBoardFromFile(*level, moves, playerRow, playerCol, board);
    }

    else if (input == 'o' || input == 'O'){
        exit(0);
    }
}

void drawGame(int *level, int *moves, int *win, int *playerRow, int *playerCol, char board[][MAX_COLS])
{
    system("cls");
    int towin = 0;
    
    for (int row = 0; row < MAX_ROWS; row++)
    {
        for (int col = 0; col < MAX_COLS; col++)
        {
            if(board[row][col] == 'P') towin++;
            if (row == *playerRow && col == *playerCol)
            {
                if(*win > 0) cout << "B" << "B";
                else if(*moves > 0) cout << "a" << "a";
                else cout << "A" << "A";
            }
            else
            {
                if(board[row][col]=='_') cout << char(176) << char(176);
                else if(board[row][col] == '#') cout << char(219) << char(219);
                else if(board[row][col] == 'C') cout << char(254) << char(254);
                else if(board[row][col] == 'P') cout << char(206) << char(206);
                else if(board[row][col] == '%') cout << char(197) << char(197);
                else if(board[row][col] == '-') cout << " " << " ";
            }
        }
        cout << endl;
    }
    if(towin == 0){
        cout << "You Win" << endl;
        *win=1;   
    }

    else if(*moves <= 0){
        system("cls");
        setBoardFromFile(*level, moves, playerRow, playerCol, board);
        cout << "You lose" << endl;
    }
    else{
        cout << "left: " << *moves << " moves" << endl;
        cout << "left: " << towin << " chests" << endl;
    }
    cout << " ------------------------- " << endl;
    cout << "| back to menu: press q/Q |" << endl; 
    cout << "| reset: press i/I        |" << endl;
    cout << "| to exit game: press o/O |" << endl;
    cout << " ------------------------- " << endl;
}

int main()
{
    char board[MAX_ROWS][MAX_COLS];
    char previous_move[100];
    for(int i=0; i<100; i++) previous_move[i] = '0';
    int playerRow;
    int playerCol;
    int level;
    int moves;
    int win = 0;
    int win1 = 0;
    int win2 = 0;
    int n = 0;
    menu(&level, &moves, &win, &win1, &win2, &n, &playerRow, &playerCol, board);

    while (true)
    {
        drawGame(&level, &moves, &win, &playerRow, &playerCol, board);
        updateGame(&level, &moves, &win, &win1, &win2, &n, &playerRow, &playerCol, board, previous_move);
    }

    return 0;
}