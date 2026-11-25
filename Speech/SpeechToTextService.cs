using System.Text.Json;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using Vosk;

namespace SpeechApp.Speech;

/// <summary>Mikrofonu NAudio ile okur, Vosk modeliyle internetsiz olarak metne çevirir.</summary>
public sealed class SpeechToTextService : IDisposable
{
    private const int SampleRate = 16000; // modelin beklediği örnekleme hızı

    private readonly string _modelPath;
    private Model? _model; // bir kez yüklenir
    private VoskRecognizer? _recognizer;
    private WaveInEvent? _microphone;

    public bool IsListening { get; private set; }
    public event Action<string>? TextRecognized;    // tamamlanan cümle
    public event Action<string>? PartialRecognized; // henüz bitmeyen cümle
    public event Action? ListeningChanged;
    public event Action<int>? LevelChanged;          // gelen sesin seviyesi (0-100)
    public event Action<string>? Failed;

    public SpeechToTextService(string modelPath) => _modelPath = modelPath;

    /// <summary>Model veya mikrofon yoksa açıklayıcı bir hata fırlatır.</summary>
    public void Start()
    {
        if (IsListening || _microphone != null) return;

        if (WaveInEvent.DeviceCount == 0)
            throw new InvalidOperationException("Mikrofon bulunamadı.");

        // Sessize alınmış mikrofon hata vermez, yalnızca sessizlik gönderir.
        if (IsMicrophoneMuted())
            throw new InvalidOperationException(
                "Mikrofon sessize alınmış. Windows Ayarları > Sistem > Ses > Giriş bölümünden sesini açın.");

        LoadModel();

        try
        {
            _recognizer = new VoskRecognizer(_model, SampleRate);
            _microphone = new WaveInEvent { WaveFormat = new WaveFormat(SampleRate, 16, 1) };
            _microphone.DataAvailable += OnData;
            _microphone.RecordingStopped += OnStopped;
            _microphone.StartRecording();
        }
        catch
        {
            Release();
            throw;
        }

        IsListening = true;
        ListeningChanged?.Invoke();
    }

    // Son cümle, kayıt tamamen durunca OnStopped içinde alınır.
    public void Stop() => _microphone?.StopRecording();

    private static bool IsMicrophoneMuted()
    {
        using var devices = new MMDeviceEnumerator();
        using MMDevice microphone = devices.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console);
        return microphone.AudioEndpointVolume.Mute;
    }

    // 16 bitlik örneklerin en yükseğini 0-100 arasına çevirir.
    private static int PeakLevel(byte[] buffer, int count)
    {
        int peak = 0;
        for (int i = 0; i + 1 < count; i += 2)
            peak = Math.Max(peak, Math.Abs((int)BitConverter.ToInt16(buffer, i)));

        return Math.Min(100, peak * 100 / short.MaxValue);
    }

    private void LoadModel()
    {
        if (_model != null) return;

        // Dosya yoksa yerel kütüphane çöker; önce kontrol edilir.
        if (!File.Exists(Path.Combine(_modelPath, "final.mdl")))
            throw new FileNotFoundException(
                "Konuşma modeli bulunamadı. README'deki adımlarla 'model' klasörünü ekleyin: " + _modelPath);

        Vosk.Vosk.SetLogLevel(-1);
        _model = new Model(_modelPath);
    }

    private void OnData(object? sender, WaveInEventArgs e)
    {
        if (_recognizer == null) return;

        LevelChanged?.Invoke(PeakLevel(e.Buffer, e.BytesRecorded));

        if (_recognizer.AcceptWaveform(e.Buffer, e.BytesRecorded))
        {
            Report(ReadField(_recognizer.Result(), "text"));
            PartialRecognized?.Invoke("");
        }
        else
        {
            PartialRecognized?.Invoke(ReadField(_recognizer.PartialResult(), "partial"));
        }
    }

    private void OnStopped(object? sender, StoppedEventArgs e)
    {
        if (_recognizer != null)
            Report(ReadField(_recognizer.FinalResult(), "text"));

        Release();
        IsListening = false;
        PartialRecognized?.Invoke("");
        LevelChanged?.Invoke(0);
        ListeningChanged?.Invoke();

        if (e.Exception != null)
            Failed?.Invoke("Mikrofon hatası: " + e.Exception.Message);
    }

    private void Report(string text)
    {
        if (text.Length > 0) TextRecognized?.Invoke(text);
    }

    // Vosk sonucu JSON olarak verir: {"text": "..."} veya {"partial": "..."}
    private static string ReadField(string json, string name)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        return document.RootElement.TryGetProperty(name, out JsonElement value) ? value.GetString() ?? "" : "";
    }

    private void Release()
    {
        _microphone?.Dispose();
        _microphone = null;
        _recognizer?.Dispose();
        _recognizer = null;
    }

    public void Dispose()
    {
        if (_microphone != null)
        {
            // pencere kapanırken olaylar artık arayüze gitmesin
            _microphone.DataAvailable -= OnData;
            _microphone.RecordingStopped -= OnStopped;
            _microphone.StopRecording();
        }

        Release();
        _model?.Dispose();
    }
}
