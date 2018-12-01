using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Celin
{
    /// <summary>
    /// Interaction logic for ConfirmDlg.xaml
    /// </summary>
    public partial class ConfirmDlg : UserControl
    {
        public string Title { get; }
        public string Message { get; }
        public ConfirmDlg(string message, string title = "Message")
        {
            InitializeComponent();
            Title = title;
            Message = message;
            DataContext = this;
        }
    }
}
