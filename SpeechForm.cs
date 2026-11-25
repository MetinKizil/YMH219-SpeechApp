using SpeechApp.Speech;

namespace SpeechApp;

/// <summary>Arayüz: üstte metinden sese, altta sesten metne bölümü.</summary>
public class SpeechForm : Form
{
    private readonly TextToSpeechService _tts;
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

        _tts = new TextToSpeechService(); // arayüz hazır olduktan sonra oluşturulur
        _tts.StateChanged += RefreshState;
        _tts.Failed += ShowError;
        _speakButton.Click += async (_, _) => await SpeakAsync();
        _pauseButton.Click += (_, _) => _tts.TogglePause();
        _stopButton.Click += (_, _) => _tts.Stop();
        _clearSpeakButton.Click += (_, _) => { _tts.Stop(); _speakBox.Clear(); };
        FormClosed += (_, _) => _tts.Dispose();
    }

    private async Task SpeakAsync()
    {
        string text = _speakBox.Text.Trim();
        if (text.Length == 0)
        {
            MessageBox.Show(this, "Okunacak metni yazın.", Text);
            return;
        }

        try
        {
            await _tts.SpeakAsync(text);
        }
        catch (Exception ex)
        {
            ShowError("Metin okunamadı: " + ex.Message);
        }
    }

    // Hangi işlemin etkin olduğunu gösterir ve butonları buna göre ayarlar.
    private void RefreshState()
    {
        bool speaking = _tts.State != SpeakState.Idle;

        _statusLabel.Text = _tts.State switch
        {
            SpeakState.Speaking => "Metin okunuyor...",
            SpeakState.Paused => "Okuma duraklatıldı",
            _ => "Hazır"
        };
        _statusLabel.BackColor = speaking ? Color.LightSkyBlue : Color.Gainsboro;

        _speakButton.Enabled = !speaking;
        _pauseButton.Enabled = _stopButton.Enabled = speaking;
        _pauseButton.Text = _tts.State == SpeakState.Paused ? "Devam Et" : "Duraklat";
    }

    private void ShowError(string message) =>
        MessageBox.Show(this, message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);

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
