namespace SpeechApp;

/// <summary>Arayüz: üstte metinden sese, altta sesten metne bölümü.</summary>
public class SpeechForm : Form
{
    private readonly TextBox _speakBox = new() { Multiline = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Vertical };
    private readonly Button _speakButton = new() { Text = "Başlat", AutoSize = true };
    private readonly Button _pauseButton = new() { Text = "Duraklat", AutoSize = true, Enabled = false };
    private readonly Button _stopButton = new() { Text = "Durdur", AutoSize = true, Enabled = false };
    private readonly Button _clearSpeakButton = new() { Text = "Temizle", AutoSize = true };

    private readonly TextBox _listenBox = new() { Multiline = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Vertical };
    private readonly Button _listenButton = new() { Text = "Dinlemeyi Başlat", AutoSize = true };
    private readonly Button _stopListenButton = new() { Text = "Dinlemeyi Durdur", AutoSize = true, Enabled = false };
    private readonly Button _clearListenButton = new() { Text = "Temizle", AutoSize = true };
    private readonly Label _partialLabel = new() { Dock = DockStyle.Bottom, Height = 24, ForeColor = Color.DimGray };

    private readonly Label _statusLabel = new()
    {
        Dock = DockStyle.Top,
        Height = 36,
        TextAlign = ContentAlignment.MiddleCenter,
        Font = new Font("Segoe UI", 11, FontStyle.Bold),
        BackColor = Color.Gainsboro,
        Text = "Hazır"
    };

    public SpeechForm()
    {
        Text = "Konuşma Uygulaması";
        ClientSize = new Size(620, 520);
        MinimumSize = new Size(520, 440);
        StartPosition = FormStartPosition.CenterScreen;

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        layout.Controls.Add(Section("Metinden Sese", _speakBox,
            _speakButton, _pauseButton, _stopButton, _clearSpeakButton), 0, 0);

        GroupBox listen = Section("Sesten Metne", _listenBox,
            _listenButton, _stopListenButton, _clearListenButton);
        listen.Controls.Add(_partialLabel);
        layout.Controls.Add(listen, 0, 1);

        Controls.Add(layout);
        Controls.Add(_statusLabel);
    }

    private static GroupBox Section(string title, Control content, params Control[] buttons)
    {
        var bar = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true };
        bar.Controls.AddRange(buttons);

        var box = new GroupBox { Text = title, Dock = DockStyle.Fill, Padding = new Padding(8) };
        box.Controls.Add(content);
        box.Controls.Add(bar);
        return box;
    }
}
