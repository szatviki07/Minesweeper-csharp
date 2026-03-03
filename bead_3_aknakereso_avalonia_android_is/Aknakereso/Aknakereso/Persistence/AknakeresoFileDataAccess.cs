using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aknakereso.Persistence
{
    // Aknakeresõ fájlkezelő típusa
    public class AknakeresoFileDataAccess : IAknakeresoDataAccess
    {
        // PATH alapu fajlbetoltes
        public async Task<AknakeresoTable> LoadAsync(string path)
        {
            return await LoadAsync(File.OpenRead(path));
        }

        // STREAM alapu fajlbetoltes
        public async Task<AknakeresoTable> LoadAsync(Stream stream)
        {
            try
            {
                using (StreamReader reader = new StreamReader(stream)) // fájl megnyitása olvasásra
                {
                    // tábla mérete
                    string line = await reader.ReadLineAsync() ?? string.Empty;
                    int tableSize = int.Parse(line);
                    AknakeresoTable table = new AknakeresoTable(tableSize);

                    // mezõk értékei
                    for (int x = 0; x < tableSize; x++)
                    {
                        line = await reader.ReadLineAsync() ?? string.Empty;
                        string[] values = line.Split(' ');

                        for (int y = 0; y < tableSize; y++)
                        {
                            table.SetValue(x, y, int.Parse(values[y]));
                        }
                    }

                    // zászlók
                    for (int x = 0; x < tableSize; x++)
                    {
                        line = await reader.ReadLineAsync() ?? string.Empty;
                        string[] flags = line.Split(' ');

                        for (int y = 0; y < tableSize; y++)
                        {
                            if (flags[y] == "F")
                            {
                                table.SetFlagged(x, y, true);
                            }
                        }
                    }

                    // felfedett mezõk
                    for (int x = 0; x < tableSize; x++)
                    {
                        line = await reader.ReadLineAsync() ?? string.Empty;
                        string[] revealed = line.Split(' ');

                        for (int y = 0; y < tableSize; y++)
                        {
                            if (revealed[y] == "R")
                            {
                                table.SetRevealed(x, y, true);
                            }
                        }
                    }

                    // aktuális játékos
                    line = await reader.ReadLineAsync() ?? "1";
                    int currentPlayer = int.Parse(line);
                    table.CurrentPlayer = currentPlayer;

                    return table;
                }
            }
            catch
            {
                throw new AknakeresoDataException();
            }
        }

        // PATH alapu fajlmentes
        public async Task SaveAsync(string path, AknakeresoTable table)
        {
            await SaveAsync(File.OpenWrite(path), table);
        }

        // STREAM alapu fajlmentes
        public async Task SaveAsync(Stream stream, AknakeresoTable table)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(stream)) // fájl megnyitása írásra
                {
                    // kiírjuk a tábla méretét
                    await writer.WriteLineAsync(table.Size.ToString());
                    
                    // mezõk értékei
                    for (int x = 0; x < table.Size; x++)
                    {
                        for (int y = 0; y < table.Size; y++)
                        {
                            await writer.WriteAsync(table[x, y] + " ");
                        }
                        await writer.WriteLineAsync();
                    }

                    // zászlók
                    for (int x = 0; x < table.Size; x++)
                    {
                        for (int y = 0; y < table.Size; y++)
                        {
                            await writer.WriteAsync((table.IsFlagged(x, y) ? "F" : "X") + " "); // kiírjuk a zárolásokat
                        }
                        await writer.WriteLineAsync();
                    }

                    // felfedett mezõk
                    for (int x = 0; x < table.Size; x++)
                    {
                        for (int y = 0; y < table.Size; y++)
                        {
                            await writer.WriteAsync((table.IsRevealed(x, y) ? "R" : "X") + " "); // kiírjuk a zárolásokat
                        }
                        await writer.WriteLineAsync();
                    }
                }
            }
            catch
            {
                throw new AknakeresoDataException();
            }
        }
    }
}
