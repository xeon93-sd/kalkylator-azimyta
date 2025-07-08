using System;
using System.Drawing;
using System.Windows.Forms;

class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.Run(new MainForm());
    }
}

public class MainForm : Form
{
    TextBox inputX1, inputX2, inputY1, inputY2;
    Label resultLabel;

    public MainForm()
    {
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
            Location = new Point(130, 80),
            Font = new Font("Arial", 12, FontStyle.Bold),
            AutoSize = true,
            ForeColor = Color.DarkBlue
        };
        Controls.Add(resultLabel);
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

            resultLabel.Text = $"Азимут: {angleDeg:F2}°";
        }
        catch
        {
            MessageBox.Show("леееее Дон цыфры мне набери!", "ащибка Дон", MessageBoxButtons.OK, MessageBoxIcon.Error );
        }
    }
}
