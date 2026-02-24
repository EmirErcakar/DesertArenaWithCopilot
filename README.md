# 🏜️ DesertArena — Mobil Arena Hayatta Kalma Oyunu

Dikey mobil, üstten bakışlı, düşük poligonlu 3D arena hayatta kalma oyunu. Unity 6.3 LTS (6000.3.9f1), Universal Render Pipeline (URP) ile geliştirilmiştir.

> **🇹🇷 Yeni başlayan mısınız?** → **[KURULUM_REHBERI.md](KURULUM_REHBERI.md)** dosyasını okuyun! Sıfırdan adım adım anlatılmıştır.

---

## 🎮 Oyun Hakkında

**DesertArena** çöl temalı bir dalga bazlı hayatta kalma oyunudur:

- 📱 Dikey mobil oyun (telefon dikey tutularak oynanır)
- 🕹️ Joystick ile hareket, otomatik ateş sistemi
- 👾 Dalga bazlı düşmanlar — bıçaklı, kılıçlı, uzak mesafe ve boss
- 🏆 20 arena × 35 seviye = 700 seviye toplam içerik
- ⬆️ XP sistemi ile güçlenme kartları (Pierce, Multishot, Ricochet vb.)
- 💰 Coin, boss sandığı, günlük ödül ve meta yükseltme sistemi
- 🔄 Seviye başına 3 canlanma hakkı (reklam sistemi stub olarak hazır)
- 👗 Kostüm sistemi (küçük bonuslarla, pay-to-win değil)

## 📁 Proje Yapısı

```
Assets/Scripts/
├── Core/               # GameManager (oyun durumu) ve EventBus (olay sistemi)
├── Player/             # Karakter hareketi, can, nişan alma, ateş etme
├── Camera/             # Kamera takibi (üstten açılı bakış)
├── Enemies/            # Düşman tipleri: Bıçak, Kılıç, Uzak Mesafe, Boss
├── Projectiles/        # Mermi sistemi ve nesne havuzu (object pool)
├── Levels/             # Seviye yöneticisi ve arena verileri
├── XP/                 # Deneyim puanı ve güçlenme kartları
├── UI/                 # Joystick, HUD, kart ekranı, boss geri sayımı vb.
├── Economy/            # Coin, boss sandığı, günlük ödül, meta yükseltme
├── Revive/             # Canlanma sistemi ve reklam simülasyonu
├── Skins/              # Kostüm verileri ve yöneticisi
├── Progression/        # İlerleme sistemi (20 arena × 35 seviye)
└── Utilities/          # Singleton tasarım kalıbı
```

## 🚀 Hızlı Başlangıç

1. **Unity Hub** kurun → [unity.com/download](https://unity.com/download)
2. **Unity 6.3 LTS** sürümünü kurun (Unity Hub → Installs → Install Editor)
3. Yeni proje oluşturun: **Universal 3D (URP)** şablonu
4. Bu repo'daki `Assets/Scripts/` klasörünü projenizin `Assets/` içine kopyalayın
5. Unity'de sahne kurulumu yapın (detaylar aşağıda)

> **📖 Detaylı kurulum için:** [KURULUM_REHBERI.md](KURULUM_REHBERI.md) — Sıfırdan her adımı ekran ekran anlatır.

## ⚔️ Oyun Sistemleri

### Savaş
- **Kayan Joystick**: Ekranda herhangi bir yere dokunarak joystick oluşur
- **Otomatik Ateş**: Oyuncu sürekli ateş eder, nişan almaz
- **Tehdit Öncelikli Hedefleme**: Boss → Uzak mesafe düşmanlar → En yakın düşman
- **Mermiler**: Düşmana çarpana, engele çarpana veya menzil bitene kadar gider

### Düşmanlar
| Tip | Menzil | Ne Zaman Çıkar |
|-----|--------|-----------------|
| 🗡️ Bıçaklı | 1.5 birim | 0-35 saniye |
| ⚔️ Kılıçlı | 2.5 birim | 35-65 saniye |
| 🏹 Uzak Mesafe | 5-7 birim | 65+ saniye |
| 👹 Boss | Değişken | Her 5. seviye |

### Seviye Akışı (~120 saniye)
Kolay → Orta → Kısa mola → Kaos → Son hamle

### XP ve Güçlenme
- Düşman öldürdükçe XP kazanırsın (~her seviye 3 güçlenme)
- 3 kart çıkar: 1 stat + 1 silah özelliği + 1 rastgele
- Kart renkleri: 🔵 Mavi (+3), 🟣 Mor (+5), 🔴 Kırmızı (silah özellikleri)
- Tekrar atma: Seviye başına 1 hak (1500 coin veya reklam)

### Silah Özellikleri
| Özellik | Açıklama |
|---------|----------|
| 🔫 Pierce | Mermi düşmanların içinden geçer |
| 🔫 Multishot | Aynı anda birden fazla mermi atar |
| 🔫 Ricochet | Mermi düşmanlar arasında sekmeli |
| 💥 Explosive Rounds | Çarpışmada alan hasarı verir |
| 🧊 Slow On Hit | Vurulan düşman yavaşlar |

### Ekonomi
- 💰 **Coinler**: Düşman öldürerek kazanılır (tekrar oynanan arenalarda %25 ödeme)
- 📦 **Boss Sandığı**: Her bosstan düşer (%60 coin, %25 kostüm parçası, %15 meta token)
- 📅 **Günlük Ödül**: 15 günlük döngü (100→1500 coin), "2x Reklam" seçeneği
- ⬆️ **Meta Yükseltme**: Kalıcı stat artışları (maks 5 seviye)

### Canlanma
- Seviye başına en fazla 3 canlanma
- Her canlanma %15 coin cezası
- 3 saniyelik dokunulmazlık kalkanı
- XP ve boss canı değişmez

### İlerleme
- 20 arena × 35 seviye = 700 toplam seviye
- Arena bitirmeden sonrakine geçemezsiniz
- Eski arenalar tekrar oynanabilir (ödüller azaltılmış)

## 🏗️ Teknik Mimari

- **Olay Tabanlı**: Sistemler `EventBus` ile iletişim kurar (gevşek bağlantı)
- **Singleton Kalıbı**: Yöneticiler `Singleton<T>` ile `DontDestroyOnLoad` kullanır
- **Nesne Havuzu**: `ProjectilePool` mermileri yeniden kullanır (performans)
- **ScriptableObject**: `ArenaData` ve `SkinData` veri odaklı içerik tanımlar
- **Reklam Stub'ları**: `AdStubManager` simüle reklam callback'leri sağlar
- **PlayerPrefs**: Coinler, ilerleme, kostümler ve günlük ödül yerel olarak kaydedilir

## 🛠️ Teknoloji

| Bileşen | Değer |
|---------|-------|
| Motor | Unity 6.3 LTS (6000.3.9f1) |
| Render | Universal Render Pipeline (URP) |
| Platform | Mobil (Dikey yönlendirme) |
| Dil | C# (.NET Standard) |
| Navigasyon | Unity NavMesh (düşman AI) |
| Arayüz | Unity UI (uGUI) + TextMeshPro |