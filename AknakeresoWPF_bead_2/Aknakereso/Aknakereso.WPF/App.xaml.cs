using System.Configuration;
using System.Data;
using System;
using System.Windows;
using Microsoft.Win32;
using Aknakereso.Model;
using Aknakereso.Persistence;
using Aknakereso.WPF.ViewModel;
using Aknakereso.WPF.View;

namespace Aknakereso.WPF
{
    public partial class App : Application
    {
        #region Fields

        private AknakeresoGameModel _model = null!;
        private AknakeresoViewModel _viewModel = null!;
        private MainWindow _view = null!;

        #endregion

        #region Constructors

        // Alkalmazás példányosítása.
        public App()
        {
            Startup += new StartupEventHandler(App_Startup);
        }

        #endregion

        #region Application event handlers

        private void App_Startup(object? sender, StartupEventArgs e)
        {
            // Modell létrehozása
            _model = new AknakeresoGameModel(new AknakeresoFileDataAccess());
            _model.GameOver += new EventHandler<AknakeresoEventArgs>(Model_GameOver);

            // Nézetmodell létrehozása
            _viewModel = new AknakeresoViewModel(_model);
            
            // Események összekötése
            _viewModel.SaveGame += new EventHandler(ViewModel_SaveGame);
            _viewModel.NewGame += new EventHandler(ViewModel_NewGame);
            _viewModel.LoadGame += new EventHandler(ViewModel_LoadGame);

            // Nézet létrehozása
            _view = new MainWindow();
            _view.DataContext = _viewModel;

            // Nézet megjelenítése
            _view.Show();
        }

        #endregion

        #region ViewModel event handlers

        // Új játék indításának eseménykezelője.
        private void ViewModel_NewGame(object? sender, EventArgs e)
        {
            _model.NewGame(_model.TableSize);
        }

        // Játék betöltésének eseménykezelője.
        private async void ViewModel_SaveGame(object? sender, EventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Title = "Aknakereső játék mentése";
                saveFileDialog.Filter = "Aknakereső állás|*.akn";

                if (saveFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        await _model.SaveGameAsync(saveFileDialog.FileName);

                        MessageBox.Show("A játék sikeresen elmentve!",
                                        "Aknakereső",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Information);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Játék mentése sikertelen!" + Environment.NewLine +
                                        "Hibás az elérési út, vagy a fájl nem írható.",
                                        "Hiba!",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Mentés sikertelen!\n" + ex.Message,
                                "Hiba",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private async void ViewModel_LoadGame(object? sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "Aknakereső játék betöltése";
                openFileDialog.Filter = "Aknakereső állás|*.akn";

                if (openFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        await _model.LoadGameAsync(openFileDialog.FileName);

                        // a nezet szinkronizalasa
                        _viewModel.RebuildFromModel();

                        MessageBox.Show("A játék sikeresen betöltve!",
                                        "Aknakereső",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Information);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("A játék betöltése sikertelen!",
                                        "Aknakereső",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Betöltés sikertelen!\n" + ex.Message,
                                "Hiba",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        #endregion

        #region Model event handlers

        // Játék végének eseménykezelője.
        private void Model_GameOver(object? sender, AknakeresoEventArgs e)
        {
            string message = e.IsWon()
                ? $"Gratulálok, győztes: Játékos {e.Winner()}!"
                : $"💣 Sajnos vesztettél. A győztes: Játékos {e.Winner()}";

            MessageBox.Show(message, "Aknakereső játék vége", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion
    }
}
