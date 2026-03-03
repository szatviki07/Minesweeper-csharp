using Aknakereso.Avalonia.ViewModels;
using Aknakereso.Avalonia.Views;
using Aknakereso.Model;
using Aknakereso.Persistence;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Chrome;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Aknakereso.Avalonia;

public partial class App : Application
{
    #region Fields

    private AknakeresoGameModel _model = null!;
    private AknakeresoViewModel _viewModel = null!;

    #endregion

    #region Properties

    private TopLevel? TopLevel
    {
        get
        {
            return ApplicationLifetime switch
            {
                IClassicDesktopStyleApplicationLifetime desktop => TopLevel.GetTopLevel(desktop.MainWindow),
                ISingleViewApplicationLifetime singleViewPlatform => TopLevel.GetTopLevel(singleViewPlatform.MainView),
                _ => null
            };
        }
    }

    #endregion

    #region Application methods

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // sudokubol
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        // tehat: Avalonia sajat hibakezelo pluginjat kikapcsoljuk (Toolkit miatt?)
        BindingPlugins.DataValidators.RemoveAt(0);

        _model = new AknakeresoGameModel(new AknakeresoFileDataAccess());
        _model.FieldChanged += (s, e) => { };  // a flaghez kell (legalabb 1 feliratokozo)
        _model.GameOver += Model_GameOver;

        _viewModel = new AknakeresoViewModel(_model);
        _viewModel.LoadGame += ViewModel_LoadGame;
        _viewModel.SaveGame += ViewModel_SaveGame;
        _viewModel.NewGame += (_, __) =>
        {
            _model.NewGame(10);
            _viewModel.RebuildFromModel();
        };

        // ablak letrehozasa
        // Desktop
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = _viewModel
            };

            desktop.Startup += async (_, __) =>
            {
                try
                {
                    await _model.LoadGameAsync(
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        "AknakeresoSuspendedGame"));
                }
                catch { }
            };

            desktop.Exit += async (_, __) =>
            {
                try
                {
                    await _model.SaveGameAsync(
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        "AknakeresoSuspendedGame"));
                }
                catch { }
            };
        }

        // Andorid
        else if (ApplicationLifetime is ISingleViewApplicationLifetime mobile)
        {
            mobile.MainView = new MainView
            {
                DataContext = _viewModel
            };

            string savePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "SuspendedGame.akn");


            if (Application.Current?.TryGetFeature<IActivatableLifetime>() is { } activatableLifetime)
            {
                // eddig csak backgroundbol mentett vissza
                activatableLifetime.Activated += async (sender, args) =>
                {
                    try
                    {
                        if (File.Exists(savePath))
                        {
                            using var stream = File.OpenRead(savePath);
                            await _model.LoadGameAsync(stream);
                            _viewModel.RebuildFromModel();
                        }
                    }
                    catch { }
                };

                activatableLifetime.Deactivated += async (sender, args) =>
                {
                    try
                    {
                        using var stream = File.Create(savePath);
                        await _model.SaveGameAsync(stream);
                    }
                    catch { }
                };
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    #endregion

    #region ViewModel event handler

    private async void ViewModel_LoadGame(object? sender, System.EventArgs e)
    {
        // ha nem desktop
        if (TopLevel == null)
        {
            await MessageBoxManager.GetMessageBoxStandard(
                "Aknakereso jatek",
                "A fajlkezeles nem tamogatott!",
                ButtonEnum.Ok, Icon.Error)
              .ShowAsync();
            return;
        }

        try
        {
            var files = await TopLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Aknakereso jatek betoltese",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("Aknakereso jatek fajl")
                    {
                        Patterns = new[] { "*.akn" }
                    }
                }
            });

            if (files.Count > 0)
            {
                using (var stream = await files[0].OpenReadAsync())
                {
                    await _model.LoadGameAsync(stream);
                }

                _viewModel.RebuildFromModel();
            }
        }
        catch (AknakeresoDataException)
        {
            await MessageBoxManager.GetMessageBoxStandard(
                "Aknakereso jatek",
                "A fajl betoltese sikertelen!",
                ButtonEnum.Ok, Icon.Error)
                .ShowAsync();
        }
    }

    private async void ViewModel_SaveGame(object? sender, EventArgs e)
    {
        if (TopLevel == null)
        {
            await MessageBoxManager.GetMessageBoxStandard(
                "Aknakereso jatek",
                "A fajlkerezeles nem tamogatott!",
                ButtonEnum.Ok, Icon.Error)
                .ShowAsync();
            return;
        }

        try
        {
            var file = await TopLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
            {
                Title = "Aknakereso jatek mentese",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("Aknakereso jatek fajl")
                    {
                        Patterns = new[] { "*.akn" }
                    }
                }
            });

            if (file != null)
            {
                using (var stream = await file.OpenWriteAsync())
                {
                    await _model.SaveGameAsync(stream);
                }
            }
        }
        catch (Exception ex)
        {
            await MessageBoxManager.GetMessageBoxStandard(
                    "Aknakereso jatek",
                    "A fajl mentese sikertelen!" + ex.Message,
                    ButtonEnum.Ok, Icon.Error)
                .ShowAsync();
        }
    }

    #endregion

    #region Model event handler

    private async void Model_GameOver(object? sender, AknakeresoEventArgs e)
    {
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            int winner = _model.CurrentPlayer;
            if (e.IsWon())
            {
                await MessageBoxManager.GetMessageBoxStandard(
                        "Aknakereso jatek",
                        $"Gratulalok! A(z) {winner}. jatkos nyert",
                        ButtonEnum.Ok, Icon.Info)
                    .ShowAsync();
            }
            else
            {
                await MessageBoxManager.GetMessageBoxStandard(
                        "Aknakereso jatek",
                        $"Sajnalom, vesztettel! A(z) {winner}. jatekos nyert!",
                        ButtonEnum.Ok, Icon.Info)
                    .ShowAsync();
            }
        });
    }

    #endregion
}
