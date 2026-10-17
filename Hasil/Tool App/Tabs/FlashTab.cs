using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CameoRescue.Core;

namespace CameoRescue.Tabs
{
    public sealed class FlashTab : UserControl
    {
        // Perangkat
        GroupBox _gDev;
        Label _lModelCap, _lSerialCap, _lModeCap, _lModel, _lSerial, _lMode;
        Button _btnRefresh;

        // Firmware
        GroupBox _gFw;
        ComboBox _cmbSource;
        Label _lFwNameCap, _lFwSizeCap, _lFwShaCap, _lFwCompatCap;
        Label _lFwName, _lFwSize, _lFwSha, _lFwCompat;

        // Proses
        GroupBox _gProc;
        CheckBox _chkDry;
        Button _btnStart, _btnVerify, _btnSaveLog, _btnCopyLog, _btnClearLog;
        ProgressBar _bar;
        Label _lStatus;
        TextBox _txtLog;

        FirmwareInfo _fw;
        CameoDevice _dev;
        bool _busy;

        public FlashTab()
        {
            AutoScroll = true;
            BuildUi();
            ApplyLang();
            LoadEmbeddedList();
        }

        void BuildUi()
        {
            int W = 836;

            // ---------- Perangkat ----------
            _gDev = new GroupBox { Left = 12, Top = 12, Width = W, Height = 108, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            Controls.Add(_gDev);

            _lModelCap = MkCap(14, 26, "lbl.model");
            _lSerialCap = MkCap(14, 50, "lbl.serial");
            _lModeCap = MkCap(14, 74, "lbl.mode");
            _lModel = MkVal(110, 26);
            _lSerial = MkVal(110, 50);
            _lMode = MkVal(110, 74);
            _gDev.Controls.AddRange(new Control[] { _lModelCap, _lSerialCap, _lModeCap, _lModel, _lSerial, _lMode });

            _btnRefresh = new Button { Left = W - 130, Top = 24, Width = 110, Height = 30, Text = "Refresh", Anchor = AnchorStyles.Top | AnchorStyles.Right };
            _btnRefresh.Click += (s, e) => { Log.Write("Refresh perangkat"); RefreshDevice(); };
            _gDev.Controls.Add(_btnRefresh);

            // ---------- Firmware ----------
            _gFw = new GroupBox { Left = 12, Top = 128, Width = W, Height = 176, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            Controls.Add(_gFw);

            _cmbSource = new ComboBox { Left = 110, Top = 22, Width = W - 150, DropDownStyle = ComboBoxStyle.DropDownList, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            _cmbSource.SelectedIndexChanged += CmbSource_Changed;
            _gFw.Controls.Add(_cmbSource);

            _lFwNameCap = MkCap(14, 56, "fw.name");
            _lFwSizeCap = MkCap(14, 80, "fw.size");
            _lFwShaCap = MkCap(14, 104, "fw.sha");
            _lFwCompatCap = MkCap(14, 128, "fw.compat");
            _lFwName = MkVal(110, 56);
            _lFwSize = MkVal(110, 80);
            _lFwSha = MkVal(110, 104);
            _lFwCompat = MkVal(110, 128);
            _lFwCompat.Font = new Font(Font, FontStyle.Bold);
            _gFw.Controls.AddRange(new Control[] { _lFwNameCap, _lFwSizeCap, _lFwShaCap, _lFwCompatCap, _lFwName, _lFwSize, _lFwSha, _lFwCompat });

            // ---------- Proses ----------
            _gProc = new GroupBox { Left = 12, Top = 312, Width = W, Height = 300, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            Controls.Add(_gProc);

            _chkDry = new CheckBox { Left = 14, Top = 24, Width = 500, Height = 22 };
            _gProc.Controls.Add(_chkDry);

            _btnStart = MkBtn(14, 52, 170, "btn.start");
            _btnStart.Click += (s, e) => StartRepair();
            _btnVerify = MkBtn(192, 52, 150, "btn.verify");
            _btnVerify.Click += (s, e) => VerifyResult();
            _btnSaveLog = MkBtn(350, 52, 120, "btn.saveLog");
            _btnSaveLog.Click += (s, e) => SaveLog();
            _btnCopyLog = MkBtn(478, 52, 110, "btn.copyLog");
            _btnCopyLog.Click += (s, e) => { try { Clipboard.SetText(Log.Text); } catch { } };
            _btnClearLog = MkBtn(596, 52, 130, "btn.clearLog");
            _btnClearLog.Click += (s, e) => { _txtLog.Clear(); };
            _gProc.Controls.AddRange(new Control[] { _btnStart, _btnVerify, _btnSaveLog, _btnCopyLog, _btnClearLog });

            _bar = new ProgressBar { Left = 14, Top = 96, Width = W - 44, Height = 20, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            _gProc.Controls.Add(_bar);

            _lStatus = new Label { Left = 14, Top = 122, Width = W - 44, Height = 20, Text = "-", Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            _gProc.Controls.Add(_lStatus);

            _txtLog = new TextBox
            {
                Left = 14,
                Top = 148,
                Width = W - 44,
                Height = 138,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 8.5f),
                BackColor = Color.FromArgb(250, 250, 250),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
            };
            _gProc.Controls.Add(_txtLog);
        }

        Label MkCap(int x, int y, string key) => new Label { Left = x, Top = y + 3, Width = 96, Height = 20, TextAlign = ContentAlignment.MiddleLeft, Tag = key };
        Label MkVal(int x, int y) => new Label { Left = x, Top = y + 3, Width = 700, Height = 20, TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
        Button MkBtn(int x, int y, int w, string key) { var b = new Button { Left = x, Top = y, Width = w, Height = 30, Tag = key }; return b; }

        void LoadEmbeddedList()
        {
            _cmbSource.Items.Clear();
            foreach (var (_, label) in Firmware.EmbeddedList()) _cmbSource.Items.Add(label);
            _cmbSource.Items.Add(Lang.T("btn.pick"));
            _cmbSource.SelectedIndex = 0;
        }

        void CmbSource_Changed(object sender, EventArgs e)
        {
            int idx = _cmbSource.SelectedIndex;
            var list = Firmware.EmbeddedList();
            if (idx >= 0 && idx < list.Count)
            {
                _fw = Firmware.FromEmbedded(list[idx].key);
                SetStatus(Lang.T("status.selected", _fw?.Name ?? "-"));
            }
            else if (idx == list.Count)
            {
                using var dlg = new OpenFileDialog { Filter = "Firmware S-record (*.S)|*.S;*.s|Semua file (*.*)|*.*", Title = Lang.T("btn.pick") };
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        _fw = Firmware.FromFile(dlg.FileName);
                        _cmbSource.Items[_cmbSource.Items.Count - 1] = Path.GetFileName(dlg.FileName);
                        _cmbSource.SelectedIndex = _cmbSource.Items.Count - 1;
                        SetStatus(Lang.T("status.selected", _fw.Name));
                    }
                    catch (Exception ex) { MessageBox.Show(this, ex.Message, Lang.T("msg.error"), MessageBoxButtons.OK, MessageBoxIcon.Error); _cmbSource.SelectedIndex = 0; }
                }
                else
                {
                    _cmbSource.SelectedIndex = 0;
                }
            }
            UpdateFirmwarePanel();
        }

        void UpdateFirmwarePanel()
        {
            if (_fw == null) { _lFwName.Text = _lFwSize.Text = _lFwSha.Text = _lFwCompat.Text = "-"; return; }
            _lFwName.Text = _fw.Name;
            _lFwSize.Text = _fw.SizeText;
            _lFwSha.Text = _fw.Sha256;
            _lFwCompat.Text = _fw.CompatibleCameo5 ? Lang.T("compat.ok") : Lang.T("compat.bad");
            _lFwCompat.ForeColor = _fw.CompatibleCameo5 ? Color.FromArgb(0, 128, 0) : Color.Firebrick;
        }

        public void RefreshDevice()
        {
            try
            {
                var list = UsbDevice.Enumerate();
                _dev = list.Count > 0 ? list[0] : null;
            }
            catch { _dev = null; }

            if (_dev == null)
            {
                _lModel.Text = _lSerial.Text = _lMode.Text = Lang.T("lbl.none");
                SetStatus(Lang.T("status.nodevice"));
            }
            else
            {
                _lModel.Text = _dev.Model;
                _lSerial.Text = _dev.Serial;
                _lMode.Text = Lang.T(_dev.ModeKey);
                SetStatus(Lang.T("status.device", _dev.Model, _dev.Serial, Lang.T(_dev.ModeKey)));
            }
        }

        async void StartRepair()
        {
            if (_busy) return;
            if (_fw == null) { MessageBox.Show(this, Lang.T("msg.fwbad.text"), Lang.T("msg.warn"), MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (!_fw.CompatibleCameo5) { MessageBox.Show(this, Lang.T("msg.fwbad.text"), Lang.T("msg.warn"), MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            RefreshDevice();
            if (_dev == null)
            {
                MessageBox.Show(this, Lang.T("msg.nodevice.text"), Lang.T("msg.warn"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var ok = MessageBox.Show(this,
                Lang.T("msg.confirm.text", _dev.Model, _dev.Serial, Lang.T(_dev.ModeKey), _fw.Name),
                Lang.T("msg.question"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (ok != DialogResult.Yes) return;

            _busy = true;
            SetBusy(true);
            _bar.Value = 0;
            var devPath = _dev.Path;
            var data = _fw.Bytes;
            bool dry = _chkDry.Checked;
            Log.Write("Mulai flashing: " + _fw.Name + " (" + _fw.Size + " byte) ke " + devPath + (dry ? " [DRY-RUN]" : ""));

            var progress = new Progress<(int pct, string text)>(v =>
            {
                _bar.Value = Math.Max(0, Math.Min(100, v.pct));
                _lStatus.Text = Lang.T("status.flashing") + " " + v.text;
            });

            try
            {
                var res = await Task.Run(() =>
                    Flasher.Flash(devPath, data, Flasher.DefaultChunk, Flasher.DefaultDelayMs, dry,
                        (p, t) => ((IProgress<(int, string)>)progress).Report((p, t)), CancellationToken.None));

                if (res.Ok)
                {
                    Log.Write("Flashing selesai. terkirim=" + res.BytesSent + "/" + res.TotalBytes);
                    _bar.Value = 100;
                    SetStatus(Lang.T("status.done"));
                    MessageBox.Show(this, Lang.T("msg.done.text", dry ? Lang.T("msg.sim.done") : ""), Lang.T("msg.info"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    Log.Write("Flashing gagal: " + res.Error);
                    SetStatus(Lang.T("status.failed", res.Error));
                    MessageBox.Show(this, res.Error, Lang.T("msg.error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Log.Write("Flashing exception: " + ex.Message);
                SetStatus(Lang.T("status.failed", ex.Message));
                MessageBox.Show(this, ex.Message, Lang.T("msg.error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _busy = false;
                SetBusy(false);
                FlushLog();
            }
        }

        void VerifyResult()
        {
            RefreshDevice();
            if (_dev == null)
            {
                SetStatus(Lang.T("status.nodevice"));
                MessageBox.Show(this, Lang.T("msg.nodevice.text"), Lang.T("msg.warn"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!_dev.LooksLikeBootloader)
            {
                SetStatus(Lang.T("status.verified.ok", _dev.Serial));
                MessageBox.Show(this, Lang.T("status.verified.ok", _dev.Serial), Lang.T("msg.info"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                SetStatus(Lang.T("status.verified.still"));
                MessageBox.Show(this, Lang.T("status.verified.still"), Lang.T("msg.warn"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            FlushLog();
        }

        void SaveLog()
        {
            using var dlg = new SaveFileDialog { Filter = "Log (*.txt)|*.txt", FileName = "cameorescue-log.txt" };
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                try { Log.SaveTo(dlg.FileName); MessageBox.Show(this, Lang.T("msg.logsaved", dlg.FileName), Lang.T("msg.info")); }
                catch (Exception ex) { MessageBox.Show(this, ex.Message, Lang.T("msg.error")); }
            }
        }

        void SetBusy(bool busy)
        {
            _btnStart.Enabled = _btnVerify.Enabled = _btnRefresh.Enabled = _cmbSource.Enabled = !busy;
        }

        void SetStatus(string s) { _lStatus.Text = s; Log.Write("[status] " + s); FlushLog(); }

        void FlushLog()
        {
            _txtLog.Text = Log.Text;
            _txtLog.SelectionStart = _txtLog.TextLength;
            _txtLog.ScrollToCaret();
        }

        public void ApplyLang()
        {
            _gDev.Text = Lang.T("flash.grp.device");
            _gFw.Text = Lang.T("flash.grp.firmware");
            _gProc.Text = Lang.T("flash.grp.process");
            _btnRefresh.Text = Lang.T("btn.refresh");
            _chkDry.Text = Lang.T("chk.dry");
            _btnStart.Text = Lang.T("btn.start");
            _btnVerify.Text = Lang.T("btn.verify");
            _btnSaveLog.Text = Lang.T("btn.saveLog");
            _btnCopyLog.Text = Lang.T("btn.copyLog");
            _btnClearLog.Text = Lang.T("btn.clearLog");
            foreach (Control c in _gDev.Controls) if (c.Tag is string k && c is Label) c.Text = Lang.T(k);
            foreach (Control c in _gFw.Controls) if (c.Tag is string k && c is Label) c.Text = Lang.T(k);

            // combo item terakhir = tombol pilih file
            var list = Firmware.EmbeddedList();
            if (_cmbSource.Items.Count > list.Count)
                _cmbSource.Items[list.Count] = Lang.T("btn.pick");

            if (_dev != null) _lMode.Text = Lang.T(_dev.ModeKey);
            UpdateFirmwarePanel();
        }
    }
}
