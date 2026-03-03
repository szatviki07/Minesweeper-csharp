using Moq;
using Aknakereso.Model;
using Aknakereso.Persistence;

namespace Aknakereso.Test
{
    [TestClass]
    public sealed class AknakeresoGameModelTest
    {
        private AknakeresoGameModel _model = null!; // a tesztelendő modell
        private AknakeresoTable _mockedTable = null!; // mockolt játéktábla
        private Mock<IAknakeresoDataAccess> _mock = null!; // az adatelérés mock-ja

        [TestInitialize]
        public void Initialize()
        {
            _mockedTable = new AknakeresoTable();
            _mockedTable.SetValue(0, 0, 0); // a (0,0) mezõ értékek 0 -> nincs körülötte akna
            _mockedTable.SetFlagged(1, 0, true); // az (1, 0) mezõ zászlózva
            _mockedTable.SetRevealed(0, 1, true); // a (0, 1) mezõ felfedve
            _mockedTable.CurrentPlayer = 1;
            // előre definiálunk egy játéktáblát a perzisztencia mockolt teszteléséhez

            _mock = new Mock<IAknakeresoDataAccess>();
            _mock.Setup(mock => mock.LoadAsync(It.IsAny<String>())) // "bármilyen  fájlnevet" is kap                  ^
                .Returns(() => Task.FromResult(_mockedTable));      // mindig ugyanazt a mock-kolt tablat adja vissza |
            // a mock a LoadAsync műveletben bármilyen paraméterre az előre beállított játéktáblát fogja visszaadni

            _model = new AknakeresoGameModel(_mock.Object);
            // példányosítjuk a modellt a mock objektummal

            _model.FieldChanged += new EventHandler<AknakeresoFieldEventArgs>(Model_OnFieldChanged);
            _model.GameOver += new EventHandler<AknakeresoEventArgs>(Model_OnGameOver);
        }

        // Eseménykezelõk
        private void Model_OnGameOver(Object? sender, AknakeresoEventArgs e)
        {
            Assert.IsTrue(_model.IsGameOver); // ha kiváltódik -> vége a játéknak
        }

        private void Model_OnFieldChanged(Object? sender, AknakeresoFieldEventArgs e)
        {
            Assert.IsTrue(e.X >= 0 && e.Y >= 0); // érvényes pozíció
        }

        //   -------    tesztek    -------   //
 
        [TestMethod]
        public void AknakeresoGameModel_NewGame_6x6_Test()
        {
            _model.NewGame(6);

            Assert.AreEqual(6, _model.TableSize);

            int aknaDb = 0;
            for (int x = 0; x < _model.TableSize; x++)
                for (int y = 0; y < _model.TableSize; y++)
                    if (_model[x, y] == -1)
                        aknaDb++;
            Assert.AreEqual(6, aknaDb); // 6x6 -> 6 akna, 10x10 -> 15 akna, 16x16 -> 40 akna
        }

        [TestMethod]
        public void AknakeresoGameModel_NewGame_10x10_Test()
        {
            _model.NewGame(10);

            Assert.AreEqual(10, _model.TableSize);

            int aknaDb = 0;
            for (int x = 0; x < _model.TableSize; x++)
                for (int y = 0; y < _model.TableSize; y++)
                    if (_model[x, y] == -1)
                        aknaDb++;
            Assert.AreEqual(15, aknaDb); // 6x6 -> 6 akna, 10x10 -> 15 akna, 16x16 -> 40 akna
        }

        [TestMethod]
        public void AknakeresoGameModel_NewGame_16x16_Test()
        {
            _model.NewGame(16);

            Assert.AreEqual(16, _model.TableSize);

            int aknaDb = 0;
            for (int x = 0; x < _model.TableSize; x++)
                for (int y = 0; y < _model.TableSize; y++)
                    if (_model[x, y] == -1)
                        aknaDb++;
            Assert.AreEqual(40, aknaDb); // 6x6 -> 6 akna, 10x10 -> 15 akna, 16x16 -> 40 akna
        }

        // általánosabb tesztelés
        [TestMethod]
        public void AknakeresoGameModel_NewGame_Fields_Test()
        {
            _model.NewGame(10); // alapértelmezett mérettel

            Assert.IsFalse(_model.IsGameOver);

            int revealedDb = 0;
            int flaggedDb = 0;
            for (int x = 0; x < _model.TableSize; x++)
                for (int y = 0; y < _model.TableSize; y++)
                {
                    if (_model.IsRevealed(x, y))
                    {
                        revealedDb++;
                    }
                    if (_model.IsFlagged(x, y))
                    {
                        flaggedDb++;
                    }
                }
            Assert.AreEqual(0, revealedDb); // alapból semmi nincs felfedve
            Assert.AreEqual(0, flaggedDb);  // alapból semmi nincs zászlózva
        }

        [TestMethod]
        public void AknakeresoGameModel_Reveal_Test()
        {
            _model.NewGame(10);

            var table = typeof(AknakeresoGameModel)
                .GetField("_table", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

            AknakeresoTable realTable = (AknakeresoTable)table.GetValue(_model)!;

            realTable.SetValue(0, 0, 0); // nem akna
            _model.Reveal(0, 0);

            Assert.IsTrue(_model.IsRevealed(0, 0));
            Assert.IsFalse(_model.IsGameOver);
        }

        // a tényleges táblát nem, csak a clone-jat latjuk, így nem lenne sikeres teszt ha az elõzõ módszerrel csináltuk volna 
        [TestMethod]
        public void AknakeresoGameModel_Reveal_Mine_Test()
        {
            _model.NewGame(10);

            // belső táblához hozzáférés reflectionnel
            var table = typeof(AknakeresoGameModel)
                .GetField("_table", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

            AknakeresoTable realTable = (AknakeresoTable)table.GetValue(_model)!;
            realTable.SetValue(0, 0, -1); // itt ténylegesen a belső táblát módosítjuk

            _model.Reveal(0, 0);

            Assert.IsTrue(_model.IsGameOver);
        }

        [TestMethod]
        public void AknakeresoGameModel_Flag_Test()
        {
            _model.NewGame(10);

            // zászlózzuk
            _model.Flag(1, 1);
            Assert.IsTrue(_model.IsFlagged(1, 1));

            // töröljük a zászlót
            _model.Flag(1, 1);
            Assert.IsFalse(_model.IsFlagged(1, 1));
        }

        // nem aknára lépünk, hanem mindent felfedünk
        [TestMethod]
        public void AknakeresoGameModel_AllSafeRevealed_GameOver_Test()
        {
            _model.NewGame(10);

            // ezzel el tudjuk érni a tényleges táblát nem csak a clone-t
            var table = typeof(AknakeresoGameModel)
                .GetField("_table", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

            AknakeresoTable realTable = (AknakeresoTable)table.GetValue(_model)!;

            // nincs akna
            for (int x = 0; x < 10; x++)
                for (int y = 0; y < 10; y++)
                    realTable.SetValue(x, y, 0);

            // mindent felfedünk
            for (int x = 0; x < 10; x++)
                for (int y = 0; y < 10; y++)
                    _model.Reveal(x, y);

            Assert.IsTrue(_model.IsGameOver);
        }

        [TestMethod]
        public async Task AknakeresoGameModelLoadTest()
        {
            _model.NewGame(10);
            await _model.LoadGameAsync(string.Empty);

            for (int x = 0; x < _model.TableSize; x++)
                for (int y = 0; y < _model.TableSize; y++)
                {
                    Assert.AreEqual(_mockedTable[x, y], _model[x, y]);
                    Assert.AreEqual(_mockedTable.IsRevealed(x, y), _model.IsRevealed(x, y));
                    Assert.AreEqual(_mockedTable.IsFlagged(x, y), _model.IsFlagged(x, y));
                }

            _mock.Verify(dataAccess => dataAccess.LoadAsync(string.Empty));
        }

        [TestMethod]
        public void AknakeresoGameModel_Reveal_Neighbours_Test()
        {
            _model.NewGame(10);

            // keszitunk egy 3x3-as tablat
            int[,] values =
            {
                { 0, 0, 0 },
                { 0, 1, 1 },
                { 0, 1, -1 }
            };

            var table = typeof(AknakeresoGameModel)
                .GetField("_table", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

            AknakeresoTable realTable = (AknakeresoTable)table.GetValue(_model)!;

            // feltoltjuk
            for (int x = 0; x < 3; x++)
                for (int y = 0; y < 3; y++)
                   realTable.SetValue(x, y, values[x, y]);

            _model.Reveal(0, 0);

            // minden szomszedos 0 mezot is felfedunk
            Assert.IsTrue(_model.IsRevealed(0, 0));
            Assert.IsTrue(_model.IsRevealed(0, 1));
            Assert.IsTrue(_model.IsRevealed(1, 0));
            Assert.IsTrue(_model.IsRevealed(2, 0));
            Assert.IsTrue(_model.IsRevealed(0, 2));
            // felfedjuk a 0 mezok szomszedait is
            Assert.IsTrue(_model.IsRevealed(1, 1));
            Assert.IsTrue(_model.IsRevealed(1, 2));
            Assert.IsTrue(_model.IsRevealed(2, 1));

            // a bomba nem lesz felfedve
            Assert.IsFalse(_model.IsRevealed(2, 2));

        }

        [TestMethod]
        public async Task AknakeresoGameModel_LoadGame_SwitchPlayer_Test()
        {
            _mockedTable.CurrentPlayer = 2;

            await _model.LoadGameAsync("");  // vegulis mindegy mi, mert a mocked tablet adja vissza

            Assert.AreEqual(2, _model.CurrentPlayer);
        }

        [TestMethod]
        public void AknakeresoGameModel_Cannot_Reveal_FLagged_Test()
        {
            _model.NewGame(6);

            var table = typeof(AknakeresoGameModel)
                .GetField("_table", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

            AknakeresoTable realTable = (AknakeresoTable)table.GetValue(_model)!;

            realTable.SetValue(0, 0, 0);
            realTable.SetFlagged(0, 0, true);

            _model.Reveal(0, 0);

            Assert.IsFalse(_model.IsRevealed(0, 0));
        }


    }
}
