using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aknakereso.Model;
using Aknakereso.Persistence;
using Aknakereso.Avalonia.ViewModels;

namespace Aknakereso.Avalonia.Views
{
    // de ez nem kotelezo
    public static class DesignData
    {
        public static AknakeresoViewModel ViewModel
        {
            get
            {
                var model = new AknakeresoGameModel(new AknakeresoFileDataAccess());
                model.NewGame(10);
                return new AknakeresoViewModel(model);
            }
        }
    }
}

