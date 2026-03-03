using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aknakereso.Model
{
    // Aknakeresõ mező eseményargumentum típusa.
    public class AknakeresoFieldEventArgs : EventArgs
    {
        private int _x;
        private int _y;
        private int _newValue;
        private bool _isRevealed;

        // Aknakeresõ mező eseményargumentum példányosítása
        public AknakeresoFieldEventArgs(int x, int y, int newValue, bool isRevealed)
        {
            _x = x;
            _y = y;
            _newValue = newValue;
            _isRevealed = isRevealed;
        }

        // Megváltozott mező X koordinátájának lekérdezése
        public int X { get { return _x; } }

        // Megváltozott mező Y koordinátájának lekérdezése
        public int Y { get { return _y; } }

        // Megváltozott mező új értékének lekérdezése
        public int NewValue { get { return _newValue; } }

        // Megváltozott mező felfedettségének lekérdezése
        public bool IsRevealed { get { return _isRevealed; } }

    }
}
