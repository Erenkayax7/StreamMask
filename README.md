<div align="center">
  <a href="#english"><img src="https://flagcdn.com/w40/us.png" width="36" height="24" alt="English"></a>
  &nbsp;&nbsp;&nbsp;
  <a href="#türkçe"><img src="https://flagcdn.com/w40/tr.png" width="36" height="24" alt="Türkçe"></a>
</div>

# StreamMask

<a id="english"></a>
## 🇺🇸 English

StreamMask is a lightweight Windows desktop application that allows you to select a specific area of your screen and hide it from streaming and screen recording software (like OBS, Discord, etc.).
Using the Windows WDA_MONITOR feature, the masked area appears as a solid black box on your stream, while you can still see and interact with the content completely transparently.

### Features

- **Quick Selection:** Press the single or combination hotkey you defined (default: Ctrl + Shift + Z) and draw a rectangle on your screen to mask an area instantly.
- **Modern User Interface:** Flat and modern UI with both **Dark** and **Light** themes. Includes a *Live Preview* box that shows exactly how your mask will look.
- **Customizable Settings:** Access the modern settings panel via the system tray to:
  - Assign any key combination by simply pressing the keys on your keyboard.
  - Customize the border color using the color palette.
  - Adjust the **border thickness** (1px to 10px) with a slider.
- **Aggressive Memory Optimization (Anti-Leak):** Automatically trims its working set RAM usage down to ~1-2 MB while idle in the background. Uses strict Garbage Collection and unmanaged resource disposal to prevent memory leaks over time.
- **Stream Security:** The masked region appears as a solid black box in capture tools like OBS and Discord, preventing leaks. The mask border is completely invisible to the stream.



### VirusTotal Result

[![VirusTotal](https://img.shields.io/badge/VirusTotal-Scan_Result-blue)](https://www.virustotal.com/gui/file/2239991d1685bd806582455f0522b22992e9c9a286ba070047634baea492fbb1)

**Note on False Positives:** Since this application uses low-level Windows APIs to hide windows from screen capture (SetWindowDisplayAffinity) and registers global keyboard shortcuts (RegisterHotKey), some heuristic antivirus engines may flag it as suspicious. This is a common **false positive** for open-source tools manipulating screen captures without an expensive digital certificate. The source code is entirely open for you to review and compile yourself!

### Build and Installation

You can easily compile the project from source using the built-in .NET Framework:

1. Open the Developer Command Prompt or add the directory containing csc.exe (e.g., C:\Windows\Microsoft.NET\Framework\v4.0.30319) to your system PATH.
2. Navigate to the project directory and run the following command:
   `cmd
   csc.exe /target:winexe /out:StreamMask.exe Program.cs
   `
3. Run the generated StreamMask.exe.

### Usage

1. Upon launching the program, an icon will appear in the System Tray (bottom right corner).
2. Press your hotkey (default: Ctrl + Shift + Z) to activate masking mode.
3. Click and drag your left mouse button to select the area you want to mask.
4. Once selected, that specific area will become a black box on your stream, while you will only see a thin border around it.
5. Press the hotkey again to remove the mask.
6. To change the hotkey or border color, right-click the system tray icon and open **Settings**.

---

<a id="türkçe"></a>
## 🇹🇷 Türkçe

StreamMask, ekranınızın belirli bir alanını seçerek o bölgeyi yayın ve ekran kayıt programlarından (OBS, Discord vb.) gizlemenizi sağlayan bir masaüstü uygulamasıdır. 
Bu program, Windows'un WDA_MONITOR özelliğini kullanarak maskelenen alanın yayınlanan ekranda siyah görünmesini sağlarken, sizin o alanı (neredeyse tamamen saydam olarak) görmeye devam etmenize olanak tanır.

### Özellikler

- **Hızlı Seçim:** Tekli veya çoklu kombinasyonla atadığınız kısayola basıp (varsayılan: Ctrl + Shift + Z) ekranınızda bir kutu çizerek saniyeler içinde o bölgeyi maskeleyebilirsiniz.
- **Modern Arayüz:** Windows 10 tarzı düz (flat), **Koyu (Siyah)** ve **Açık (Beyaz)** tema destekli yeni nesil arayüz. Maskenizin nasıl görüneceğini anlık gösteren *Canlı Önizleme* paneli içerir.
- **Gelişmiş Ayarlar:** Sistem tepsisi ikonuna çift tıklayarak ulaşabileceğiniz menüden:
  - Kısayol butonuna basıp klavyeden dilediğiniz tuşları tuşlayarak anında atama yapabilirsiniz.
  - Sadece sizin gördüğünüz çerçevenin rengini renk paletinden seçebilirsiniz.
  - Çerçevenin kalınlığını (1px - 10px arası) kaydırmalı çubukla (slider) ayarlayabilirsiniz.
- **Agresif Bellek Optimizasyonu (Anti-Leak):** Arka planda beklerken Windows'un SetProcessWorkingSetSize API'sini kullanarak RAM kullanımını **1-2 MB** seviyelerine indirir. Gelişmiş çöp toplayıcı (Garbage Collector) komutlarıyla yıllarca açık kalsa bile Memory Leak (bellek sızıntısı) yaşatmaz.
- **Yayın Güvenliği:** Maskelenen bölge, OBS ve Discord gibi kayıt araçlarında tamamen kapkaranlık (siyah) bir kutu olarak belirir ve içeriğin sızmasını kesin olarak engeller. Çerçevenin kendisi ise yayına gitmez.



### VirusTotal Sonucu

[![VirusTotal](https://img.shields.io/badge/VirusTotal-Tarama_Sonucu-blue)](https://www.virustotal.com/gui/file/2239991d1685bd806582455f0522b22992e9c9a286ba070047634baea492fbb1)

**False Positive Hakkında Not:** Bu uygulama, pencereleri ekran kaydından gizlemek için düşük seviyeli Windows API'leri (SetWindowDisplayAffinity) kullandığı ve arka planda global klavye kısayollarını (RegisterHotKey) dinlediği için bazı antivirüsler tarafından şüpheli olarak işaretlenebilir (False Positive). Dijital bir imza sertifikası olmayan ve ekran kartına müdahale eden açık kaynaklı programlarda bu durum çok normaldir. Kaynak kodları tamamen açıktır, dilediğiniz gibi inceleyip kendi bilgisayarınızda derleyebilirsiniz!

### Kurulum ve Derleme

Proje kaynak koddan .NET Framework ile kolayca derlenebilir:

1. Geliştirici Komut İstemini (Developer Command Prompt) açın veya csc.exe'nin bulunduğu dizini (örneğin C:\Windows\Microsoft.NET\Framework\v4.0.30319) sistem yolunuza (PATH) ekleyin.
2. Proje dizinine giderek aşağıdaki komutu çalıştırın:
   `cmd
   csc.exe /target:winexe /out:StreamMask.exe Program.cs
   `
3. Oluşan StreamMask.exe dosyasını çalıştırın.

### Kullanım

1. Programı başlattığınızda sağ alt köşedeki Sistem Tepsisinde (System Tray) bir ikon belirecektir.
2. Kısayolunuzu kullanarak (varsayılan: Ctrl + Shift + Z) veya ayarlardan belirlediğiniz kısayola basarak maskeleme modunu aktif edin.
3. Farenizin sol tuşuna basılı tutarak maskelemek istediğiniz alanı çizerek seçin.
4. Alan seçildikten sonra ilgili bölge yayına siyah gidecek, siz ise ince bir çerçeve göreceksiniz.
5. Maskeyi kaldırmak için kısayol tuşuna tekrar basın.
6. Kısayol veya renk ayarını değiştirmek için sistem tepsisindeki ikona sağ tıklayıp **Ayarlar**'a girebilirsiniz.












