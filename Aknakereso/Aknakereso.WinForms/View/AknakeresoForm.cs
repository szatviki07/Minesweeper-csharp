using System;
using Aknakereso.Model;
using Aknakereso.Persistence;

namespace Aknakereso.WinForms
{
    public partial class AknakeresoForm : Form
    {
        #region Fields

        private AknakeresoGameModel gameModel;

        #endregion

        #region Constructors
        public AknakeresoForm()
        {
            InitializeComponent();

            // adatelérés példányosítása
            IAknakeresoDataAccess _dataAccess = new AknakeresoFileDataAccess();
            gameModel = new AknakeresoGameModel(_dataAccess);

            // GameModel esemenyek osszekotese
            gameModel.FieldChanged += GameModel_FieldChanged;
            gameModel.GameOver += GameModel_GameOver;

            //hosszabb szintax
            //gameModel.FieldChanged += new EventHandler<AknakeresoFieldEventArgs>(GameModel_FieldChanged);
            //gameModel.GameOver += new EventHandler<AknakeresoEventArgs>(GameModel_GameOver);

            this.Load += AknakeresoForm_Load;

            // Fix ablakméret
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = true;

            
            // Controls sorrend, hogy ne fedjék egymást
            this.Controls.Add(tableLayoutPanel1);
            this.Controls.Add(statusStrip1);
            this.Controls.Add(menuStrip1);
        }

        #endregion

        #region Game start
        private void AknakeresoForm_Load(object? sender, EventArgs e)
        {
            StartNewGame(10); // alapbol 10x10
        }

        private void StartNewGame(int size)
        {
            gameModel.NewGame(size);
            GenerateButtons();
            UpdateCurrentPlayerLabel();

            int cellSize = 40;   // minden mező kb. 40x40 pixel
            int border = 100;    // plusz hely pl menünek

            int newFormWidth = size * cellSize + border;
            int newFormHeight = size * cellSize + border + statusStrip1.Height;

            // Ablak új mérete
            this.ClientSize = new Size(newFormWidth, newFormHeight);
        }


        #endregion

        #region Generate table

        // Gombok generálása a tábla mezõibõl
        private void GenerateButtons()
        {
            int size = gameModel.TableSize;

            // összes elõzõ gomb törlése
            tableLayoutPanel1.Controls.Clear();
            tableLayoutPanel1.ColumnCount = size;
            tableLayoutPanel1.RowCount = size;
            tableLayoutPanel1.ColumnStyles.Clear();
            tableLayoutPanel1.RowStyles.Clear();

            // minden mezõ egyforma méretű legyen, százalékos elosztás
            for (int i = 0; i < size; i++)
            {
                tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / size));
                tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / size));
            }

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    // generált gomb
                    Button b = new Button();
                    b.Dock = DockStyle.Fill;
                    b.Margin = new Padding(1);
                    b.Tag = Tuple.Create(x, y); // gomb koordinátáinak elmentése
                    b.MouseUp += Button_MouseUp;
                    b.TextAlign = ContentAlignment.MiddleCenter;
                    b.Font = GetFontForButton(b); // betüméret változása a cellaméret -> táblaméret alapján

                    // betöltés után a gomb "értékei"
                    if (gameModel.Table.IsRevealed(x, y))
                    {
                        int value = gameModel.Table[x, y];
                        if (value == -1)
                        {
                            b.Text = "💣";
                            b.BackColor = Color.Red;
                        }
                        else if (value == 0)
                        {
                            b.Text = "";
                        }
                        else
                        {
                            b.Text = value.ToString();
                        }
                    }
                    else if (gameModel.Table.IsFlagged(x, y))
                    {
                        b.Text = "🚩";
                        b.BackColor = Color.Yellow;
                    }
                    else
                    {
                        b.Text = "";
                    }

                    tableLayoutPanel1.Controls.Add(b, y, x);

                }
            }
        }


        // Segédmetódus a betűmérethez
        private Font GetFontForButton(Button b)
        {
            int size = Math.Min(b.Width, b.Height);
            if (size < 1) size = 1; // alap biztonság
            return new Font("Segoe UI", size * 0.6f, FontStyle.Bold);
        }

        #endregion

        #region Event handlers

        private void Button_MouseUp(object? sender, MouseEventArgs e)
        {
            if (sender is not Button b) return;

            var coords = (Tuple<int, int>)b.Tag!; // a GenerateButtons-ból
            int x = coords.Item1;
            int y = coords.Item2;

            if (e.Button == MouseButtons.Left)
                gameModel.Reveal(x, y);
            else if (e.Button == MouseButtons.Right)
                gameModel.Flag(x, y);
        }

        private void GameModel_FieldChanged(object? sender, AknakeresoFieldEventArgs e)
        {
            // lekérjük az adott mezõt, mint gomb
            Button? b = tableLayoutPanel1.GetControlFromPosition(e.Y, e.X) as Button;
            
            if (b == null) return;

            if (gameModel.IsFlagged(e.X, e.Y) && !e.IsRevealed)
            {
                b.Text = "🚩";
                b.BackColor = Color.Yellow;
                b.Enabled = true; // tudunk újra kattintani rá
            }
            else if (e.IsRevealed)
            {
                b.Enabled = false;

                if (e.NewValue == -1)
                {
                    b.Text = "💣";
                    b.BackColor = Color.Red;
                }
                else if (e.NewValue > 0)
                {
                    b.Text = e.NewValue.ToString();
                }
                else
                {
                    b.Text = "";
                }
            }
            else
            {
                b.Text = "";
                b.Enabled = true;
            }

            UpdateCurrentPlayerLabel();
        }

        private void GameModel_GameOver(object? sender, AknakeresoEventArgs e)
        {
            MessageBox.Show($"Game Over! The winner is Player {e.Winner()}");
        }

        private void UpdateCurrentPlayerLabel()
        {
            toolStripStatusLabelCurrentPlayer.Text = $"Player {gameModel.CurrentPlayer} is next";
        }


        #endregion

        #region Menu event handlers

        private void newGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartNewGame(10);
        }

        private async void saveGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Filter = "Aknakereső game file|*.akn"; // opcionális szűrés
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        await gameModel.SaveGameAsync(dlg.FileName);
                        MessageBox.Show("Game saved!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error while saving game... " + ex.Message);
                    }
                }
            }
        }

        private async void loadGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "Aknakereso game file|*.akn";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        await gameModel.LoadGameAsync(dlg.FileName);

                        GenerateButtons();

                        UpdateCurrentPlayerLabel(); // frissítjük a játékos jelzőt
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error while loading game... " + ex.Message);
                    }
                }
            }
        }

        private void x6ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartNewGame(6);
        }

        private void x10ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartNewGame(10);
        }

        private void x16ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartNewGame(16);
        }

        #endregion

    }
}