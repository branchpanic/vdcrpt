using System;
using System.Windows.Input;

namespace Vdcrpt.Desktop;

internal class DelegateCommand : ICommand
{
    private readonly Func<object?, bool> _doCanExecute;
    private readonly Action<object?> _doExecute;

    public DelegateCommand(Action<object?> doExecute) : this(_ => true, doExecute)
    {
    }

    public DelegateCommand(Func<object?, bool> doCanExecute, Action<object?> doExecute)
    {
        _doCanExecute = doCanExecute;
        _doExecute = doExecute;
    }

    public bool CanExecute(object? parameter)
    {
        return _doCanExecute(parameter);
    }

    public void Execute(object? parameter)
    {
        _doExecute(parameter);
    }

    public event EventHandler? CanExecuteChanged;

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}