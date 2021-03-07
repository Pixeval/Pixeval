using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;
using Pixeval.Objects;
using Pixeval.Objects.I18n;
using Pixeval.Objects.Primitive;
using PropertyChanged;

namespace Pixeval.UI.UserControls
{
    /// <summary>
    ///     Interaction logic for MessageDialog.xaml
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public partial class MessageDialog
    {
        public static readonly DependencyProperty TitleContentProperty = DependencyProperty.Register(nameof(TitleContent), typeof(string), typeof(MessageDialog), new PropertyMetadata(AkaI18N.Warning));

        public static readonly DependencyProperty MessageContentProperty = DependencyProperty.Register(nameof(MessageContent), typeof(string), typeof(MessageDialog), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty AcknowledgeProperty = DependencyProperty.Register(nameof(Acknowledge), typeof(bool), typeof(MessageDialog), new PropertyMetadata(true, PropertyChangedCallback));

        public MessageDialog()
        {
            InitializeComponent();
        }

        public string TitleContent
        {
            get => (string) GetValue(TitleContentProperty);
            set => SetValue(TitleContentProperty, value);
        }

        public string MessageContent
        {
            get => (string) GetValue(MessageContentProperty);
            set => SetValue(MessageContentProperty, value);
        }

        public bool Acknowledge
        {
            get => (bool) GetValue(AcknowledgeProperty);
            set => SetValue(AcknowledgeProperty, value);
        }

        public DialogResult? Result { get; set; }

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var messageDialog = (MessageDialog) d;
            var value = (bool) e.NewValue;
            messageDialog.NoButton.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            messageDialog.NoButton.Width = value ? 125 : 0;
            Grid.SetColumnSpan(messageDialog.YesButton, value ? 1 : 2);
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static Task<DialogResult> Warning(DialogHost attached, string content, bool acknowledge = false)
        {
            var taskCompletionSource = new TaskCompletionSource<DialogResult>(TaskCreationOptions.AttachedToParent);
            var messageDialog = (MessageDialog) attached.DialogContent;
            messageDialog.Result = null;
            messageDialog.TitleContent = AkaI18N.Warning;
            messageDialog.MessageContent = content;
            messageDialog.Acknowledge = acknowledge;
            attached.OpenControl();
            Task.Run(() =>
            {
                while (messageDialog.Result == null)
                {
                }
                attached.CloseControl();
                taskCompletionSource.SetResult(messageDialog.Result.Value);
            });
            return taskCompletionSource.Task;
        }

        private void YesButton_OnClick(object sender, RoutedEventArgs e)
        {
            Result = DialogResult.Yes;
        }

        private void NoButton_OnClick(object sender, RoutedEventArgs e)
        {
            Result = DialogResult.No;
        }
    }
}