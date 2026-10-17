using System;
using System.Drawing;
using System.Windows.Forms;
using CameoRescue.Core;
using CameoRescue.Tabs;

namespace CameoRescue
{
    public sealed class MainForm : Form
    {
        ComboBox _cmbLang;
        Label _lblLang;
        Label _lblDevice;
        TabControl _tabs;
        TabPage _pFlash, _pUsb, _pGuide;
        FlashTab _flash;
        UsbTab _usb;
        GuideTab _guide;
        System.Windows.Forms.Timer _timer;

        public MainForm()
        {
            Text = "CAMEO 5 Rescue Tool";
            ClientSize = new Size(884, 720);
            MinimumSize = new Size(780, 580);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);

            Log.Init();

            var top = new Panel { Dock = DockStyle.Top, Height = 76 };
            _lblLang = new Label { Left = 14, Top = 16, Width = 70, Height = 22, Text = "Bahasa:", TextAlign = ContentAlignment.MiddleLeft };
            _cmbLang = new ComboBox { Left = 92, Top = 13, Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbLang.Items.AddRange(new object[] { "Indonesia", "English" });
            _cmbLang.SelectedIndex = 0;
            _cmbLang.SelectedIndexChanged += (s, e) => Lang.Set(_cmbLang.SelectedIndex == 1 ? "en" : "id");
            _lblDevice = new Label { Left = 14, Top = 46, Width = 840, Height = 22, Text = "-", Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, ForeColor = Color.FromArgb(40, 40, 40) };
            top.Controls.AddRange(new Control[] { _lblLang, _cmbLang, _lblDevice });

            _tabs = new TabControl { Dock = DockStyle.Fill };
            _pFlash = new TabPage("Flashing");
            _pUsb = new TabPage("Perbaikan USB");
            _pGuide = new TabPage("Panduan");
            _flash = new FlashTab { Dock = DockStyle.Fill };
            _usb = new UsbTab { Dock = DockStyle.Fill };
            _guide = new GuideTab { Dock = DockStyle.Fill };
            _pFlash.Controls.Add(_flash);
            _pUsb.Controls.Add(_usb);
            _pGuide.Controls.Add(_guide);
            _tabs.TabPages.AddRange(new[] { _pFlash, _pUsb, _pGuide });
            _tabs.SelectedIndexChanged += (s, e) =>
            {
                if (_tabs.SelectedTab == _pUsb) _usb.RefreshDevices();
                if (_tabs.SelectedTab == _pFlash) _flash.RefreshDevice();
            };

            // Fill dulu, baru Top -> urutan docking deterministik
            Controls.Add(_tabs);
            Controls.Add(top);

            Lang.Changed += ApplyLang;
            ApplyLang();

            _timer = new System.Windows.Forms.Timer { Interval = 2500 };
            _timer.Tick += (s, e) => UpdateDeviceStatus();
            _timer.Start();
            UpdateDeviceStatus();
            _flash.RefreshDevice();
        }

        void UpdateDeviceStatus()
        {
            try
            {
                var d = UsbDevice.Find();
                _lblDevice.Text = d == null
                    ? Lang.T("status.nodevice")
                    : Lang.T("status.device", d.Model, d.Serial, Lang.T(d.ModeKey));
            }
            catch { _lblDevice.Text = Lang.T("status.nodevice"); }
        }

        void ApplyLang()
        {
            Text = Lang.T("app.title");
            _lblLang.Text = Lang.T("lang.label");
            _pFlash.Text = Lang.T("tab.flash");
            _pUsb.Text = Lang.T("tab.usb");
            _pGuide.Text = Lang.T("tab.guide");
            _flash.ApplyLang();
            _usb.ApplyLang();
            _guide.ApplyLang();
            UpdateDeviceStatus();
        }
    }
}
