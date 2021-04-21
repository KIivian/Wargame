using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Shapes;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Wargame
{

    public partial class MapForm : Form
    {
        private PointF[] GetHexCorners(PointF center,double size)
        {
            var corners = new PointF[6];
            for (var i = 0; i < 6; i++)
            {
                var angle_deg = 60 * i + 30;
                var angle_rad = Math.PI / 180 * angle_deg;
                corners[i].X = (float)(center.X + size * Math.Cos(angle_rad));
                corners[i].Y = (float)(center.Y + size * Math.Sin(angle_rad));
        }
            return corners;
        }
        public MapForm()
        {
            var hexagon = new Polyg
            InitializeComponent();
        }

    }
}
