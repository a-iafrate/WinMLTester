using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace WinMLTester.Models
{
    public class ResultModel
    {
        public String Name { get; set; }
        public SoftwareBitmapSource Image { get; set; }

        public double Percent { get; set; }

        public string Info
        {
            get { return Name+" "+Percent.ToString("#0.00") + "%"; }
        }
    }
}
