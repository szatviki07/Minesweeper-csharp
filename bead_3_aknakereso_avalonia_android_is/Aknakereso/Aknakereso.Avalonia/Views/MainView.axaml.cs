using Aknakereso.Avalonia.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Aknakereso.Avalonia.Views;

public partial class MainView : UserControl, IDisposable
{
    public MainView()
    {
        InitializeComponent();
    }

    // megszakithato idozito
    private CancellationTokenSource? _longPressToken = null!;
    private bool _longPressDone;

    // jobb es bal katt (hosszu katt) -> desktop vs andorid kezeles
    private async void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Border border) return;
        if (border.DataContext is not AknakeresoField field) return;

        _longPressDone = false;

        if (e.GetCurrentPoint(border).Properties.IsRightButtonPressed)
        {
            field.FlagCommand?.Execute(null);
            _longPressDone = true;
            return;
        }

        // ezzel jelezzuk a megszakitast
        _longPressToken = new CancellationTokenSource();

        try
        {
            await Task.Delay(600, _longPressToken.Token); // 0.6 mp
            field.FlagCommand?.Execute(null);
            _longPressDone = true;
        }
        catch (TaskCanceledException)
        {

        }

    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is not Border border) return;
        if (border.DataContext is not AknakeresoField field) return;

        // ha mar tortent flag -> semmi
        if (_longPressDone) return;

        field.RevealCommand?.Execute(null);
    }

    // statikus elemzo hibai miatt - memoria szivargas
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        Dispose();
    }

    // a tokeneket a hosszu nyomasbol, minden UI ujraepiteskor el kell tavolitani 
    public void Dispose()
    {
        _longPressToken?.Cancel();
        _longPressToken?.Dispose();
        _longPressToken = null;
    }

}
