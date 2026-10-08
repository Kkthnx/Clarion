using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;

namespace Clarion.App.Services;

/// <summary>
/// Raises property changes on the UI thread no matter which thread set the value, so work done in the
/// background can update bound controls without crashing them.
/// </summary>
public abstract class UiObservableObject : ObservableObject
{
    private readonly DispatcherQueue? _dispatcher = DispatcherQueue.GetForCurrentThread();

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (_dispatcher is null || _dispatcher.HasThreadAccess)
        {
            base.OnPropertyChanged(e);
            return;
        }
        _dispatcher.TryEnqueue(() => base.OnPropertyChanged(e));
    }
}
