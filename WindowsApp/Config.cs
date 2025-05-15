using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsApp
{
    class Config
    {
        public string title { get; set; }
        public string uri { get; set; }
        public string icon { get; set; }
        public string welcomeImg { get; set; }
        public string position { get; set; }
        public Theme theme { get; set; }
    }

    class Theme
    {
        public string bgColor { get; set; }
        public string foreColor { get; set; }

        public Theme(string foreColor, string bgColor)
        {
            this.foreColor = foreColor;
            this.bgColor = bgColor;
        }
    }
}
