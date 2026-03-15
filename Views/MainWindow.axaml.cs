using SukiUI.Controls;
using SukiUI.Dialogs;

namespace ReWindows.Views
{
    public partial class MainWindow : SukiWindow
    {
        public static ISukiDialogManager DialogManager = new SukiDialogManager();
        public MainWindow()
        {
            InitializeComponent();
        }
    }
}