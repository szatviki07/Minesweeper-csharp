using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aknakereso.Avalonia.ViewModels
{
    // Sudoku játékmező típusa.
    public class AknakeresoField : ViewModelBase
    {
        private bool _isRevealed;
        private bool _isFlagged;
        private string _text = string.Empty;

        // Felfedettség lekérdezése, vagy beállítása.
        public bool IsRevealed
        {
            get { return _isRevealed; }
            set
            {
                if (_isRevealed != value)
                {
                    _isRevealed = value;
                    OnPropertyChanged();
                }
            }
        }

        // Zászlózottság lekérdezése, vagy beállítása.
        public bool IsFlagged
        {
            get { return _isFlagged; }
            set
            {
                if (_isFlagged != value)
                {
                    _isFlagged = value;
                    OnPropertyChanged();
                }
            }
        }

        // Felirat lekérdezése, vagy beállítása.
        public string Text
        {
            get { return _text; }
            set
            {
                if (_text != value)
                {
                    _text = value;
                    OnPropertyChanged();
                }
            }
        }

        // Vízszintes koordináta lekérdezése, vagy beállítása.
        public int X { get; set; }

        // Függőleges koordináta lekérdezése, vagy beállítása.
        public int Y { get; set; }

        // Koordináta lekérdezése.
        public Tuple<int, int> XY
        {
            get { return new(X, Y); }
        }

        // Felfedés parancs (bal kattintás).
        public RelayCommand? RevealCommand { get; set; }

        // Zászlózás parancs (jobb kattintás).
        public RelayCommand? FlagCommand { get; set; }

    }
}
