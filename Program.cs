using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.Run(new MainForm());
    }
}
public class PointData
{
    public string Comment { get; set; }
    public double? X { get; set; }
    public double? Y { get; set; }
}

public class CommentedPointsForm : Form
{
    public event Action<PointData, bool> PointSelectedInEditor;
    private Button saveButton;
    private FlowLayoutPanel pointsPanel;
    private Button addPointButton;
    private List<PointRow> pointRows = new List<PointRow>();
    private Button removePointButton;
    private Button loadButton;
    private void LoadFromFile()
    {
        using (OpenFileDialog dialog = new OpenFileDialog())
        {
            dialog.Filter = "Текстовые файлы (*.txt)|*.txt";
            dialog.Title = "Загрузить список точек";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var lines = File.ReadAllLines(dialog.FileName);
                pointsPanel.Controls.Clear();
                pointRows.Clear();

                foreach (var line in lines)
                {
                    var parts = line.Split(';');
                    var comment = parts.Length > 0 ? parts[0] : "";
                    var x = parts.Length > 1 && double.TryParse(parts[1], out var xVal) ? xVal : (double?)null;
                    var y = parts.Length > 2 && double.TryParse(parts[2], out var yVal) ? yVal : (double?)null;

                    var row = new PointRow();
                    row.SetData(new PointData { Comment = comment, X = x, Y = y });

                    pointRows.Add(row);
                    pointsPanel.Controls.Add(row.GetPanel());
                }

                MessageBox.Show("Загружено успешно.", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
    private void OnPointSelected(PointData point, bool isStart)
    {
        PointSelectedInEditor?.Invoke(point, isStart);
    }
    private void AddPointRow()
    {
        var row = new PointRow();
        row.PointSelected += OnPointSelected;
        pointRows.Add(row);
        pointsPanel.Controls.Add(row.GetPanel());
    }
    private void SaveToFile()
    {
        using (SaveFileDialog dialog = new SaveFileDialog())
        {
            dialog.Filter = "Текстовые файлы (*.txt)|*.txt";
            dialog.Title = "Сохранить список точек";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter writer = new StreamWriter(dialog.FileName))
                {
                    foreach (var row in pointRows)
                    {
                        var point = row.ToPointData();
                        writer.WriteLine($"{point.Comment};{point.X};{point.Y}");
                    }
                }
                MessageBox.Show("Сохранено успешно.", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
    public CommentedPointsForm(bool darkTheme = false)
    {
        Text = "Точки";
        Size = new Size(550, 500);
        StartPosition = FormStartPosition.CenterScreen;
        pointsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoScroll = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Height = 300,
            Width = 500
        };
        Controls.Add(pointsPanel);
        addPointButton = new Button
        {
            Text = "Добавить точку",
            Dock = DockStyle.Bottom,
            Height = 40
        };
        removePointButton = new Button
        {
            Text = "Удалить последнюю точку",
            Dock = DockStyle.Bottom,
            Height = 40
        };
        saveButton = new Button
        {
            Text = "Сохранить в файл",
            Dock = DockStyle.Bottom,
            Height = 40
        };
        loadButton = new Button
        {
            Text = "Загрузить из файла",
            Dock = DockStyle.Bottom,
            Height = 40
        };
        loadButton.Click += (s, e) => LoadFromFile();
        Controls.Add(loadButton);
        saveButton.Click += (s, e) => SaveToFile();
        Controls.Add(saveButton);
        removePointButton.Click += (s, e) => RemoveLastPoint();
        Controls.Add(removePointButton);
        addPointButton.Click += (s, e) => AddPointRow();
        Controls.Add(addPointButton);
        AddPointRow();
        ApplyTheme(darkTheme);
    }
    private void ApplyTheme(bool dark)
    {
        Color bg = dark ? Color.FromArgb(32, 32, 32) : Color.White;
        Color fg = dark ? Color.White : Color.Black;

        BackColor = bg;
        ForeColor = fg;

        foreach (Control ctrl in Controls)
        {
            ApplyToControl(ctrl, dark, fg, bg);
        }
        foreach (var row in pointRows)
        {
            ApplyToControl(row.GetPanel(), dark, fg, bg);
        }
    }
    private void ApplyToControl(Control ctrl, bool dark, Color fg, Color bg)
    {
        if (ctrl is Label || ctrl is CheckBox)
        {
            ctrl.ForeColor = fg;
            ctrl.BackColor = bg;
        }
        else if (ctrl is TextBox tb)
        {
            tb.BackColor = dark ? Color.FromArgb(48, 48, 48) : Color.White;
            tb.ForeColor = fg;
        }
        else if (ctrl is Button btn)
        {
            btn.BackColor = dark ? Color.FromArgb(64, 64, 64) : SystemColors.Control;
            btn.ForeColor = fg;
        }
        else if (ctrl.HasChildren)
        {
            foreach (Control child in ctrl.Controls)
            {
                ApplyToControl(child, dark, fg, bg);
            } 
        }
    }
    private void RemoveLastPoint()
    {
        if (pointRows.Count > 0)
        {
            var last = pointRows[pointRows.Count - 1];
            pointsPanel.Controls.Remove(last.GetPanel());
            pointRows.RemoveAt(pointRows.Count - 1);
        }
    }
    private class PointRow
    {
        public event Action<PointData, bool> PointSelected; // bool = true если начальная, false если конечная
        private Button startButton = new Button { Text = "Начальная", Width = 80, Height = 25 };
        private Button endButton = new Button { Text = "Конечная", Width = 80, Height = 25 };
        private TextBox commentBox = new TextBox { PlaceholderText = "Комментарий", Width = 220 };
        private TextBox xBox = new TextBox { PlaceholderText = "X", Width = 80 };
        private TextBox yBox = new TextBox { PlaceholderText = "Y", Width = 80 };
        

        private Panel panel = new Panel { Width = 500, Height = 60 };
        public void SetData(PointData data)
        {
            commentBox.Text = data.Comment;
            xBox.Text = data.X?.ToString() ?? "";
            yBox.Text = data.Y?.ToString() ?? "";
        }

        public PointRow()
        {

            commentBox.Location = new Point(0, 5);
            xBox.Location = new Point(230, 5);
            yBox.Location = new Point(320, 5);

            panel.Controls.Add(commentBox);
            panel.Controls.Add(xBox);
            panel.Controls.Add(yBox);

            commentBox.Location = new Point(0, 5);
            xBox.Location = new Point(230, 5);
            yBox.Location = new Point(320, 5);

            startButton.Location = new Point(410, 2);
            endButton.Location = new Point(410, 32);

            startButton.Click += (s, e) => PointSelected?.Invoke(ToPointData(), true);
            endButton.Click += (s, e) => PointSelected?.Invoke(ToPointData(), false);

            panel.Controls.Add(commentBox);
            panel.Controls.Add(xBox);
            panel.Controls.Add(yBox);
            panel.Controls.Add(startButton);
            panel.Controls.Add(endButton);
        }

        public Panel GetPanel() => panel;

        public PointData ToPointData()
        {
            double.TryParse(xBox.Text, out double x);
            double.TryParse(yBox.Text, out double y);
            return new PointData
            {
                Comment = commentBox.Text,
                X = string.IsNullOrWhiteSpace(xBox.Text) ? null : x,
                Y = string.IsNullOrWhiteSpace(yBox.Text) ? null : y

            };

        }
    }
}
public class MainForm : Form
{
    TextBox inputX1, inputX2, inputY1, inputY2;
    Label resultLabel;
    Label distanceLabel;
    CheckBox themeToggle;
    string themeFilePath = "theme.txt";

    public MainForm()
    {
        var editorButton = new Button
        {
            Text = "Редактор точек",
            Location = new Point(140, 220),
            Height = 30,
            Width = 120
        };
        editorButton.Click += (s, e) =>
        {
            var editor = new CommentedPointsForm(themeToggle.Checked);
            editor.PointSelectedInEditor += (point, isStart) =>
            {
                if (point != null)
                {
                    if (isStart)
                    {
                        inputX1.Text = point.X?.ToString() ?? "";
                        inputY1.Text = point.Y?.ToString() ?? "";
                    }
                    else
                    {
                       inputX2.Text = point.X?.ToString() ?? "";
                        inputY2.Text = point.Y?.ToString() ?? ""; 
                    }
                }

            };
            editor.ShowDialog();
        };
        Text = "Калькулятор азимута by xeon93.";
        Width = 400;
        Height = 300;

        // Лейблы полей
        Controls.Add(new Label { Text = "Координата по X1", Location = new Point(20, 3), AutoSize = true });
        Controls.Add(new Label { Text = "Координата по X2", Location = new Point(20, 50), AutoSize = true });
        Controls.Add(new Label { Text = "Координата по Y1", Location = new Point(260, 3), AutoSize = true });
        Controls.Add(new Label { Text = "Координата по Y2", Location = new Point(260, 50), AutoSize = true });

        // Поля для ввода
        inputX1 = new TextBox { Location = new Point(35, 25), Width = 60 };
        inputX2 = new TextBox { Location = new Point(35, 75), Width = 60 };
        inputY1 = new TextBox { Location = new Point(280, 25), Width = 60 };
        inputY2 = new TextBox { Location = new Point(280, 75), Width = 60 };

        Controls.Add(inputX1);
        Controls.Add(inputX2);
        Controls.Add(inputY1);
        Controls.Add(inputY2);

        // кнопачка
        var button = new Button
        {
            Location = new Point(140, 140),
            Text = "Рассчитать",
            Height = 40,
            Width = 120
        };
        button.Click += OnCalculateClick;
        Controls.Add(button);

        // лэйбл для результата
        resultLabel = new Label
        {
            Location = new Point(130, 90),
            Font = new Font("Arial", 12, FontStyle.Bold),
            AutoSize = true,
            ForeColor = Color.DarkBlue
        };
        Controls.Add(resultLabel);
        //Лэйбл дитсанции

        distanceLabel = new Label
        {
            Location = new Point(130, 110),
            Font = new Font("Arial", 12, FontStyle.Bold),
            AutoSize = true
        };
        //переключалка темы
        themeToggle = new CheckBox
        {
            Text = "Сохранить свои глаза",
            Location = new Point(140, 180),
            AutoSize = true
        };
        themeToggle.CheckedChanged += (s, e) =>
        {
            ApplyTheme(themeToggle.Checked);
            SaveTheme(themeToggle.Checked);
        };
         Controls.Add(editorButton);
        Controls.Add(themeToggle);
        Controls.Add(distanceLabel);
        //Загрузка темы
        bool dark = LoadSavedTheme();
        themeToggle.Checked = dark;
        ApplyTheme(dark);
    }

    private void ApplyTheme(bool dark)
    {
        Color bg = dark ? Color.FromArgb(32, 32, 32) : Color.White;
        Color fg = dark ? Color.White : Color.Black;

        BackColor = bg;
        ForeColor = fg;

        foreach (Control ctrl in Controls)
        {
            if (ctrl is Label || ctrl is CheckBox)
            {
                ctrl.ForeColor = fg;
                ctrl.BackColor = bg;
            }
            else if (ctrl is TextBox tb)
            {
                tb.BackColor = dark ? Color.FromArgb(48, 48, 48) : Color.White;
                tb.ForeColor = fg;
            }
            else if (ctrl is Button btn)
            {
                btn.BackColor = dark ? Color.FromArgb(64, 64, 64) : SystemColors.Control;
                btn.ForeColor = fg;
            }
        }
    }
    //логика рассчёта 
    private void OnCalculateClick(object sender, EventArgs e)
    {
        try
        {
            double x1 = double.Parse(inputX1.Text);
            double x2 = double.Parse(inputX2.Text);
            double y1 = double.Parse(inputY1.Text);
            double y2 = double.Parse(inputY2.Text);

            double dx = x2 - x1;
            double dy = y2 - y1;

            double angleDeg = Math.Atan2(dx, dy) * 180.0 / Math.PI;

            if (angleDeg > 180)
                angleDeg -= 360;
            else if (angleDeg < -180)
                angleDeg += 360;

            double distance = Math.Sqrt(dx * dx + dy * dy);
            resultLabel.Text = $"Азимут: {angleDeg:F2}°";
            distanceLabel.Text = $"Расстояние: {distance:F2}";

        }
        catch
        {
            MessageBox.Show("леееее Дон цыфры мне набери!", "ащибка Дон", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    private void SaveTheme(bool dark)
    {
        try
        {
            File.WriteAllText(themeFilePath, dark ? "dark" : "light");
        }
        catch { /* проигнорировать */ }
    }
    private bool LoadSavedTheme()
    {
        try
        {
            if (File.Exists(themeFilePath))
            {
                string content = File.ReadAllText(themeFilePath).Trim().ToLower();
                return content == "dark";
            }
        }
        catch { }
        return false; // по умолчанию — светлая
    }
}