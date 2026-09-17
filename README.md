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

- **Quick Selection:** Press the customizable hotkey (default: Ctrl + Shift + Z) and draw a rectangle on your screen to mask an area.
- **System Tray Integration:** The app runs silently in the background and is accessible via the system tray.
- **Customizable Settings:** Double-click the tray icon or right-click and select "Settings" to:
  - Change the masking hotkey.
  - Customize the color of the selection border (visible only to you).
- **Stream Security:** The masked region appears as a solid black box in capture tools, preventing leaks. The border itself is completely invisible to the stream.

### Why StreamMask? (How is it different?)

Unlike standard screen-blocking tools that just place a physical black window on your screen (which blocks **your** vision too), StreamMask uses the native Windows WDA_MONITOR API. This means the mask is completely **transparent to you**, allowing you to play games or read text normally, but appears as a **solid black box to OBS** and your viewers. Furthermore, you don't need a clunky separate control panel to place your masks—just press the hotkey and draw directly on your screen like the Snipping Tool!

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

- **Hızlı Seçim:** Belirlediğiniz kısayol tuşuna (varsayılan: Ctrl + Shift + Z) basarak ekranda maskelemek istediğiniz alanı seçebilirsiniz.
- **Sistem Tepsisi Desteği:** Program arka planda, sistem tepsisinde (System Tray) çalışır.
- **Özelleştirilebilir Ayarlar:** Sistem tepsisindeki ikona çift tıklayarak veya sağ tıklayıp "Ayarlar" menüsünden:
  - Maskeleme kısayol tuşunu değiştirebilirsiniz.
  - Sizin ekranınızda görünen seçim çerçevesinin rengini dilediğiniz gibi ayarlayabilirsiniz.
- **Yayın Güvenliği:** Maskelenen bölge, kayıt araçlarında tamamen siyah bir kutu olarak belirir ve içeriğin sızmasını engeller. Çerçevenin kendisi ise yayına gitmez.

### Neden StreamMask? (Farkımız Ne?)

Ekrana sadece fiziksel siyah bir pencere koyan (ve dolayısıyla **sizin kendi görüşünüzü de** kapatan) sıradan gizleme araçlarının aksine, StreamMask doğrudan Windows WDA_MONITOR API'sini kullanır. Bu sayede sansürlenen alan **size tamamen şeffaf görünür** ve altındaki oyunu oynayıp yazıları okuyabilirsiniz; ancak **OBS, Discord ve yayındaki izleyicileriniz sadece siyah bir kutu görür**. Ayrıca maskeyi yerleştirmek için ayrı bir harita/kontrol paneli ile uğraşmazsınız; kısayola basın ve tıpkı Ekran Alıntısı Aracı gibi doğrudan ekranınızın üzerinde saniyeler içinde çizin!

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







