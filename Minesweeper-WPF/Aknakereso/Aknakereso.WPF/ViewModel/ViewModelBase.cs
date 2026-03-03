using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Aknakereso.WPF.ViewModel
{
    // Nézetmodell ősosztály típusa.
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        // Nézetmodell ősosztály példányosítása.
        protected ViewModelBase() { }

        // Tulajdonság változásának eseménye.
        public event PropertyChangedEventHandler? PropertyChanged;

        // Tulajdonság változása ellenőrzéssel.
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
