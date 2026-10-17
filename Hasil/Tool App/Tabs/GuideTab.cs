using System;
using System.Drawing;
using System.Windows.Forms;

namespace CameoRescue.Tabs
{
    public sealed class GuideTab : UserControl
    {
        RichTextBox _txt;

        public GuideTab()
        {
            _txt = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 10f),
                WordWrap = true,
                ScrollBars = RichTextBoxScrollBars.Vertical,
            };
            Controls.Add(_txt);
            ApplyLang();
        }

        public void ApplyLang()
        {
            _txt.Text = Lang.T("guide.text");
        }
    }
}
