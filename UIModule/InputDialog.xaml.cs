using Windows.UI.Xaml.Controls;

namespace BangumiArchive.UIModule
{
    /// <summary>
    /// A content dialog for the input of the name of anime
    /// </summary>
    public sealed partial class InputDialog : ContentDialog
    {
        public string Text;

        public InputDialog()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Pass the text if receive yes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Text = Inbox.Text;
        }

        /// <summary>
        /// Pass nothing if receive no
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Text = "";
        }
    }
}
