using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using CameoRescue.Core;

namespace CameoRescue.Tabs
{
    public sealed class UsbTab : UserControl
    {
        GroupBox _gDev, _gAct, _gRep;
        Label _lAdmin, _lHint, _lDevices;
        Button _btnDiag, _btnScan, _btnRestart, _btnResetHub, _btnDevMgr, _btnAdmin;
        TextBox _txtRep;

        public UsbTab()
        {
            AutoScroll = true;
            BuildUi();
            ApplyLang();
        }

        void BuildUi()
        {
            int W = 836;

            _gDev = new GroupBox { Left = 12, Top = 12, Width = W, Height = 96, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            Controls.Add(_gDev);

            _lAdmin = new Label { Left = 14, Top = 24, Width = 500, Height = 20 };
            _lDevices = new Label { Left = 14, Top = 48, Width = W - 44, Height = 38, AutoEllipsis = true, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            _gDev.Controls.AddRange(new Control[] { _lAdmin, _lDevices });

            _gAct = new GroupBox { Left = 12, Top = 116, Width = W, Height = 132, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            Controls.Add(_gAct);
            _btnDiag = MkBtn(14, 26, 150, "usb.btn.diag");
            _btnDiag.Click += (s, e) => Diagnose();
            _btnScan = MkBtn(172, 26, 150, "usb.btn.scan");
            _btnScan.Click += (s, e) => ScanUsb();
            _btnRestart = MkBtn(330, 26, 150, "usb.btn.restart");
            _btnRestart.Click += (s, e) => RestartDevice();
            _btnResetHub = MkBtn(488, 26, 150, "usb.btn.resethub");
            _btnResetHub.Click += (s, e) => ResetHub();
            _btnDevMgr = MkBtn(14, 64, 150, "usb.btn.devmgr");
            _btnDevMgr.Click += (s, e) => OpenDevMgr();
            _btnAdmin = MkBtn(172, 64, 180, "usb.btn.admin");
            _btnAdmin.Click += (s, e) => RelaunchAdmin();
            _gAct.Controls.AddRange(new Control[] { _btnDiag, _btnScan, _btnRestart, _btnResetHub, _btnDevMgr, _btnAdmin });

            _lHint = new Label { Left = 14, Top = 100, Width = W - 44, Height = 28, ForeColor = Color.FromArgb(90, 90, 90), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };

            _gRep = new GroupBox { Left = 12, Top = 256, Width = W, Height = 356, Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right };
            Controls.Add(_gRep);
            _gRep.Controls.Add(_lHint);
            _txtRep = new TextBox
            {
                Left = 14,
                Top = 24,
                Width = W - 44,
                Height = 318,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                WordWrap = false,
                Font = new Font("Consolas", 8.5f),
                BackColor = Color.FromArgb(250, 250, 250),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            };
            _gRep.Controls.Add(_txtRep);

            UpdateAdminLabel();
        }

        Button MkBtn(int x, int y, int w, string key)
        {
            var b = new Button { Left = x, Top = y, Width = w, Height = 30, Tag = key };
            return b;
        }

        void UpdateAdminLabel()
        {
            _lAdmin.Text = Proc.IsAdmin() ? Lang.T("usb.lbl.admin.yes") : Lang.T("usb.lbl.admin.no");
            _lAdmin.ForeColor = Proc.IsAdmin() ? Color.FromArgb(0, 128, 0) : Color.Firebrick;
        }

        void Append(string s)
        {
            _txtRep.AppendText(s + Environment.NewLine);
            Log.Write(s);
        }

        public void RefreshDevices()
        {
            try
            {
                var list = UsbDevice.Enumerate();
                if (list.Count == 0) _lDevices.Text = Lang.T("status.nodevice");
                else
                {
                    var sb = new System.Text.StringBuilder();
                    foreach (var d in list)
                        sb.AppendLine(d.Model + "  |  serial " + d.Serial + "  |  " + Lang.T(d.ModeKey));
                    _lDevices.Text = sb.ToString().TrimEnd();
                }
            }
            catch { _lDevices.Text = "-"; }
        }

        void Diagnose()
        {
            _txtRep.Clear();
            Append("== Diagnosa " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ==");
            var (_, o) = Proc.PowerShell(
                "$ErrorActionPreference='SilentlyContinue';" +
                "'## Perangkat VID_0B4D';" +
                "Get-PnpDevice | Where-Object { $_.InstanceId -match 'VID_0B4D' } | Select-Object Status,Present,InstanceId,Problem | Format-Table -AutoSize | Out-String;" +
                "'## Perangkat bermasalah (present)';" +
                "Get-PnpDevice -PresentOnly | Where-Object { $_.Status -ne 'OK' } | Select-Object Status,Class,FriendlyName,InstanceId,Problem | Format-Table -AutoSize | Out-String");
            Append(o);
            RefreshDevices();
        }

        void ScanUsb()
        {
            var (code, o) = Proc.PowerShell("pnputil /scan-devices");
            Append("Scan ulang: " + Lang.T("usb.scan.done") + " (exit " + code + ")");
            if (!string.IsNullOrWhiteSpace(o)) Append(o);
            RefreshDevices();
        }

        void RestartDevice()
        {
            string script =
                "$id = (Get-PnpDevice -PresentOnly | Where-Object { $_.InstanceId -match 'VID_0B4D' } | Select-Object -First 1).InstanceId;" +
                "if ($id) { pnputil /restart-device \"$id\"; Write-Output ('__OK__' + $id) } else { Write-Output '__NONE__' }";
            var (code, o) = Proc.PowerShell(script);
            if (o.Contains("__NONE__")) { Append(Lang.T("usb.restart.none")); MessageBox.Show(this, Lang.T("usb.restart.none"), Lang.T("msg.warn")); return; }
            string id = "";
            foreach (var line in o.Split('\n'))
                if (line.Trim().StartsWith("__OK__")) id = line.Trim().Substring(6);
            Append(Lang.T("usb.restart.done", id));
            MessageBox.Show(this, Lang.T("usb.restart.done", id), Lang.T("msg.info"));
            RefreshDevices();
        }

        void ResetHub()
        {
            var (_, o) = Proc.PowerShell(
                "Get-PnpDevice -PresentOnly -Class USB | Where-Object { $_.FriendlyName -match 'Hub' } | Select-Object -ExpandProperty InstanceId");
            var lines = o.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0) { Append(Lang.T("usb.resethub.none")); MessageBox.Show(this, Lang.T("usb.resethub.none"), Lang.T("msg.warn")); return; }

            string pick = PickDialog(Lang.T("usb.btn.resethub"), lines);
            if (pick == null) return;

            string script =
                "try { Get-PnpDevice -InstanceId '" + pick.Replace("'", "''") + "' | Disable-PnpDevice -Confirm:$false; Start-Sleep 2; " +
                "Get-PnpDevice -InstanceId '" + pick.Replace("'", "''") + "' | Enable-PnpDevice -Confirm:$false; Write-Output 'DONE' } " +
                "catch { Write-Output ('ERR ' + $_.Exception.Message) }";
            var (started, code, err) = Proc.PowerShellElevated(script);
            if (!started) { Append(Lang.T("usb.admin.declined")); return; }
            Append(Lang.T("usb.resethub.done", pick));
            RefreshDevices();
        }

        void OpenDevMgr()
        {
            try { Process.Start(new ProcessStartInfo("devmgmt.msc") { UseShellExecute = true }); } catch (Exception ex) { Append(ex.Message); }
        }

        void RelaunchAdmin()
        {
            try
            {
                string exe = Environment.ProcessPath ?? Application.ExecutablePath;
                Process.Start(new ProcessStartInfo(exe) { UseShellExecute = true, Verb = "runas" });
                Append(Lang.T("usb.admin.launched"));
                Application.Exit();
            }
            catch { Append(Lang.T("usb.admin.declined")); }
        }

        string PickDialog(string title, string[] items)
        {
            using var f = new Form { Text = title, Width = 620, Height = 300, StartPosition = FormStartPosition.CenterParent, MinimizeBox = false, MaximizeBox = false, FormBorderStyle = FormBorderStyle.FixedDialog };
            var lb = new ListBox { Left = 12, Top = 12, Width = 580, Height = 200, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            lb.Items.AddRange(items);
            if (lb.Items.Count > 0) lb.SelectedIndex = 0;
            var ok = new Button { Text = "OK", Left = 432, Top = 222, Width = 80, Height = 28, DialogResult = DialogResult.OK };
            var cancel = new Button { Text = "Cancel", Left = 518, Top = 222, Width = 80, Height = 28, DialogResult = DialogResult.Cancel };
            f.Controls.AddRange(new Control[] { lb, ok, cancel });
            f.AcceptButton = ok; f.CancelButton = cancel;
            return f.ShowDialog(this) == DialogResult.OK ? lb.SelectedItem as string : null;
        }

        public void ApplyLang()
        {
            _gDev.Text = Lang.T("usb.grp.devices");
            _gAct.Text = Lang.T("usb.grp.actions");
            _gRep.Text = Lang.T("usb.grp.report");
            foreach (Control c in _gAct.Controls) if (c is Button b && b.Tag is string k) b.Text = Lang.T(k);
            _lHint.Text = Lang.T("usb.hint").Replace("\n", "  ");
            UpdateAdminLabel();
            RefreshDevices();
        }
    }
}
