using Aknakereso.Model;
using Aknakereso.Persistence;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aknakereso.WPF.ViewModel
{
    // Aknakeresõ nézetmodell típusa.
    public class AknakeresoViewModel : ViewModelBase
    {
        #region Fields

        private AknakeresoGameModel _model;

        #endregion

        #region Properties

        // Új játék parancs lekérdezése.
        public DelegateCommand NewGameCommand { get; private set; }

        // Játék betöltése parancs lekérdezése.
        public DelegateCommand LoadGameCommand { get; private set; }

        // Játék mentése parancs lekérdezése.
        public DelegateCommand SaveGameCommand { get; private set; }

        // Kis méretű pálya (6x6) parancs.
        public DelegateCommand SmallBoardCommand { get; private set; }

        // Közepes méretű pálya (10x10) parancs.
        public DelegateCommand MediumBoardCommand { get; private set; }

        // Nagy méretű pálya (16x16) parancs.
        public DelegateCommand LargeBoardCommand { get; private set; }

        // A játékmezők gyűjteménye.
        public ObservableCollection<AknakeresoField> Fields { get; private set; }

        // A tábla mérete.
        public int TableSize => _model.TableSize;

        // Az aktuális játékos megjelenítése.
        public string CurrentPlayer => $"Játékos {_model.CurrentPlayer} következik";

        #endregion

        #region Events

        // Új játék eseménye.
        public event EventHandler? NewGame;

        // Játék mentésének eseménye.
        public event EventHandler? SaveGame;

        // Játék betöltésének eseménye.
        public event EventHandler? LoadGame;

        #endregion

        #region Constructors

        // Nézetmodell példányosítása.
        public AknakeresoViewModel(AknakeresoGameModel model)
        {
            // játék csatlakoztatása
            _model = model;
            _model.FieldChanged += Model_FieldChanged;

            // parancsok kezelése
            SmallBoardCommand = new DelegateCommand(_ => StartNewGame(6));
            MediumBoardCommand = new DelegateCommand(_ => StartNewGame(10));
            LargeBoardCommand = new DelegateCommand(_ => StartNewGame(16));

            NewGameCommand = new DelegateCommand(_ => OnNewGame());
            SaveGameCommand = new DelegateCommand(_ => OnSaveGame());
            LoadGameCommand = new DelegateCommand(_ => OnLoadGame());

            Fields = new ObservableCollection<AknakeresoField>();

            StartNewGame(10); // alapértelmezett tábla
        }

        #endregion

        #region Private methods

        private void StartNewGame(int size)
        {
            _model.NewGame(size);
            InitializeFields();
            OnPropertyChanged(nameof(CurrentPlayer));
            OnPropertyChanged(nameof(TableSize));
            NewGame?.Invoke(this, EventArgs.Empty);
        }

        // A tábla mezőinek létrehozása.
        private void InitializeFields()
        {
            Fields.Clear();

            for (int x = 0; x < _model.TableSize; x++)
            {
                for (int y = 0; y < _model.TableSize; y++)
                {
                    int xx = x, yy = y;   // mentjuk a coordot

                    var field = new AknakeresoField
                    {
                        X = xx,
                        Y = yy,
                        Text = GetTextForField(xx, yy),
                        IsRevealed = _model.IsRevealed(xx, yy),
                        IsFlagged = _model.IsFlagged(xx, yy),
                        RevealCommand = new DelegateCommand(null, _ => _model.Reveal(xx, yy)),
                        FlagCommand = new DelegateCommand(null, _ => _model.Flag(xx, yy))
                    };
                    Fields.Add(field);
                }
            }
        }

        private string GetTextForField(int x, int y)
        {
            if (!_model.IsRevealed(x, y))
                return _model.IsFlagged(x, y) ? "🚩" : string.Empty;

            int value = _model[x, y];
            if (value == -1) return "💣";
            if (value == 0) return string.Empty;
            return value.ToString();
        }

        #endregion

        #region Public Methods

        // kell hogy ujrameretezze az ablakot pl betolteskor
        public void RebuildFromModel()
        {
            InitializeFields();
            OnPropertyChanged(nameof(TableSize));
            OnPropertyChanged(nameof(CurrentPlayer));
        }

        #endregion

        #region Game event handlers

        private void Model_FieldChanged(object? sender, AknakeresoFieldEventArgs e)
        {
            var field = Fields.FirstOrDefault(f => f.X == e.X && f.Y == e.Y);
            if (field == null) return;

            field.IsRevealed = e.IsRevealed;
            field.IsFlagged = _model.IsFlagged(e.X, e.Y);
            field.Text = GetTextForField(e.X, e.Y);

            OnPropertyChanged(nameof(CurrentPlayer));
        }

        #endregion

        #region Event methods

        // Új játék eseménykiváltása.
        private void OnNewGame() => NewGame?.Invoke(this, EventArgs.Empty);

        // Játék mentése eseménykiváltása.
        private void OnSaveGame()
        {
            SaveGame?.Invoke(this, EventArgs.Empty);
        }

        // Játék betöltése eseménykiváltása.
        private void OnLoadGame()
        {
            LoadGame?.Invoke(this, EventArgs.Empty);
        }

        #endregion

    }
}
