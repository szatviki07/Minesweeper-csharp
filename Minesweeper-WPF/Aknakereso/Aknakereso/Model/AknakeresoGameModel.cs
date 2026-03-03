using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aknakereso.Persistence;

namespace Aknakereso.Model
{
    // Aknakeresõ játék típusa
    public class AknakeresoGameModel
    {
        #region Fields

        private IAknakeresoDataAccess _dataAccess; // adatelérés
        private AknakeresoTable _table; // játéktábla
        private int _currentPlayer; // aktuális játékos
        private bool _isGameOver; // vége-e a játéknak

        #endregion

        #region Properties

        //játéktábla méretének lekérdezése
        public int TableSize { get { return _table.Size; } }

        //aktuális játékos
        public int CurrentPlayer { get { return _currentPlayer; } }

        //vége-e a játéknak
        public bool IsGameOver { get { return _isGameOver; } }

        //indexer
        public int this[int x, int y] => _table[x, y];

        /*
        public AknakeresoTable Table
        {
            get { return _table; }
        }
        */
        //helyette inkább Clone-nal oldjuk meg, hogy ne érjük el közvetlenül
        public AknakeresoTable Table
        {
            get { return _table.Clone(); }
        }


        #endregion

        #region Events

        // Mező megváltozásának eseménye.
        public event EventHandler<AknakeresoFieldEventArgs>? FieldChanged;

        // Játék végének eseménye.
        public event EventHandler<AknakeresoEventArgs>? GameOver;

        #endregion

        #region Constructor

        // Aknakeresõ játék példányosítása
        public AknakeresoGameModel(IAknakeresoDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
            _table = new AknakeresoTable();
            _currentPlayer = 1;
            _isGameOver = false;
        }

        #endregion

        #region Public methods

        //Új játék kezdése
        public void NewGame(int size)
        {
            _table = new AknakeresoTable(size);
            _currentPlayer = 1;
            _isGameOver = false;

            GenerateMines(size);
            CalculateMines();

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    OnFieldChanged(x, y);
        }

        //Mező felfedése
        public void Reveal(int x, int y)
        {
            // játék vége / mező felfedett / mező zászlózott - nem csinál semmit
            if (_isGameOver == true)
            {
                return;
            }
            if (_table.IsRevealed(x, y) || _table.IsFlagged(x, y))
            {
                return;
            }

            // felfedjük a mezőt
            _table.SetRevealed(x, y, true);
            OnFieldChanged(x, y);

            // -1 jelöli az aknát
            if (_table[x, y] ==  -1)
            {
                _isGameOver = true;
                OnGameOver();
                return;
            }

            // rekurzív felfedés a szomszédokra, ha nincs akna
            if (_table[x,y] == 0)
            {
                RevealNeighbours(x, y);
            }

            // Hogy végelegyen a játéknak, ha már minden akna fel van szedve
            if (AllSafeFieldsRevealed() == true)
            {
                _isGameOver = true;
                OnGameOver(true, _currentPlayer);
                return;
            }

            // következő játékos jön
            SwitchPlayer();
        }

        // teszteknél használjuk
        public bool IsRevealed(int x, int y)
        {
            return _table.IsRevealed(x, y);
        }

        //Zászló elhelyezése / törlése
        public void Flag(int x, int y)
        {
            if (_isGameOver) return;

            // felfedett mezõt nem zászlózhatunk
            if (_table.IsRevealed(x, y)) return;

            // ha már van zászló, törli
            bool currentFlag = _table.IsFlagged(x, y);
            _table.SetFlagged(x, y, !currentFlag);

            // frissíti a mezőt
            OnFieldChanged(x, y);
        }

        // Hogy a View lekérdezhesse az állapotát
        public bool IsFlagged(int x, int y)
        {
            return _table.IsFlagged(x, y);
        }

        public async Task LoadGameAsync(string path)
        {
            if (_dataAccess == null)
                throw new InvalidOperationException("Az adatelérés nincs biztosítva.");

            // Betöltjük a táblát
            _table = await _dataAccess.LoadAsync(path);

            // UI frissítése
            for (int x = 0; x < _table.Size; x++)
            {
                for (int y = 0; y < _table.Size; y++)
                {
                    if (_table.IsRevealed(x, y) || _table.IsFlagged(x, y))
                    {
                        // hogy tényleges változzanak a mezõk a betöltés után
                        OnFieldChanged(x, y);
                    }
                }
            }

            // mentett aktuális játékos
            _currentPlayer = _table.CurrentPlayer;

            // Játék vége-e
            _isGameOver = AllSafeFieldsRevealed();
        }

        // Játék mentése
        public async Task SaveGameAsync(String path)
        {
            if (_dataAccess == null)
                throw new InvalidOperationException("Az adatelérés nincs biztosítva.");

            await _dataAccess.SaveAsync(path, _table);
        }

        #endregion

        #region Private methods

        // Játékos váltása
        private void SwitchPlayer()
        {
            if (_currentPlayer == 1)
            {
                _currentPlayer = 2;
            }
            else
            {
                _currentPlayer = 1;
            }
            _table.CurrentPlayer = _currentPlayer; // hogy a táblában is tároljuk (save és load-hoz kell)
        }

        // Szomszédos mezõk rekurzív felfedése, amíg nincs szomszédos akna (=0)
        private void RevealNeighbours(int x, int y)
        {
            int size = _table.Size;
            int[,] neighbours = { { -1, -1 }, { -1, 0 }, { 0, -1 }, { -1, 1 }, { 1, -1 }, { 1, 0 }, { 0, 1 }, { 1, 1 } };

            for (int i = 0; i < neighbours.GetLength(0); i++)
            {
                // végignézzük a szomszédokat
                int nx = x + neighbours[i, 0];
                int ny = y + neighbours[i, 1];

                // kiment-e a pályáról
                if (nx < 0 || ny < 0 || nx >= size || ny >= size)
                {
                    continue;
                }

                // ha már felfedett vagy zászlózott, nem változtatjuk
                if (_table.IsRevealed(nx, ny) || _table.IsFlagged(nx, ny))
                {
                    continue;
                }

                _table.SetRevealed(nx, ny, true);
                OnFieldChanged(nx, ny);

                // rekurzív hívás
                if (_table[nx, ny] == 0)
                {
                    RevealNeighbours(nx, ny);
                }
            }

        }

        // Aknák generálása vétlenszerűen
        private void GenerateMines(int size)
        {
            Random rand = new Random();
            int allMines = 0;
            int mines = 0;

            //altlános szabály Aknakeresõben:
            //6x6-s pálya -> 6 akna
            //10x10-es pálya -> 15 akna
            //15x15-ös pálya -> 40 akna 
            
            if (size == 6)
            {
                allMines = 6;
            }
            else if (size == 10)
            {
                allMines = 15;
            }
            else if (size == 16)
            {
                allMines = 40;
            }

            while (mines < allMines)
            {
                // random 0 és size-1 közötti szám
                int x = rand.Next(size);
                int y = rand.Next(size);

                // van-e már akna
                if (_table[x, y] == -1)
                {
                    continue;
                }

                _table.SetValue(x, y, -1);
                mines++;
            }

        }

        // A mezõk értékeinek kiszámítása ((-1)-(8) aknák alapján)
        private void CalculateMines()
        {
            int size = _table.Size;
            int[,] neighbours = { { -1, -1 }, { -1, 0 }, { 0, -1 }, { -1, 1 }, { 1, -1 }, { 1, 0 }, { 0, 1 }, { 1, 1 } };

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    // ha akna, nem számoljuk hány van körülötte
                    if (_table[x, y] == -1)
                    {
                        continue;
                    }

                    int mines = 0;

                    for (int i = 0; i < neighbours.GetLength(0); i++)
                    {
                        int nx = x + neighbours[i, 0];
                        int ny = y + neighbours[i, 1];

                        if (nx < 0 || ny < 0 || nx >= size || ny >= size)
                        {
                            continue;
                        }

                        if (_table[nx, ny] == -1)
                            mines++;
                    }

                    _table.SetValue(x, y, mines);
                }
            }
        }

        private bool AllSafeFieldsRevealed()
        {
            for (int x = 0; x < _table.Size; x++)
            {
                for (int y = 0; y < _table.Size; y++)
                {
                    if (_table[x,y] != -1 && !_table.IsRevealed(x, y))
                        return false; // még van nem akna, felfedetlen mezõ
                }
            }
            return true; // különben játék vége
        }


        #endregion

        #region Private event methods

        // Mező változás eseményének kiváltása
        private void OnFieldChanged(int x, int y)
        {
            int value = _table[x, y];
            bool revealed = _table.IsRevealed(x, y);
            FieldChanged?.Invoke(this, new AknakeresoFieldEventArgs(x, y, value, revealed));
        }

        // Játék vége eseményének kiváltása
        private void OnGameOver(bool isWon, int winner)
        {
            _isGameOver = true;
            GameOver?.Invoke(this, new AknakeresoEventArgs(isWon, winner));
        }

        // Játék vége eseményének kiváltása - ha valaki aknára lépett
        private void OnGameOver()
        {
            // ha épp az 1-es játékos játszott -> a 2-es nyer, és fordítva
            int winner = (_currentPlayer == 1) ? 2 : 1;
            OnGameOver(false, winner);
        }

        #endregion
    }
}
