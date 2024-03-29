﻿# DpiAware メモ

WinFormsで per monitor DPIを使うには以下の条件がある。

- .NET Framework4.6（System Monitor方式）
- .NET Framework4.7以降（Per Monitor V2方式）
- Windows10 1703以降

もともと自動スケーリングの機能があったがVistaでDPI仮想化が導入されたことで
表示がにじむようになってきた。  

https://nishy-software.com/ja/dev-sw/windows-high-dpi-desktop-app-4/


## DpiAware ( per monitor v2 ) を有効にする方法

app.configに以下を追記

```
  <System.Windows.Forms.ApplicationConfigurationSection>
    <add key="DpiAwareness" value="PerMonitorV2" />
  </System.Windows.Forms.ApplicationConfigurationSection>
  ```

さらに app.manifest へ記載

```
    <application>
      <!-- Windows 10 -->
      <supportedOS Id="{8e0f7a12-bfb3-4fe8-b9a5-48fd50a15a9a}" />
    </application>
```

フォーム起動時にEnableVisualStyles()の記述があることを確認

```csharp
static void Main()
{
    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);
    Application.Run(new Form2());
}
```

## イベント通知

Win10 1703以降なら以下のイベントが飛ぶようになる。

- DpiChanged
- DpiChangedAfterParent
- DpiChangedBeforeParent

## フォームのプロパティ

Form.AutoScaleMode　がある。

| mode   | 説明 |
|:-------|:-----|
|Dpi	 | ディスプレイの解像度に応じてスケールを制御します。 一般的な解像度は 96 dpi と 120 dpi です。|
|Font    | クラスが使用するフォント (通常はシステム フォント) のサイズに応じてスケールを制御します。|
|Inherit | クラスの親のスケーリング モードに従ってスケールを制御します。 親が存在しない場合、自動スケーリングは無効になっています。|
|None    | 自動スケーリングが無効|


デフォルトはFont。  
https://docs.microsoft.com/ja-jp/dotnet/api/system.windows.forms.autoscalemode?view=windowsdesktop-6.0

詳しい動きはここに記載がある。

Windows フォームでの自動スケーリング
https://docs.microsoft.com/ja-jp/dotnet/desktop/winforms/automatic-scaling-in-windows-forms?view=netframeworkdesktop-4.8

## その他

フォントはビットマップフォント以外を使ったほうがいい
https://qiita.com/mono1729/items/a93505a5cb3fe194b7dc

DPIを取得にするにはGraphicsから得る。

```csharp
static readonly float DpiScale = ((new System.Windows.Forms.Form()).CreateGraphics().DpiX) / 96;
```
