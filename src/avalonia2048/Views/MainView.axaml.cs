using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using avalonia2048.ViewModels;

namespace avalonia2048.Views;

public class TileValueConverter : IValueConverter
{
    public static readonly TileValueConverter Instance = new();

    public object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        System.Globalization.CultureInfo culture
    )
    {
        if (value is int v && v == 0)
            return string.Empty;
        return value?.ToString();
    }

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        System.Globalization.CultureInfo culture
    ) => throw new NotSupportedException();
}

public class GameStateConverter : IValueConverter
{
    public static readonly GameStateConverter Instance = new();

    public object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        System.Globalization.CultureInfo culture
    ) => value is true ? "You Win! 🎉" : "Game Over!";

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        System.Globalization.CultureInfo culture
    ) => throw new NotSupportedException();
}

public partial class MainView : UserControl
{
    private Avalonia.Point _touchStart;
    private const double SwipeThreshold = 50;

    public MainView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        PointerPressed += OnPointerPressed;
        PointerReleased += OnPointerReleased;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
            vm.TileAnimationRequested += AnimateTileAsync;
        Focus();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        if (DataContext is MainViewModel vm)
            vm.TileAnimationRequested -= AnimateTileAsync;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (DataContext is not MainViewModel vm)
            return;

        var dir = e.Key switch
        {
            Key.Left or Key.A => "Left",
            Key.Right or Key.D => "Right",
            Key.Up or Key.W => "Up",
            Key.Down or Key.S => "Down",
            _ => null,
        };

        if (dir != null)
        {
            e.Handled = true;
            _ = vm.MoveCommand.ExecuteAsync(dir);
        }
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _touchStart = e.GetPosition(this);
        Focus();
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (DataContext is not MainViewModel vm)
            return;

        var end = e.GetPosition(this);
        var dx = end.X - _touchStart.X;
        var dy = end.Y - _touchStart.Y;

        if (Math.Abs(dx) < SwipeThreshold && Math.Abs(dy) < SwipeThreshold)
            return;

        string dir;
        if (Math.Abs(dx) > Math.Abs(dy))
            dir = dx > 0 ? "Right" : "Left";
        else
            dir = dy > 0 ? "Down" : "Up";

        _ = vm.MoveCommand.ExecuteAsync(dir);
    }

    private async Task AnimateTileAsync(TileViewModel tile, bool isNew)
    {
        if (isNew)
        {
            // New tile: scale 0 -> 1.1 -> 1.0
            tile.Scale = 0;
            await Task.Delay(16);
            await AnimateScaleAsync(tile, 0, 1.1, 120);
            await AnimateScaleAsync(tile, 1.1, 1.0, 80);
        }
        else
        {
            // Merge tile: scale 1.0 -> 1.2 -> 1.0
            await AnimateScaleAsync(tile, 1.0, 1.2, 80);
            await AnimateScaleAsync(tile, 1.2, 1.0, 80);
        }
    }

    private static async Task AnimateScaleAsync(
        TileViewModel tile,
        double from,
        double to,
        int durationMs
    )
    {
        const int steps = 12;
        int stepDelay = Math.Max(1, durationMs / steps);

        for (int i = 1; i <= steps; i++)
        {
            double t = (double)i / steps;
            // Ease in-out
            t = t < 0.5 ? 2 * t * t : -1 + (4 - 2 * t) * t;
            tile.Scale = from + (to - from) * t;
            await Task.Delay(stepDelay);
        }
        tile.Scale = to;
    }
}
