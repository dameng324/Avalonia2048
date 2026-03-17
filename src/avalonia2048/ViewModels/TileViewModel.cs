using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace avalonia2048.ViewModels;

public partial class TileViewModel : ObservableObject
{
    [ObservableProperty] private int _value;
    [ObservableProperty] private double _scale = 1.0;
    [ObservableProperty] private IBrush _background = Brushes.Transparent;
    [ObservableProperty] private IBrush _foreground = Brushes.Black;
    [ObservableProperty] private double _fontSize = 36;

    public int Row { get; }
    public int Col { get; }

    public TileViewModel(int row, int col)
    {
        Row = row;
        Col = col;
    }

    partial void OnValueChanged(int value)
    {
        UpdateAppearance(value);
    }

    private void UpdateAppearance(int value)
    {
        Background = GetBackground(value);
        Foreground = GetForeground(value);
        FontSize = value >= 1024 ? 24 : value >= 128 ? 30 : 36;
    }

    public static IBrush GetBackground(int value) => value switch
    {
        0 => new SolidColorBrush(Color.Parse("#cdc1b4")),
        2 => new SolidColorBrush(Color.Parse("#eee4da")),
        4 => new SolidColorBrush(Color.Parse("#ede0c8")),
        8 => new SolidColorBrush(Color.Parse("#f2b179")),
        16 => new SolidColorBrush(Color.Parse("#f59563")),
        32 => new SolidColorBrush(Color.Parse("#f67c5f")),
        64 => new SolidColorBrush(Color.Parse("#f65e3b")),
        128 => new SolidColorBrush(Color.Parse("#edcf72")),
        256 => new SolidColorBrush(Color.Parse("#edcc61")),
        512 => new SolidColorBrush(Color.Parse("#edc850")),
        1024 => new SolidColorBrush(Color.Parse("#edc53f")),
        2048 => new SolidColorBrush(Color.Parse("#edc22e")),
        _ => new SolidColorBrush(Color.Parse("#3c3a32"))
    };

    public static IBrush GetForeground(int value) => value switch
    {
        2 or 4 => new SolidColorBrush(Color.Parse("#776e65")),
        _ => new SolidColorBrush(Color.Parse("#f9f6f2"))
    };
}
