namespace ConwayWinApp
{

    public class GenerationStats
    {
        public int Generation { get; set; }
        public int Alive { get; set; }
        public int Births { get; set; }
        public int Deaths { get; set; }
    }

    public partial class Form1 : Form
    {

        List<GenerationStats> statHistory = new List<GenerationStats>();

        int stagnantFrameCount = 0;
        const int stagnationThreshold = 10; // How many stagnant generations before triggering
        int rows = 75;
        int cols = 75;
        int cellSize = 15;
        bool[,]? currentGrid;
        bool[,]? nextGrid;
        bool isPaused = false;
        System.Windows.Forms.Timer? timer;

        int generationCount = 0;
        int totalBirths = 0;
        int totalDeaths = 0;
        int currentlyAlive = 0;

        Label? statsLabel;
        Panel? logPanel;
        Label? pauseLabel;
        Label? highestBirthLabel;
        Label? lowestBirthLabel;
        Label? highestDeathLabel;
        Label? lowestDeathLabel;
        Label? stagnationLabel;
        Panel? overlayPanel;

        HashSet<string> recentHashes = new HashSet<string>();
        Queue<string> hashQueue = new Queue<string>();
        const int hashMemory = 20; // How many generations to keep

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            int scoreboardOffset = 20; // Padding from grid
            int gridRightEdge = cols * cellSize;

            this.FormBorderStyle = FormBorderStyle.None;
            this.Bounds = Screen.PrimaryScreen.Bounds;
            this.Paint += Form1_Paint;
            this.Load += Form1_Load;
            this.BackColor = Color.White;
            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;
            

            statsLabel = new Label();
            statsLabel.Font = new Font("Segoe UI", 25, FontStyle.Regular);
            statsLabel.ForeColor = Color.Black;
            statsLabel.BackColor = Color.Transparent;
            statsLabel.AutoSize = true;
            statsLabel.Location = new Point(gridRightEdge + scoreboardOffset, 20);
            this.Controls.Add(statsLabel);

            logPanel = new Panel();
            logPanel.AutoScroll = true;
            logPanel.Width = 400;
            logPanel.Height = 1000;
            logPanel.Location = new Point(statsLabel.Location.X, statsLabel.Bottom + 20);
            logPanel.BorderStyle = BorderStyle.None; // Optional
            this.Controls.Add(logPanel);

            int summaryLabelX = logPanel.Location.X;
            int summaryStartY = logPanel.Bottom + 20;
            int spacing = 30;

            highestBirthLabel = new Label();
            highestBirthLabel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            highestBirthLabel.ForeColor = Color.DarkGreen;
            highestBirthLabel.AutoSize = true;
            highestBirthLabel.Text = "Highest Birthrate: Gen - | Alive: - | Births: -";
            highestBirthLabel.Location = new Point(summaryLabelX, summaryStartY);
            this.Controls.Add(highestBirthLabel);

            lowestBirthLabel = new Label();
            lowestBirthLabel.Font = highestBirthLabel.Font;
            lowestBirthLabel.ForeColor = Color.Green;
            lowestBirthLabel.AutoSize = true;
            lowestBirthLabel.Text = "Lowest Birthrate: Gen - | Alive: - | Births: -";
            lowestBirthLabel.Location = new Point(summaryLabelX, summaryStartY + spacing);
            this.Controls.Add(lowestBirthLabel);

            highestDeathLabel = new Label();
            highestDeathLabel.Font = highestBirthLabel.Font;
            highestDeathLabel.ForeColor = Color.DarkRed;
            highestDeathLabel.AutoSize = true;
            highestDeathLabel.Text = "Highest Deathrate: Gen - | Alive: - | Deaths: -";
            highestDeathLabel.Location = new Point(summaryLabelX, summaryStartY + spacing * 2);
            this.Controls.Add(highestDeathLabel);

            lowestDeathLabel = new Label();
            lowestDeathLabel.Font = highestBirthLabel.Font;
            lowestDeathLabel.ForeColor = Color.Maroon;
            lowestDeathLabel.AutoSize = true;
            lowestDeathLabel.Text = "Lowest Deathrate: Gen - | Alive: - | Deaths: -";
            lowestDeathLabel.Location = new Point(summaryLabelX, summaryStartY + spacing * 3);
            this.Controls.Add(lowestDeathLabel);

            Label escLabel = new Label();
            escLabel.Text = "Press ESC to quit application";
            escLabel.ForeColor = Color.Black;
            escLabel.BackColor = Color.Transparent;
            escLabel.Font = new Font("Segoe UI", 25, FontStyle.Regular);
            escLabel.AutoSize = true;
            escLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            // Adjust position for larger font size
            escLabel.Location = new Point(this.ClientSize.Width - escLabel.PreferredWidth - 20,
                                          this.ClientSize.Height - escLabel.PreferredHeight - 20);

            escLabel.BringToFront();
            this.Controls.Add(escLabel);

            Label pauseInstructionLabel = new Label();
            pauseInstructionLabel.Text = "Press SPACE to pause";
            pauseInstructionLabel.ForeColor = Color.Black;
            pauseInstructionLabel.BackColor = Color.Transparent;
            pauseInstructionLabel.Font = new Font("Segoe UI", 25, FontStyle.Regular);
            pauseInstructionLabel.AutoSize = true;
            pauseInstructionLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            pauseInstructionLabel.Location = new Point(escLabel.Location.X,
                                                       escLabel.Location.Y - pauseInstructionLabel.PreferredHeight - 10);

            pauseInstructionLabel.BringToFront();
            this.Controls.Add(pauseInstructionLabel);


            int horizontalOffset = 200; // adjust as needed (more = further left)
            pauseLabel = new Label();
            pauseLabel.Text = "PAUSED";
            pauseLabel.Font = new Font("Segoe UI", 20, FontStyle.Bold);
            pauseLabel.ForeColor = Color.Red;
            pauseLabel.BackColor = Color.Transparent;
            pauseLabel.AutoSize = true;
            pauseLabel.Visible = false;
            pauseLabel.Location = new Point(statsLabel.Location.X, statsLabel.Location.Y + statsLabel.Height + 10);
            this.Controls.Add(pauseLabel);

            Label rulesLabel = new Label();
            rulesLabel.Text =
                "Rules of Conway’s Game of Life:\n" +
                "1. Alive cell with <2 neighbors dies (underpopulation)\n" +
                "2. Alive cell with 2–3 neighbors lives (survival)\n" +
                "3. Alive cell with >3 neighbors dies (overpopulation)\n" +
                "4. Dead cell with exactly 3 neighbors becomes alive (reproduction)";
            rulesLabel.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            rulesLabel.ForeColor = Color.Black;
            rulesLabel.BackColor = Color.Transparent;
            rulesLabel.AutoSize = true;
            rulesLabel.MaximumSize = new Size(600, 0); // Optional wrap width

            // ✨ Position under the grid and center it
            int gridWidth = cols * cellSize;
            int gridCenterX = gridWidth / 2;
            int rulesX = gridCenterX - (rulesLabel.PreferredWidth / 2);
            int rulesY = rows * cellSize + 20;

            rulesLabel.Location = new Point(rulesX, rulesY);

            this.Controls.Add(rulesLabel);

            this.DoubleBuffered = true; // Reduce flicker
            currentGrid = new bool[rows, cols];
            nextGrid = new bool[rows, cols];

            // Random initial state
            Random rand = new Random();
            for (int y = 0; y < rows; y++)
                for (int x = 0; x < cols; x++)
                    currentGrid[y, x] = rand.Next(2) == 0;

            // Timer setup
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 100; // 100 ms
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Application.Exit();
            }
            else if (e.KeyCode == Keys.Space)
            {
                isPaused = !isPaused;
            }

            if (pauseLabel != null)
            {
                pauseLabel.Visible = isPaused;
            }
            else if (e.KeyCode == Keys.R && isPaused && overlayPanel != null)
            {
                this.Controls.Remove(overlayPanel);
                overlayPanel.Dispose();
                overlayPanel = null;
                isPaused = false;
                stagnantFrameCount = 0;
                timer?.Start();
            }

        }


        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (isPaused || currentGrid == null || nextGrid == null) return;

            currentlyAlive = 0;
            int birthsThisGen = 0;
            int deathsThisGen = 0;

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    bool wasAlive = currentGrid[y, x];
                    bool willBeAlive;

                    int aliveNeighbors = CountNeighbors(x, y);

                    // Apply Conway's rules
                    if (wasAlive && (aliveNeighbors < 2 || aliveNeighbors > 3))
                        willBeAlive = false;
                    else if (!wasAlive && aliveNeighbors == 3)
                        willBeAlive = true;
                    else
                        willBeAlive = wasAlive;

                    // Track births and deaths
                    if (wasAlive && !willBeAlive)
                    {
                        totalDeaths++;
                        deathsThisGen++;
                    }
                    else if (!wasAlive && willBeAlive)
                    {
                        totalBirths++;
                        birthsThisGen++;
                    }

                    if (willBeAlive)
                        currentlyAlive++;

                    nextGrid[y, x] = willBeAlive;
                }
            }

            generationCount++;

            statHistory.Add(new GenerationStats
            {
                Generation = generationCount,
                Alive = currentlyAlive,
                Births = birthsThisGen,
                Deaths = deathsThisGen
            });

            UpdateSummaryLabels();

            // Swap grids
            var temp = currentGrid;
            currentGrid = nextGrid;
            nextGrid = temp;

            // Optional: update stats label
            if (statsLabel != null)
            {
                statsLabel.Text = $"Gen: {generationCount} | Alive: {currentlyAlive} | Births: {totalBirths} | Deaths: {totalDeaths}";
            }

           

            if (logPanel != null)
            {
                Label genLabel = new Label();
                genLabel.Text = $"Gen {generationCount}: Alive={currentlyAlive}  Births=+{birthsThisGen}  Deaths=-{deathsThisGen}";
                genLabel.Font = new Font("Segoe UI", 10, FontStyle.Regular);
                genLabel.ForeColor = Color.Black;
                genLabel.AutoSize = true;
                genLabel.MaximumSize = new Size(logPanel.Width - 10, 0); // Optional wrap

                // Stack vertically after the last label
                int yOffset = 0;
                if (logPanel.Controls.Count > 0)
                {
                    var lastLabel = logPanel.Controls[logPanel.Controls.Count - 1];
                    yOffset = lastLabel.Bottom + 5;
                }
                genLabel.Location = new Point(0, yOffset);

                logPanel.Controls.Add(genLabel);

                // 👇 Scroll to bottom
                logPanel.ScrollControlIntoView(genLabel);
            }



            DetectStagnation();
            this.Invalidate(); // Redraw
        }

        private int CountNeighbors(int x, int y)
        {
            int count = 0;
            for (int j = -1; j <= 1; j++)
            {
                for (int i = -1; i <= 1; i++)
                {
                    if (i == 0 && j == 0) continue;

                    int nx = x + i;
                    int ny = y + j;

                    if (nx >= 0 && nx < cols && ny >= 0 && ny < rows)
                        if (currentGrid[ny, nx]) count++;
                }
            }
            return count;
        }

        private void UpdateSummaryLabels()
        {
            if (statHistory.Count == 0) return;

            var highestBirth = statHistory.OrderByDescending(s => s.Births).FirstOrDefault();
            var lowestBirth = statHistory.OrderBy(s => s.Births).FirstOrDefault();
            var highestDeath = statHistory.OrderByDescending(s => s.Deaths).FirstOrDefault();
            var lowestDeath = statHistory.OrderBy(s => s.Deaths).FirstOrDefault();

            if (highestBirth != null && highestBirthLabel != null)
                highestBirthLabel.Text = $"Highest Birthrate: Gen {highestBirth.Generation} | Alive: {highestBirth.Alive} | Births: {highestBirth.Births}";

            if (lowestBirth != null && lowestBirthLabel != null)
                lowestBirthLabel.Text = $"Lowest Birthrate: Gen {lowestBirth.Generation} | Alive: {lowestBirth.Alive} | Births: {lowestBirth.Births}";

            if (highestDeath != null && highestDeathLabel != null)
                highestDeathLabel.Text = $"Highest Deathrate: Gen {highestDeath.Generation} | Alive: {highestDeath.Alive} | Deaths: {highestDeath.Deaths}";

            if (lowestDeath != null && lowestDeathLabel != null)
                lowestDeathLabel.Text = $"Lowest Deathrate: Gen {lowestDeath.Generation} | Alive: {lowestDeath.Alive} | Deaths: {lowestDeath.Deaths}";
        }

        private void DetectStagnation()
        {
            if (currentGrid == null) return;

            string hash = GetGridHash(currentGrid);

            if (recentHashes.Contains(hash))
            {
                stagnantFrameCount++;
            }
            else
            {
                stagnantFrameCount = 0;
                recentHashes.Add(hash);
                hashQueue.Enqueue(hash);

                if (hashQueue.Count > hashMemory)
                {
                    string oldHash = hashQueue.Dequeue();
                    recentHashes.Remove(oldHash);
                }
            }

            if (stagnantFrameCount >= stagnationThreshold)
            {
                isPaused = true;

                overlayPanel = new Panel();
                overlayPanel.BackColor = Color.FromArgb(180, 0, 0, 0);
                overlayPanel.Bounds = this.ClientRectangle;
                overlayPanel.BringToFront();
                this.Controls.Add(overlayPanel);

                stagnationLabel = new Label();
                stagnationLabel.Text = "STAGNATION ACHIEVED";
                stagnationLabel.Font = new Font("Segoe UI", 48, FontStyle.Bold);
                stagnationLabel.ForeColor = Color.White;
                stagnationLabel.BackColor = Color.Transparent;
                stagnationLabel.AutoSize = true;

                int centerX = (this.ClientSize.Width - stagnationLabel.PreferredWidth) / 2;
                int centerY = (this.ClientSize.Height - stagnationLabel.PreferredHeight) / 2;
                stagnationLabel.Location = new Point(centerX, centerY);

                overlayPanel.Controls.Add(stagnationLabel);
                overlayPanel.BringToFront();

                // Create a sub-label for the generation number
                Label generationLabel = new Label();
                generationLabel.Text = $"Generation: {generationCount}";
                generationLabel.Font = new Font("Segoe UI", 24, FontStyle.Regular);
                generationLabel.ForeColor = Color.White;
                generationLabel.BackColor = Color.Transparent;
                generationLabel.AutoSize = true;

                // Position it just below the main message
                int labelX = (this.ClientSize.Width - generationLabel.PreferredWidth) / 2;
                int labelY = stagnationLabel.Location.Y + stagnationLabel.Height + 20;
                generationLabel.Location = new Point(labelX, labelY);

                // Add to overlay
                overlayPanel.Controls.Add(generationLabel);

                timer?.Stop();
            }
        }

        private string GetGridHash(bool[,] grid)
        {
            var sb = new System.Text.StringBuilder();
            for (int y = 0; y < rows; y++)
                for (int x = 0; x < cols; x++)
                    sb.Append(grid[y, x] ? '1' : '0');
            return sb.ToString();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Brush aliveBrush = Brushes.Black;
            Pen gridPen = Pens.Gray;

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    Rectangle cell = new Rectangle(x * cellSize, y * cellSize, cellSize, cellSize);
                    if (currentGrid[y, x])
                        g.FillRectangle(aliveBrush, cell);

                    g.DrawRectangle(gridPen, cell);
                }
            }
        }



    }
}