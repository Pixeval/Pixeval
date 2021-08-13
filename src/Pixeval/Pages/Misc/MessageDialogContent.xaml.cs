namespace Pixeval.Pages.Misc
{

    public sealed partial class MessageDialogContent
    {
        public string ContentText { get; }

        public MessageDialogContent(string content)
        {
            ContentText = content;
            InitializeComponent();
        }
    }
}
