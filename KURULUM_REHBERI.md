# 🎮 DesertArena — Adım Adım Kurulum Rehberi (Yeni Başlayanlar İçin)

Merhaba! Bu rehber, DesertArena oyununu bilgisayarınızda çalıştırmanız için gereken **her adımı** sıfırdan anlatmaktadır. Yazılım bilmenize gerek yok, sadece adımları takip edin.

---

## 📋 İçindekiler

1. [Ne Yaptık? Bu Proje Ne?](#1--ne-yaptık-bu-proje-ne)
2. [Bilgisayarınıza Ne Kurmanız Gerekiyor?](#2--bilgisayarınıza-ne-kurmanız-gerekiyor)
3. [Unity Kurulumu (Adım Adım)](#3--unity-kurulumu-adım-adım)
4. [Projeyi GitHub'dan İndirme](#4--projeyi-githubdan-indirme)
5. [Unity'de Yeni Proje Oluşturma](#5--unityde-yeni-proje-oluşturma)
6. [Script'leri Projeye Kopyalama](#6--scriptleri-projeye-kopyalama)
7. [Oyun Sahnesini Kurma](#7--oyun-sahnesini-kurma)
8. [Oyunu Test Etme](#8--oyunu-test-etme)
9. [Sık Sorulan Sorular](#9--sık-sorulan-sorular)

---

## 1. 🤔 Ne Yaptık? Bu Proje Ne?

**DesertArena** bir mobil hayatta kalma oyunudur. Şu özelliklere sahiptir:

- 📱 **Dikey mobil oyun** (telefon dikey tutularak oynanır)
- 🏜️ **Çöl temalı arena** — düşük poligon (low-poly) 3D grafik
- 🔫 **Otomatik ateş** — karakter düşmanlara kendisi nişan alır
- 🕹️ **Joystick ile hareket** — ekrana dokunarak karakter yönetilir
- 👾 **Dalga bazlı düşmanlar** — her seviye giderek zorlaşır
- 🏆 **20 arena × 35 seviye = 700 seviye** toplam içerik
- 💰 **Coin sistemi, günlük ödüller, boss sandıkları**

**Bu repo'da ne var?**
Bu GitHub deposunda oyunun **tüm C# kodları** (script'leri) hazır. Bunlar oyunun beyin kısmıdır. Ancak oyunu görmek ve oynamak için bunları **Unity** programına aktarmanız gerekiyor.

---

## 2. 💻 Bilgisayarınıza Ne Kurmanız Gerekiyor?

| Program | Ne İçin Gerekli | İndirme Linki |
|---------|----------------|---------------|
| **Unity Hub** | Unity'yi yönetmek için | [unity.com/download](https://unity.com/download) |
| **Unity 6.3 LTS** | Oyun motoru (oyunu çalıştıran program) | Unity Hub içinden kurulur |
| **Visual Studio** veya **VS Code** | Kod düzenlemek için (opsiyonel) | Unity ile birlikte gelir |
| **Git** (opsiyonel) | Projeyi indirmek için | [git-scm.com](https://git-scm.com) |

**Minimum bilgisayar gereksinimleri:**
- Windows 10/11 veya macOS 12+
- 8 GB RAM (16 GB önerilir)
- 10 GB boş disk alanı
- DirectX 11 destekli ekran kartı

---

## 3. 🔧 Unity Kurulumu (Adım Adım)

### Adım 3.1: Unity Hub İndirin
1. Tarayıcınızda **https://unity.com/download** adresine gidin
2. **"Download Unity Hub"** butonuna tıklayın
3. İndirilen dosyayı çalıştırın ve kurulum sihirbazını takip edin
4. Kurulum bittikten sonra Unity Hub'ı açın

### Adım 3.2: Unity Hesabı Oluşturun
1. Unity Hub açıldığında sizden **giriş yapmanızı** isteyecek
2. **"Create account"** (Hesap oluştur) butonuna tıklayın
3. E-posta, şifre ve adınızı girin
4. E-postanıza gelen doğrulama linkine tıklayın
5. Unity Hub'a geri dönüp giriş yapın
6. **"Personal"** (Kişisel) lisansı seçin — **ücretsizdir**

### Adım 3.3: Unity 6.3 LTS Sürümünü Kurun
1. Unity Hub'ın sol menüsünde **"Installs"** (Kurulumlar) sekmesine tıklayın
2. Sağ üstteki **"Install Editor"** butonuna tıklayın
3. Listeden **"Unity 6 (6000.3.x LTS)"** sürümünü bulun
4. **"Install"** butonuna tıklayın
5. Ek modüller ekranında şunları seçin:
   - ✅ **Android Build Support** (telefona yüklemek isterseniz)
   - ✅ **iOS Build Support** (iPhone için — sadece Mac'te)
   - ✅ **Visual Studio** (kod editörü)
6. **"Install"** butonuna tıklayın ve kurulumun bitmesini bekleyin (bu 20-40 dakika sürebilir)

---

## 4. 📥 Projeyi GitHub'dan İndirme

### Yöntem A: ZIP Olarak İndirme (En Kolay)
1. Bu GitHub sayfasına gidin: **https://github.com/EmirErcakar/DesertArenaWithCopilot**
2. Yeşil **"<> Code"** butonuna tıklayın
3. **"Download ZIP"** seçeneğine tıklayın
4. İndirilen ZIP dosyasını bilgisayarınızda bir yere çıkartın (örneğin: `C:\Projeler\DesertArena\`)
5. Çıkarttığınız klasörde `Assets/Scripts/` klasörünü görebilmelisiniz

### Yöntem B: Git ile İndirme (İsteğe Bağlı)
Eğer Git kuruluysa, komut satırında:
```
git clone https://github.com/EmirErcakar/DesertArenaWithCopilot.git
```

---

## 5. 🆕 Unity'de Yeni Proje Oluşturma

1. **Unity Hub**'ı açın
2. Sol menüde **"Projects"** (Projeler) sekmesine tıklayın
3. Sağ üstte **"New project"** (Yeni proje) butonuna tıklayın
4. Şu ayarları yapın:

| Ayar | Değer |
|------|-------|
| **Editor Version** | Unity 6 (6000.3.x) — sağ üstten seçin |
| **Template** | **Universal 3D** (URP şablonu) |
| **Project name** | `DesertArena` |
| **Location** | İstediğiniz bir klasör (örn: `C:\Projeler\`) |

5. **"Create project"** (Proje oluştur) butonuna tıklayın
6. Unity açılana kadar bekleyin (ilk açılış 3-5 dakika sürebilir)

---

## 6. 📁 Script'leri Projeye Kopyalama

### Adım 6.1: Script Klasörünü Bulun
1. GitHub'dan indirdiğiniz klasörü açın
2. İçinde `Assets/Scripts/` klasörünü bulun
3. Bu `Scripts` klasörünü **kopyalayın** (Ctrl+C)

### Adım 6.2: Unity Projesine Yapıştırın
1. Unity'de oluşturduğunuz projenin klasörünü bulun (örn: `C:\Projeler\DesertArena\`)
2. İçinde `Assets` klasörü olacak — bunu açın
3. **`Scripts` klasörünü buraya yapıştırın** (Ctrl+V)
4. Sonuç olarak şu yapı oluşmalı:
```
C:\Projeler\DesertArena\
└── Assets\
    └── Scripts\
        ├── Camera\
        ├── Core\
        ├── Economy\
        ├── Enemies\
        ├── Levels\
        ├── Player\
        ├── Progression\
        ├── Projectiles\
        ├── Revive\
        ├── Skins\
        ├── UI\
        ├── Utilities\
        └── XP\
```

### Adım 6.3: Unity'ye Geri Dönün
1. Unity'ye geri geçin
2. Unity otomatik olarak dosyaları algılayıp **derleme** (compile) yapacak
3. Alt kısımdaki **Console** penceresinde kırmızı hata yoksa — her şey doğru!
4. Eğer hata varsa, `Scripts` klasörünün doğru yerde olduğunu kontrol edin

---

## 7. 🎬 Oyun Sahnesini Kurma

Bu en detaylı bölüm. Unity'de sahneyi adım adım oluşturacağız.

### Adım 7.1: Arena Zeminini Oluşturun
1. Unity'de üst menüden: **GameObject → 3D Object → Plane** seçin
2. Sol taraftaki **Hierarchy** panelinde "Plane" nesnesini seçin
3. Sağ taraftaki **Inspector** panelinde:
   - **Position**: X=0, Y=0, Z=0
   - **Scale**: X=5, Y=1, Z=5 (büyük bir zemin için)
4. Zemine bir isim verin: "Plane" yazısına çift tıklayıp **"Arena"** yazın

### Adım 7.2: Görünmez Sınır Duvarları Ekleyin
1. **GameObject → Create Empty** ile boş bir obje oluşturun
2. İsmini **"Boundaries"** yapın
3. Bu objenin altına 4 adet duvar ekleyin:
   - **GameObject → 3D Object → Cube** ile 4 küp oluşturun
   - Her birini arenanın bir kenarına yerleştirin (duvar gibi)
   - **Inspector**'da **Mesh Renderer** bileşenini kaldırın (görünmez olması için)
   - Her birinin **Box Collider** bileşeni kalsın

### Adım 7.3: Oyuncu (Player) Oluşturun
1. **GameObject → 3D Object → Capsule** seçin (oyuncu karakteri)
2. İsmini **"Player"** yapın
3. **Inspector**'da **Position**: X=0, Y=1, Z=0
4. **Tag** alanını **"Player"** olarak ayarlayın (üst kısımda)
5. Şu bileşenleri ekleyin (**Add Component** butonuyla):
   - **Character Controller** (Unity'nin yerleşik bileşeni)
   - **PlayerController** (bizim script'imiz — arama kutusuna yazın)
   - **PlayerStats** (otomatik eklenecek — PlayerController ile birlikte gelir)
   - **PlayerTargeting** (otomatik eklenecek)
   - **WeaponController** (otomatik eklenecek)

### Adım 7.4: Kamerayı Ayarlayın
1. **Hierarchy**'de **"Main Camera"** nesnesini seçin
2. **Inspector**'da **CameraFollow** bileşenini ekleyin (Add Component → CameraFollow)
3. **CameraFollow** bileşeninde:
   - **Target** alanına **Player** objesini sürükleyin
   - **Offset**: X=0, Y=12, Z=-8 (üstten açılı bakış)

### Adım 7.5: UI Sistemi (Arayüz) Oluşturun
1. **GameObject → UI → Canvas** seçin
2. Canvas'ı seçin, **Inspector**'da:
   - **Canvas Scaler** → **UI Scale Mode**: "Scale With Screen Size"
   - **Reference Resolution**: X=1080, Y=1920 (dikey mobil)
   - **Match**: 0.5
3. Canvas altına şu boş objeleri oluşturun (sağ tık → Create Empty):
   - **"JoystickArea"** — FloatingJoystick bileşeni ekleyin
   - **"HUD"** — HUDManager bileşeni ekleyin
   - **"UpgradePanel"** — UpgradeCardUI bileşeni ekleyin
   - **"BossCountdown"** — BossCountdownUI bileşeni ekleyin
   - **"RevivePrompt"** — RevivePromptUI bileşeni ekleyin
   - **"BossChestPanel"** — BossChestUI bileşeni ekleyin
   - **"DailyStreakPanel"** — DailyStreakUI bileşeni ekleyin

### Adım 7.6: Yönetici (Manager) Objelerini Oluşturun
1. **GameObject → Create Empty** ile boş obje oluşturun
2. İsmini **"GameManager"** yapın
3. Şu bileşenleri ekleyin:
   - **GameManager** (Core klasöründen)
   - **LevelManager**
   - **EnemySpawner**
   - **XPSystem**
   - **UpgradeSystem**
   - **ReviveSystem**
   - **BossChestSystem**
4. Başka bir boş obje oluşturup **"EconomyManager"** adını verin:
   - **CoinManager** ekleyin
   - **DailyStreakSystem** ekleyin
   - **MetaUpgradeSystem** ekleyin
5. Başka bir boş obje: **"ProgressionManager"**:
   - **ProgressionManager** ekleyin
6. Başka bir boş obje: **"SkinManager"**:
   - **SkinManager** ekleyin

### Adım 7.7: Düşman Prefab'ları Oluşturun
1. **Assets** klasöründe sağ tık → **Create → Folder** → İsim: **"Prefabs"**
2. **GameObject → 3D Object → Capsule** ile bir kapsül oluşturun
3. İsmini **"MeleeKnifeEnemy"** yapın
4. Bileşen ekleyin: **MeleeEnemy**
5. **Hierarchy**'den bu objeyi **Assets/Prefabs** klasörüne sürükleyin (bu prefab yapar)
6. Sahndeki orijinali silin
7. Aynı işlemi tekrarlayın:
   - **"MeleeSwordEnemy"** — MeleeEnemy bileşeni (Inspector'da melee türünü "Sword" yapın)
   - **"RangedEnemy"** — RangedEnemy bileşeni
   - **"BossEnemy"** — BossEnemy bileşeni

### Adım 7.8: Mermi (Projectile) Prefab'ı Oluşturun
1. **GameObject → 3D Object → Sphere** ile bir küre oluşturun
2. **Scale**: X=0.2, Y=0.2, Z=0.2 (küçük mermi)
3. İsmini **"Projectile"** yapın
4. Bileşen ekleyin:
   - **Rigidbody** (Use Gravity: kapalı)
   - **Projectile** (bizim script'imiz)
5. Bu objeyi **Assets/Prefabs** klasörüne sürükleyin
6. Sahndeki orijinali silin

### Adım 7.9: NavMesh Ayarları (Düşman Hareketi İçin)
1. Üst menüden: **Window → AI → Navigation** seçin
2. **Arena** (zemin) objesini seçin
3. Navigation penceresinde **Bake** sekmesine geçin
4. **"Bake"** butonuna tıklayın — zemin mavi renk olacak
5. Bu, düşmanların zeminde yürüyebileceği anlamına gelir

### Adım 7.10: Referansları Bağlayın
1. **Player** objesini seçin:
   - **WeaponController** → **Projectile Prefab** alanına mermi prefab'ını sürükleyin
2. **GameManager** objesini seçin:
   - **EnemySpawner** → Düşman prefab'larını ilgili alanlara sürükleyin
3. **JoystickArea** objesini seçin:
   - **Player** → **PlayerController** → **Joystick** alanına JoystickArea'yı sürükleyin

---

## 8. ▶️ Oyunu Test Etme

1. Unity'nin üst ortasındaki **▶ Play** butonuna tıklayın
2. **Game** sekmesine geçin (oyun burada görünür)
3. Fare ile ekrana tıklayıp sürükleyerek joystick'i kullanın
4. Karakter hareket etmeli ve düşmanlara otomatik ateş açmalı
5. Durdurmak için tekrar **▶** butonuna tıklayın

### Sorun Giderme
| Sorun | Çözüm |
|-------|-------|
| Kırmızı hatalar Console'da | Script'lerin doğru klasörde olduğunu kontrol edin |
| Karakter hareket etmiyor | PlayerController'da Joystick referansının atandığını kontrol edin |
| Düşmanlar gelmiyor | EnemySpawner'da prefab'ların atandığını kontrol edin |
| Kamera takip etmiyor | CameraFollow'da Target'ın Player olduğunu kontrol edin |

---

## 9. ❓ Sık Sorulan Sorular

### "Bu kodlar ne işe yarıyor?"
Bu kodlar oyunun her parçasını kontrol eder:
- **PlayerController**: Karakterin hareket etmesini sağlar
- **EnemySpawner**: Düşmanların oluşmasını yönetir
- **XPSystem**: Deneyim puanı ve güçlenme sistemi
- **CoinManager**: Para biriktirme sistemi
- **GameManager**: Oyunun genel akışını (menü, oyun, duraklatma vb.) yönetir

### "Oyunu telefona nasıl yüklerim?"
1. Unity'de: **File → Build Settings**
2. **Android** veya **iOS** seçin
3. **Switch Platform** butonuna tıklayın
4. **Build** butonuna tıklayın
5. Oluşan APK (Android) dosyasını telefonunuza kopyalayıp kurun

### "Grafikler nasıl eklenir?"
Şu an oyun basit geometrik şekillerle çalışır. Daha güzel görseller için:
1. **Unity Asset Store**'dan ücretsiz low-poly paketleri indirin
2. **Window → Asset Store** ile erişebilirsiniz
3. İndirdiğiniz 3D modelleri prefab'lara yerleştirin

### "Sesleri nasıl eklerim?"
1. Ses dosyalarını (MP3/WAV) `Assets/Audio/` klasörüne koyun
2. Objelere **AudioSource** bileşeni ekleyin
3. Script'lerde `AudioSource.Play()` çağrısı ekleyin

### "Reklamları nasıl gerçek yaparım?"
Şu an reklam sistemi "sahte" (stub) olarak çalışır. Gerçek reklam için:
1. **Unity Ads** veya **Google AdMob** SDK'larından birini kurun
2. `AdStubManager.cs` dosyasındaki kodları gerçek SDK çağrılarıyla değiştirin

---

## 📁 Dosya Yapısı Açıklaması

| Klasör | İçeriği | Ne Yapar |
|--------|---------|----------|
| `Core/` | GameManager, EventBus | Oyunun genel durumunu yönetir |
| `Player/` | Karakter script'leri | Hareket, can, nişan alma, ateş etme |
| `Camera/` | Kamera takibi | Kameranın oyuncuyu takip etmesi |
| `Enemies/` | Düşman tipleri | Bıçaklı, kılıçlı, uzak mesafe ve boss düşmanlar |
| `Projectiles/` | Mermi sistemi | Mermilerin hareketi ve çarpışması |
| `Levels/` | Seviye yöneticisi | Seviye süresi, zorluk ayarı |
| `XP/` | Deneyim ve güçlenme | XP kazanma, kart seçimi, yetenek artışı |
| `UI/` | Arayüz elemanları | Joystick, sağlık barı, kart ekranı vb. |
| `Economy/` | Ekonomi sistemi | Coin, boss sandığı, günlük ödül, meta yükseltme |
| `Revive/` | Canlanma sistemi | 3 canlanma hakkı, reklam simülasyonu |
| `Skins/` | Kostüm sistemi | Karakter görünümleri ve bonusları |
| `Progression/` | İlerleme sistemi | 20 arena × 35 seviye takibi |
| `Utilities/` | Yardımcı araçlar | Singleton tasarım kalıbı |

---

## 🎯 Sonraki Adımlar

Temel kurulumu yaptıktan sonra şunları yapabilirsiniz:

1. ✅ Oyunu Play butonuyla test edin
2. 🎨 Asset Store'dan ücretsiz 3D modeller indirip görselliği geliştirin
3. 🔊 Ses efektleri ve müzik ekleyin
4. 📱 Android/iOS için build alın
5. ⚖️ Düşman sayılarını, hızlarını ve hasarlarını Inspector'dan ayarlayın
6. 🏪 Unity Ads ekleyerek gerçek reklam entegrasyonu yapın

---

**Herhangi bir sorununuz olursa GitHub Issues kısmından yeni bir issue açabilirsiniz! 🙌**
