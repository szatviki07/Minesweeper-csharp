using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aknakereso.Persistence
{
    //Aknakeresõ játéktábla típusa
    public class AknakeresoTable
    {
        #region Fields

        private int _size; //tábla mérete
        private int[,] _fieldValues; //mezõértékek (-1 = akna, 0-8 = szomszédos aknák száma)
        private bool[,] _isRevealed; //felfedett mezõ-e
        private bool[,] _isFlagged;  //zászlóval jelölt mezõ-e

        #endregion

        #region Properties

        //tábla mérete
        public int Size { get { return _size; } }

        //játék vége állapot, minden mezö felfedett-e
        public bool IsGameOver { get; }

        //aktuális játékos
        public int CurrentPlayer { get; set; } = 1; // Player 1 kezd

        //indexer
        public int this[int x, int y]
        {
            get { return GetValue(x,y); }
        }

        #endregion

        #region Constructors

        //Aknakeresõ játéktábla példányosítása
        public AknakeresoTable() : this(10) { }  //alapból 10x10-es tábla

        public AknakeresoTable(int size)
        {
            if (size == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size), "A tabla merete nem lehet kisebb mint 0!");
            }
            _size = size;
            _fieldValues = new int[_size, _size];
            _isRevealed = new bool[_size, _size];
            _isFlagged = new bool[_size, _size];
            IsGameOver = false;
        }

        #endregion

        #region Public methods
        
        //Mező értékének lekérdezése
        public int GetValue(int x, int y)
        {
            if (x < 0 || x >= _size)
                throw new ArgumentOutOfRangeException(nameof(x), "Az X koordinata kivul esik a tabla mereten.");
            if (y < 0 || y >= _size)
                throw new ArgumentOutOfRangeException(nameof(y), "Az Y koordinata kivul esik a tabla mereten.");

            return _fieldValues[x, y];
        }
        
        //Mező értékének beállítása
        public void SetValue(int x, int y, int value)
        {
            if (x < 0 || x >= _size)
                throw new ArgumentOutOfRangeException(nameof(x), "Az X koordinata kivul esik a tabla mereten.");
            if (y < 0 || y >= _size)
                throw new ArgumentOutOfRangeException(nameof(y), "Az Y koordinata kivul esik a tabla mereten.");
            if (value < -1 || value > 8)
                throw new ArgumentOutOfRangeException(nameof(value), "A megadott ertek nem ertelmezett.");

            _fieldValues[x, y] = value;
        }
        
        //Mezõ felfedettségének ellenörzése
        public bool IsRevealed(int x, int y)
        {
            if (x < 0 || x >= _size)
                throw new ArgumentOutOfRangeException(nameof(x), "Az X koordinata kivul esik a tabla mereten.");
            if (y < 0 || y >= _size)
                throw new ArgumentOutOfRangeException(nameof(y), "Az Y koordinata kivul esik a tabla mereten.");
            return _isRevealed[x, y];
        }

        //Mezõ felfedettségének beállítása
        public void SetRevealed(int x, int y, bool revealed)
        {
            if (x < 0 || x >= _size)
                throw new ArgumentOutOfRangeException(nameof(x), "Az X koordinata kivul esik a tabla mereten.");
            if (y < 0 || y >= _size)
                throw new ArgumentOutOfRangeException(nameof(y), "Az Y koordinata kivul esik a tabla mereten.");
            _isRevealed[x, y] = revealed;

        }

        //A mezõ zászlóval megjelölt-e
        public bool IsFlagged(int x, int y)
        {
            if (x < 0 || x >= _size)
                throw new ArgumentOutOfRangeException(nameof(x), "Az X koordinata kivul esik a tabla mereten.");
            if (y < 0 || y >= _size)
                throw new ArgumentOutOfRangeException(nameof(y), "Az Y koordinata kivul esik a tabla mereten.");
            return _isFlagged[x, y];
        }

        //A mezõ zászlóval megjelölése vagy eltávolítása
        public void SetFlagged(int x, int y, bool flagged)
        {
            if (x < 0 || x >= _size)
                throw new ArgumentOutOfRangeException(nameof(x), "Az X koordinata kivul esik a tabla mereten.");
            if (y < 0 || y >= _size)
                throw new ArgumentOutOfRangeException(nameof(y), "Az Y koordinata kivul esik a tabla mereten.");
            _isFlagged[x, y] = flagged;
        }

        // segédmetódus, hogy ne érhessük el közvetlenül a table-t, hanem csak egy másolatát
        public AknakeresoTable Clone()
        {
            AknakeresoTable copy = new AknakeresoTable(this.Size);
            for (int x = 0; x < this.Size; x++)
            {
                for (int y = 0; y < this.Size; y++)
                {
                    copy.SetValue(x, y, this[x, y]);
                    copy.SetFlagged(x, y, this.IsFlagged(x, y));
                    copy.SetRevealed(x, y, this.IsRevealed(x, y));
                }
            }
            copy.CurrentPlayer = this.CurrentPlayer;
            return copy;


        }

        #endregion

    }
}
