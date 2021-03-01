# ToDoリスト

## やりたいこと


- Data構造とロジックをフォルダ分け
- Task/ValueTask化の推進
- 7zipライブラリの更新・併用
- Utyクラスの整理
- サムネイル画面で描写中にアプリ終了すると例外。indexがoutOfRange
- BinaryFormatter をやめる。.NET5にはない。
- ClearBox表示時にメモリー利用量が60MBぐらいずつ増加している
- GC実施タイミングを決める
- 

## 完了

- Sortバグを修正
- BmpからRawImage作るとき、jpegではなくpngにする
- サムネイル関係の最適化
- GC頻度を下げる

## 高DPIサポート

Windows10+.NET Framework4.8前提で行く。app.manifestを追加

### System Monitor方式

.NET Framework4.6.2以降でサポート。4.7以降であれば supportOS の設定だけ必要で dpiAware や dpiAwareness のタグは使用しません。
また `app.config`に以下の設定を入れる。

```
<configuration>
  <System.Windows.Forms.ApplicationConfigurationSection>
    <add key="DpiAwareness" value="System" />
  </System.Windows.Forms.ApplicationConfigurationSection>
<configuration>

  <application xmlns="urn:schemas-microsoft-com:asm.v3">
    <windowsSettings>
      <dpiAware xmlns="http://schemas.microsoft.com/SMI/2005/WindowsSettings">true</dpiAware>
    </windowsSettings>
  </application>
```

### Per Monitor 方式

.NET Framework 4.7では、Per Monitor V2方式に対応。app.manifest で supportOS をマニフェストで記述した後、
App.configに以下を記載。

```
<configuration>
  <System.Windows.Forms.ApplicationConfigurationSection>
    <add key="DpiAwareness" value="PerMonitor" />
  </System.Windows.Forms.ApplicationConfigurationSection>
<configuration>
```

app.manifestの以下の部分は有効にしてはダメ。PerMonitorにならない、DPI変更イベント通知も来ない。

```
  <!--
  <application xmlns="urn:schemas-microsoft-com:asm.v3">
    <windowsSettings>
      <dpiAware xmlns="http://schemas.microsoft.com/SMI/2005/WindowsSettings">true</dpiAware>
    </windowsSettings>
  </application>
  -->
```

### PerMonitor V2

```
<configuration>
  <System.Windows.Forms.ApplicationConfigurationSection>
    <add key="DpiAwareness" value="PerMonitorV2" />
  </System.Windows.Forms.ApplicationConfigurationSection>
<configuration>
```
また、
Program.csに `EnableVisualStyles()` の呼び出しがあることを確認します。


### コード側の追加対応

- Form1.AutoScaleModeをDpiにする。
- フォントをMeiryoあたりにする。MS UI Gothic、ＭＳ Ｐゴシックはビットマップフォントがある。

Pixel単位の操作を行っているところでは自分でDpi比率で乗算して補正する必要がある。
コントロールによる。

```
static readonly float DpiScale = ((new System.Windows.Forms.Form()).CreateGraphics().DpiX) / 96;
```

### イベント処理

.NET Framework 4.7 以降、Windows10 1703以降で、3つの新イベントが受信できる。Windows 10 1703以前の場合は自分でWM_DPICHANGEDを処理する。

| event                  | desc      |
|------------------------|-----------|
| DpiChangedAfterParent  | DPI 変更イベント発生後に、コントロールの DPI 設定がプログラムによって変更されたときに発生。|
| DpiChangedBeforeParent | DPI 変更イベント発生前に、コントロールの DPI 設定がプログラムによって変更されたときに発生。|
| DpiChanged             | フォームが現在表示されているディスプレイデバイスで DPI 設定が変更されたときに発生します。|


### 参考URL  

MSDN:Windows フォームでの高 DPI サポート  
https://docs.microsoft.com/ja-jp/dotnet/desktop/winforms/high-dpi-support-in-windows-forms?view=netframeworkdesktop-4.8  

デスクトップアプリの高DPI対応 #4 – WinFormsアプリ  
https://nishy-software.com/ja/dev-sw/windows-high-dpi-desktop-app-4/



