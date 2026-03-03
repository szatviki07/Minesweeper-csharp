using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aknakereso.Persistence
{
    // Aknakeresõ fájl kezelõ felülete.
    public interface IAknakeresoDataAccess
    {
        // Fájl betöltése
        Task<AknakeresoTable> LoadAsync(string path);

        // Fájl mentése
        Task SaveAsync(string path, AknakeresoTable table);
    }
}
