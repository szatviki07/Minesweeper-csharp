using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Aknakereso.WPF.ViewModel
{
    // Általános parancs típusa.
    public class DelegateCommand : ICommand
    {
        private readonly Action<object?> _execute; // a tevékenységet végrehajtó lambda-kifejezés
        private readonly Func<object?, bool>? _canExecute; // a tevékenység feltételét ellenőrző lambda-kifejezés

        // Parancs létrehozása.
        public DelegateCommand(Action<object?> execute) : this(null, execute) { }

        public DelegateCommand(Func<object?, bool>? canExecute, Action<object?> execute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            _execute = execute;
            _canExecute = canExecute;
        }

        // Végrehajthatóság változásának eseménye.
        public event EventHandler? CanExecuteChanged;

        // Végrehajthatóság ellenőrzése
        public bool CanExecute(object? parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);
        }

        // Tevékenység végrehajtása.
        public void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
            {
                throw new InvalidOperationException("Command execution is disabled.");
            }
            _execute(parameter);
        }

        // Végrehajthatóság változásának eseménykiváltása.
        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, EventArgs.Empty);
            }
        }
    }
}
