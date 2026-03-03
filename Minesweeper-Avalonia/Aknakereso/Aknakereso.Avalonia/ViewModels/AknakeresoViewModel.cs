using Aknakereso.Model;
using Aknakereso.Persistence;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Aknakereso.Avalonia.ViewModels
{
    // Sudoku nézetmodell típusa.
    public class AknakeresoViewModel : ViewModelBase
    {
        #region Fields

        private AknakeresoGameModel _model;

        #endregion

        #region Properties

        public RelayCommand NewGameCommand { get; private set; }
        public RelayCommand LoadGameCommand {  get; private set; }
        public RelayCommand SaveGameCommand { get; private set; }
        public RelayCommand SmallBoardCommand {  get; private set; }
        public RelayCommand MediumBoardCommand { get; private set; }
        public RelayCommand LargeBoardCommand { get; private set; }

        public ObservableCollection<AknakeresoField> Fields { get; set; }

        public int TableSize => _model.TableSize;
        public string CurrentPlayer => $"Jatekos {_model.CurrentPlayer} kovetkezik";

        #endregion

        #region Events

        public event EventHandler? NewGame;
        public event EventHandler? LoadGame;
        public event EventHandler? SaveGame;

        #endregion

        #region Construcotrs

        public AknakeresoViewModel(AknakeresoGameModel model)
        {
            _model = model;
            _model.FieldChanged += Model_FieldChanged;

            NewGameCommand = new RelayCommand(() => OnNewGame());
            LoadGameCommand = new RelayCommand(() => OnLoadGame());
            SaveGameCommand = new RelayCommand(() => OnSaveGame());

            SmallBoardCommand = new RelayCommand(() => StartNewGame(6));
            MediumBoardCommand = new RelayCommand(() => StartNewGame(10));
            LargeBoardCommand = new RelayCommand(() => StartNewGame(16));

            Fields = new ObservableCollection<AknakeresoField>();

            StartNewGame(10);
        }

        public AknakeresoViewModel() : this(new AknakeresoGameModel(new AknakeresoFileDataAccess()))
        {
        }

        #endregion

        #region Private methods

        private void StartNewGame(int size)
        {
            _model.NewGame(size);
            InitializeFields();
            OnPropertyChanged(nameof(CurrentPlayer));
            OnPropertyChanged(nameof(TableSize));
        }

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
                    };

                    field.RevealCommand = new RelayCommand(() => _model.Reveal(xx, yy));
                    field.FlagCommand = new RelayCommand(() => _model.Flag(xx, yy));

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

        #region Game event handlers

        private void Model_FieldChanged(object? sender, AknakeresoFieldEventArgs e)
        {
            AknakeresoField? field = null;

            foreach (AknakeresoField f in Fields)
            {
                if (f.X == e.X && f.Y == e.Y)
                {
                    field = f;
                    break;
                }
            }
            // csak egyszerubben, sudoku minta
            // var field = Fields.FirstOrDefault(f => f.X == e.X && f.Y == e.Y);

            if (field == null) return;

            field.IsRevealed = e.IsRevealed;
            field.IsFlagged = _model.IsFlagged(e.X, e.Y);
            field.Text = GetTextForField(e.X, e.Y);

            OnPropertyChanged(nameof(CurrentPlayer));
        }

        #endregion

        #region Public methods

        public void RebuildFromModel()
        {
            InitializeFields();
            OnPropertyChanged(nameof(TableSize));
            OnPropertyChanged(nameof(CurrentPlayer));
        }

        #endregion

        #region Event methods

        private void OnNewGame()
        {
            NewGame?.Invoke(this, EventArgs.Empty);
        }

        private void OnLoadGame()
        {
            LoadGame?.Invoke(this, EventArgs.Empty);
        }

        private void OnSaveGame()
        {
            SaveGame?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}