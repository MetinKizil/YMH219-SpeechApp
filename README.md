# YMH219 - Konuşma Uygulaması (Speech-to-Text / Text-to-Speech)

Nesne Tabanlı Programlama (YMH 219) dersi için yazılan metni Türkçe okuyan ve mikrofondan alınan konuşmayı metne çeviren Windows Forms uygulaması.

**Öğrenci:** _(adını soyadını yaz)_
**Öğrenci No:** _(öğrenci numaranı yaz)_

## Kullanılan Teknolojiler / NuGet Paketleri

- C# / .NET 8, Windows Forms
- Metinden sese: Windows `Windows.Media.SpeechSynthesis` (Türkçe ses)
- Sesten metne: [Vosk](https://alphacephei.com/vosk/) 0.3.38 (internetsiz tanıma)
- Mikrofon: NAudio 2.2.1

## Kurulum ve Çalıştırma

Gereksinim: Windows 10/11, .NET 8 SDK, Türkçe konuşma sesi ve mikrofon.

1. Depoyu indir.
2. [Vosk model sayfasından](https://alphacephei.com/vosk/models) `vosk-model-small-tr-0.3.zip` dosyasını indir (yaklaşık 35 MB).
3. Zip içindeki `vosk-model-small-tr-0.3` klasörünü proje klasörüne çıkar ve adını `model` yap. `model/final.mdl` dosyası bulunmalıdır.
4. Çalıştır:

```bash
dotnet run
```

Mikrofon algılanmazsa: Windows Ayarları > Gizlilik ve güvenlik > Mikrofon bölümünden masaüstü uygulamalarının erişimine izin ver.

## Temel Özellikler

- Yazılan metni okutma; Başlat, Duraklat/Devam Et, Durdur ve Temizle
- Mikrofondan konuşmayı metne aktarma, konuşurken ara sonucu gösterme
- Etkin işlemi gösteren durum çubuğu
- Mikrofon, ses veya model bulunamadığında anlaşılır hata mesajı

## Sınıf Yapısı

| Sınıf | Görevi |
|---|---|
| `TextToSpeechService` | Metni sese çevirme, duraklatma, durdurma |
| `SpeechToTextService` | Mikrofon kaydı ve konuşma tanıma |
| `SpeechForm` | Arayüz |

## Ekran Görüntüleri

_(en az 2 ekran görüntüsü eklenecek)_

## Bilinen Sınırlamalar

- Küçük Türkçe model her kelimeyi doğru tanıyamaz; noktalama ve büyük harf üretmez.
- Konuşma modeli depoya eklenmemiştir, ayrıca indirilmelidir.
- Okuma ve dinleme aynı anda yapılamaz.
