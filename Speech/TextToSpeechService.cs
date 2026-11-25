using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.SpeechSynthesis;

namespace SpeechApp.Speech;

public enum SpeakState { Idle, Speaking, Paused }

/// <summary>Metni Windows'un Türkçe sesiyle okur; duraklatma ve durdurma sağlar.</summary>
public sealed class TextToSpeechService : IDisposable
{
    private readonly SpeechSynthesizer _synthesizer = new();
    private readonly MediaPlayer _player = new();
    private readonly SynchronizationContext _ui;
    private int _request; // durdurulan isteğin sonradan çalmasını önler

    public SpeakState State { get; private set; }
    public event Action? StateChanged;
    public event Action<string>? Failed;

    /// <summary>Arayüz iş parçacığında oluşturulmalıdır.</summary>
    public TextToSpeechService()
    {
        _ui = SynchronizationContext.Current ?? new SynchronizationContext();

        // MediaPlayer olayları başka iş parçacığından gelir, arayüze aktarılır
        _player.MediaEnded += (_, _) => _ui.Post(_ => SetState(SpeakState.Idle), null);
        _player.MediaFailed += (_, e) => _ui.Post(_ =>
        {
            SetState(SpeakState.Idle);
            Failed?.Invoke("Ses çalınamadı: " + e.ErrorMessage);
        }, null);
    }

    public async Task SpeakAsync(string text)
    {
        VoiceInformation voice = SpeechSynthesizer.AllVoices.FirstOrDefault(v => v.Language.StartsWith("tr"))
            ?? throw new InvalidOperationException(
                "Türkçe ses bulunamadı. Windows Ayarları > Saat ve dil > Konuşma bölümünden ekleyin.");

        Stop();
        int request = _request;
        SetState(SpeakState.Speaking);

        try
        {
            _synthesizer.Voice = voice;
            SpeechSynthesisStream stream = await _synthesizer.SynthesizeTextToStreamAsync(text);
            if (request != _request) return; // bu arada durdurulmuş

            _player.Source = MediaSource.CreateFromStream(stream, stream.ContentType);
            if (State == SpeakState.Speaking) _player.Play();
        }
        catch
        {
            SetState(SpeakState.Idle);
            throw;
        }
    }

    /// <summary>Okurken duraklatır, duraklatılmışken devam ettirir.</summary>
    public void TogglePause()
    {
        if (State == SpeakState.Speaking)
        {
            _player.Pause();
            SetState(SpeakState.Paused);
        }
        else if (State == SpeakState.Paused)
        {
            _player.Play();
            SetState(SpeakState.Speaking);
        }
    }

    public void Stop()
    {
        _request++;
        _player.Pause();
        _player.Source = null;
        SetState(SpeakState.Idle);
    }

    private void SetState(SpeakState state)
    {
        State = state;
        StateChanged?.Invoke();
    }

    public void Dispose()
    {
        _player.Dispose();
        _synthesizer.Dispose();
    }
}
