# DrmMasker

DrmMasker, ekranınızın belirli bir alanını seçerek o bölgeyi yayın ve ekran kayıt programlarından (OBS vb.) gizlemenizi sağlayan bir masaüstü uygulamasıdır. 
Bu program, Windows'un `WDA_MONITOR` özelliğini kullanarak maskelenen alanın yayınlanan ekranda siyah görünmesini sağlarken, sizin o alanı (neredeyse tamamen saydam olarak) görmeye devam etmenize olanak tanır.

Bu proje **Erenkayax7** tarafından geliştirilmiştir.

## Özellikler

- **Hızlı Seçim:** Belirlediğiniz kısayol tuşuna (varsayılan: `Ctrl + Shift + Z`) basarak ekranda maskelemek istediğiniz alanı seçebilirsiniz.
- **Sistem Tepsisi Desteği:** Program arka planda, sistem tepsisinde (System Tray) çalışır.
- **Özelleştirilebilir Ayarlar:** Sistem tepsisindeki ikona çift tıklayarak veya sağ tıklayıp "Ayarlar" menüsünden:
  - Maskeleme kısayol tuşunu değiştirebilirsiniz.
  - Sizin ekranınızda görünen seçim çerçevesinin rengini dilediğiniz gibi ayarlayabilirsiniz.
- **Yayın Güvenliği:** Maskelenen bölge, kayıt araçlarında tamamen siyah bir kutu olarak belirir ve içeriğin sızmasını engeller. Çerçevenin kendisi ise yayına gitmez.

## Kurulum ve Derleme

Proje kaynak koddan .NET Framework ile kolayca derlenebilir:

1. Geliştirici Komut İstemini (Developer Command Prompt) açın veya `csc.exe`'nin bulunduğu dizini (örneğin `C:\Windows\Microsoft.NET\Framework\v4.0.30319`) sistem yolunuza (PATH) ekleyin.
2. Proje dizinine giderek aşağıdaki komutu çalıştırın:
   ```cmd
   csc.exe /target:winexe /out:DrmMask.exe Program.cs
   ```
3. Oluşan `DrmMask.exe` dosyasını çalıştırın.

## Kullanım

1. Programı başlattığınızda sağ alt köşedeki Sistem Tepsisinde (System Tray) bir ikon belirecektir.
2. Kısayolunuzu kullanarak (varsayılan: `Ctrl + Shift + Z`) veya ayarlardan belirlediğiniz kısayola basarak maskeleme modunu aktif edin.
3. Farenizin sol tuşuna basılı tutarak maskelemek istediğiniz alanı çizerek seçin.
4. Alan seçildikten sonra ilgili bölge yayına siyah gidecek, siz ise ince bir çerçeve göreceksiniz.
5. Maskeyi kaldırmak için kısayol tuşuna tekrar basın.
6. Kısayol veya renk ayarını değiştirmek için sistem tepsisindeki ikona sağ tıklayıp **Ayarlar**'a girebilirsiniz.
