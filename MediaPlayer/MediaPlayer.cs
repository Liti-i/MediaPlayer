using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MediaPlayer
{
    public partial class frmMediaPlayer : Form
    {
        private const int WM_GETMINMAXINFO = 0x24;
        private int videoWidth = 0;
        private int videoHeight = 0;
        private Timer playbackTimer;
        private double totalDuration = 0;

        public frmMediaPlayer()
        {
            InitializeComponent();
            // 訂閱影片開啟完成事件
            wmpVideo.OpenStateChange += WmpVideo_OpenStateChange;
            
            // 初始化計時器用於更新播放進度
            playbackTimer = new Timer();
            playbackTimer.Interval = 500; // 每500毫秒更新一次
            playbackTimer.Tick += PlaybackTimer_Tick;
        }

        private void PlaybackTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                // 取得當前播放時間
                double currentPosition = wmpVideo.Ctlcontrols.currentPosition;

                // 將秒數轉換為 時:分:秒 格式
                int curHours = (int)(currentPosition / 3600);
                int curMinutes = (int)((currentPosition % 3600) / 60);
                int curSeconds = (int)(currentPosition % 60);

                int totalHours = (int)(totalDuration / 3600);
                int totalMinutes = (int)((totalDuration % 3600) / 60);
                int totalSeconds = (int)(totalDuration % 60);

                // 更新標籤顯示當前播放時間 / 總時長
                lblDuration.Text = $"{curHours:D2}:{curMinutes:D2}:{curSeconds:D2} / {totalHours:D2}:{totalMinutes:D2}:{totalSeconds:D2}";
            }
            catch
            {
                // 忽略計時更新期間的錯誤
            }
        }

        private void WmpVideo_OpenStateChange(object sender, AxWMPLib._WMPOCXEvents_OpenStateChangeEvent e)
        {
            // 當影片完全開啟時
            if (e.newState == 13) // 13 = wmppMediaOpen
            {
                try
                {
                    // 取得影片的實際寬度和高度
                    videoWidth = wmpVideo.currentMedia.imageSourceWidth;
                    videoHeight = wmpVideo.currentMedia.imageSourceHeight;

                    // 取得影片時長（以秒為單位）
                    totalDuration = wmpVideo.currentMedia.duration;
                    
                    // 將秒數轉換為 時:分:秒 格式
                    int hours = (int)(totalDuration / 3600);
                    int minutes = (int)((totalDuration % 3600) / 60);
                    int seconds = (int)(totalDuration % 60);
                    
                    // 更新標籤顯示時長
                    lblDuration.Text = $"00:00:00 / {hours:D2}:{minutes:D2}:{seconds:D2}";
                    
                    // 啟動計時器以更新播放進度
                    playbackTimer.Start();

                    // 設定視窗最小尺寸（加上按鈕面板的高度）
                    int minHeight = videoHeight + palButton.Height;
                    this.MinimumSize = new Size(videoWidth, minHeight);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"錯誤：{ex.Message}", "載入影片失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        protected override void WndProc(ref Message m)
        {
            // 處理視窗最小尺寸限制
            if (m.Msg == WM_GETMINMAXINFO)
            {
                if (videoWidth > 0 && videoHeight > 0)
                {
                    MINMAXINFO mmi = (MINMAXINFO)System.Runtime.InteropServices.Marshal.PtrToStructure(m.LParam, typeof(MINMAXINFO));
                    int minHeight = videoHeight + palButton.Height;
                    mmi.ptMinTrackSize = new Point(videoWidth, minHeight);
                    System.Runtime.InteropServices.Marshal.StructureToPtr(mmi, m.LParam, true);
                }
            }
            base.WndProc(ref m);
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        private struct MINMAXINFO
        {
            public System.Drawing.Point ptReserved;
            public System.Drawing.Point ptMaxSize;
            public System.Drawing.Point ptMaxPosition;
            public System.Drawing.Point ptMinTrackSize;
            public System.Drawing.Point ptMaxTrackSize;
        }

        private void axWindowsMediaPlayer1_Enter(object sender, EventArgs e)
        {

        }

        private void btnBrowser_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "WMV files (*.wmv)|*.wmv|MP4files(*.mp4) | *.mp4 | AVI files(*.avi) | *.avi | All files(*.*) | *.* ";
                if (ofd.ShowDialog() == DialogResult.OK)
            {
                wmpVideo.URL = ofd.FileName;
                wmpVideo.Ctlcontrols.stop(); // 停止
                playbackTimer.Stop(); // 停止計時器
                
                // 顯示上傳成功提示
                MessageBox.Show(
                    $"影片上傳成功！\n\n檔案名稱：{System.IO.Path.GetFileName(ofd.FileName)}\n\n請按播放按鈕開始播放",
                    "上傳成功",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
        }
        private void btnPlay_Click(object sender, EventArgs e)
        {
            wmpVideo.Ctlcontrols.play(); // 播放
            playbackTimer.Start(); // 啟動計時器
        }
        private void btnPause_Click(object sender, EventArgs e)
        {
            wmpVideo.Ctlcontrols.pause(); // 暫停
            playbackTimer.Stop(); // 停止計時器
        }
        private void btnStop_Click(object sender, EventArgs e)
        {
            wmpVideo.Ctlcontrols.stop(); // 停止
            playbackTimer.Stop(); // 停止計時器
            lblDuration.Text = "         "; // 清空時長顯示
        }
    }
}
