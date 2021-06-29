using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;
using Pixeval.Objects;
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
        public static readonly DependencyProperty TitleContentProperty = DependencyProperty.Register(nameof(TitleContent), typeof(string), typeof(MessageDialog), new PropertyMetadata(Pixeval.Resources.Resources.Warning));

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

        public MessageDialogResult? Result { get; set; }

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var messageDialog = (MessageDialog) d;
            var value = (bool) e.NewValue;
            messageDialog.NoButton.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            messageDialog.NoButton.Width = value ? 125 : 0;
            Grid.SetColumnSpan(messageDialog.YesButton, value ? 1 : 2);
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static Task<MessageDialogResult> Warning(DialogHost attached, string content, bool acknowledge = false)
        {
            return Show(attached, content, Pixeval.Resources.Resources.Warning, acknowledge);
        }

        public static Task<MessageDialogResult> Show(DialogHost attached, string content, string titleContent, bool acknowledge = false)
        {
            var messageDialog = (MessageDialog) attached.DialogContent;
            attached.Visibility = Visibility.Visible;
            messageDialog!.Result = null;
            messageDialog.TitleContent = titleContent;
            messageDialog.MessageContent = content;
            messageDialog.Acknowledge = acknowledge;
            attached.IsOpen = true;
            return Task.Run(() =>
            {
                while (messageDialog.Result == null)
                {
                }
                attached.CloseControl();
                return messageDialog.Result.Value;
            });
        }

        private void YesButton_OnClick(object sender, RoutedEventArgs e)
        {
            Result = MessageDialogResult.Yes;
        }

        private void NoButton_OnClick(object sender, RoutedEventArgs e)
        {
            Result = MessageDialogResult.No;
        }
    }
}