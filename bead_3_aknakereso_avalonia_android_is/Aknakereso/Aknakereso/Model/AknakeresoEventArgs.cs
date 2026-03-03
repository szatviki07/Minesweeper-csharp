using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aknakereso.Model
{
    // Aknakeresõ eseményargumentum típusa.

    public class AknakeresoEventArgs : EventArgs
    {
        private bool _isWon;
        private int _winner;

        // Aknakeresõ eseményargumentum példányosítása
        public AknakeresoEventArgs(bool isWon, int winner)
        {
            _isWon = isWon;
            _winner = winner;
        }

        // Győztes lekérdezése
        public int Winner()
        {
            return _winner;
        }

        public bool IsWon()
        {
            return _isWon;
        }
    }
}
