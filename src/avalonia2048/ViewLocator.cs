using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia2048.ViewModels;

namespace Avalonia2048;

public class ViewLocator : IDataTemplate
{
    private static readonly Dictionary<Type, Func<Control>> _registry = new();

    public static void Register<TViewModel, TView>()
        where TView : Control, new()
    {
        _registry[typeof(TViewModel)] = static () => new TView();
    }

    public Control? Build(object? data)
    {
        if (data is null)
            return null;
        if (_registry.TryGetValue(data.GetType(), out var factory))
            return factory();
        return new TextBlock { Text = "Not Found: " + data.GetType().FullName };
    }

    public bool Match(object? data) => data is ViewModelBase;
}
