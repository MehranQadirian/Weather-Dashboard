using System;
using System.Windows.Input;

namespace Weather.Dashboard.Avalonia.ViewModels
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        public event EventHandler? CanExecuteChanged;

        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object? parameter)
        {
            if (CanExecute(parameter))
            {
                _execute(parameter);
            }
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T?> _execute;
        private readonly Func<T?, bool>? _canExecute;

        public event EventHandler? CanExecuteChanged;

        public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute((T?)parameter);
        }

        public void Execute(object? parameter)
        {
            if (CanExecute(parameter))
            {
                try
                {
                    if (typeof(T) == typeof(int) && parameter is string s)
                    {
                        _execute((T?)(object)int.Parse(s));
                    }
                    else
                    {
                        _execute((T?)parameter);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ RelayCommand Execute cast error: {ex.Message}");
                }
            }
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}