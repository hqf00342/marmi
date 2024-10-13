/*
BindableBaseクラス

INotifyPropertyChanged 用の基底クラス
prop5スニペットでプロパティを実装する。

    private int _MyProperty;
    public int MyProperty { get => _MyProperty; set => SetProperty(ref _MyProperty, value); }

SetProperty()以外の場所で変更通知をしたい場合は OnPropertyChanged を呼び出す。
（あまりないはず）

2024年10月13日
WinFormsでは、1つのプロパティを変更通知すると残りのプロパティも
自動チェックしてくれる。
今回はコード側から変更通知するのはInit()でのみのため
全プロパティに実装せず、１つだけOnPropertyChangedを発行し
残りのプロパティもチェックしてもらう。
そのため、OnPropertyChangedをprotectedからpublicに変更した。
*/
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;


namespace Mii
{
    public abstract class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// プロパティに値を設定
        /// </summary>
        /// <typeparam name="T">型。通常は省略</typeparam>
        /// <param name="store">対象のバッキングフィールド</param>
        /// <param name="value">設定値</param>
        /// <param name="propertyName">プロパティ名。省略する</param>
        /// <returns>設定した場合はtrue。設定不要だった場合はfalse</returns>
        protected bool SetProperty<T>(ref T store, T value, [CallerMemberName] string propertyName = null)
        {
            if (object.Equals(store, value))
                return false;

            VerifyPropertyName(propertyName);

            store = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// プロパティ変更用の通知メソッド。
        /// 通常はSetProperty()の利用で事足りるが、
        /// どうしても手動でWPFに変更通知する必要がある場合に利用
        /// </summary>
        /// <param name="propertyName"></param>
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(sender: this, e: new PropertyChangedEventArgs(propertyName));
        }

        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                throw new ArgumentException($"Invalid property name:{propertyName}");
            }
        }
    }
}