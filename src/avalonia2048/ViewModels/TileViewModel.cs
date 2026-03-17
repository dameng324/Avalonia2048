using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace avalonia2048.ViewModels;

public partial class TileViewModel : ObservableObject
{
    // Cached static brushes to avoid per-call allocations and GC pressure.
    private static readonly IBrush BgEmpty  = new SolidColorBrush(Color.Parse("#cdc1b4"));
    private static readonly IBrush Bg2      = new SolidColorBrush(Color.Parse("#eee4da"));
    private static readonly IBrush Bg4      = new SolidColorBrush(Color.Parse("#ede0c8"));
    private static readonly IBrush Bg8      = new SolidColorBrush(Color.Parse("#f2b179"));
    private static readonly IBrush Bg16     = new SolidColorBrush(Color.Parse("#f59563"));
    private static readonly IBrush Bg32     = new SolidColorBrush(Color.Parse("#f67c5f"));
    private static readonly IBrush Bg64     = new SolidColorBrush(Color.Parse("#f65e3b"));
    private static readonly IBrush Bg128    = new SolidColorBrush(Color.Parse("#edcf72"));
    private static readonly IBrush Bg256    = new SolidColorBrush(Color.Parse("#edcc61"));
    private static readonly IBrush Bg512    = new SolidColorBrush(Color.Parse("#edc850"));
    private static readonly IBrush Bg1024   = new SolidColorBrush(Color.Parse("#edc53f"));
    private static readonly IBrush Bg2048   = new SolidColorBrush(Color.Parse("#edc22e"));
    private static readonly IBrush BgHigher = new SolidColorBrush(Color.Parse("#3c3a32"));

    private static readonly IBrush FgLight = new SolidColorBrush(Color.Parse("#776e65"));
    private static readonly IBrush FgDark  = new SolidColorBrush(Color.Parse("#f9f6f2"));

    [ObservableProperty] private int _value;
    [ObservableProperty] private double _scale = 1.0;
    [ObservableProperty] private IBrush _background = BgEmpty;
    [ObservableProperty] private IBrush _foreground = FgDark;
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
        Background = GetBackground(value);
        Foreground = GetForeground(value);
        FontSize = value >= 1024 ? 24 : value >= 128 ? 30 : 36;
    }

    public static IBrush GetBackground(int value) => value switch
    {
        0    => BgEmpty,
        2    => Bg2,
        4    => Bg4,
        8    => Bg8,
        16   => Bg16,
        32   => Bg32,
        64   => Bg64,
        128  => Bg128,
        256  => Bg256,
        512  => Bg512,
        1024 => Bg1024,
        2048 => Bg2048,
        _    => BgHigher
    };

    public static IBrush GetForeground(int value) => value is 2 or 4 ? FgLight : FgDark;
}
