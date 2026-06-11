using System.Diagnostics;
using System.Windows.Forms.DataVisualization.Charting;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Windows.Forms.DataVisualization.Charting;
using System.Runtime.InteropServices;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using System.Text;




namespace Donkey_car_manager
{
    public partial class Form1 : Form
    {
        // 클래스 내부 전역 변수 공간
        private float currentSteering = 0.0f; // -1.0(좌) ~ 1.0(우) 현재 조향값
        private float correctSteering = 0.0f; // -1.0(좌) ~ 1.0(우) 올바른 조향값 (★추가)

        // (기존에 있을) 현재 조향값 업데이트 메서드
        public void UpdateSteeringIndicator(float angle)
        {
            currentSteering = angle;
            picCurFrame.Invalidate(); // Paint 이벤트를 발생시켜 픽처박스를 다시 그림
        }

        // 올바른 조향값 업데이트 메서드 (★추가)
        public void UpdateCorrectSteeringIndicator(float angle)
        {
            correctSteering = angle;
            picCurFrame.Invalidate(); // Paint 이벤트를 발생시켜 픽처박스를 다시 그림
        }
        private async Task WaitForDonkeyServer()
        {
            using (HttpClient client = new HttpClient())
            {
                while (true)
                {
                    try
                    {
                        HttpResponseMessage response =
                            await client.GetAsync("http://localhost:8887");

                        if (response.IsSuccessStatusCode)
                            return;
                    }
                    catch
                    {
                    }

                    await Task.Delay(1000);
                }
            }
        }
        private async Task SetAutoSteer()
        {
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    using (ClientWebSocket ws = new ClientWebSocket())
                    {
                        await ws.ConnectAsync(
                            new Uri("ws://localhost:8887/wsDrive"),
                            CancellationToken.None);

                        string json =
                            "{\"drive_mode\":\"local_angle\"}";

                        byte[] buffer =
                            Encoding.UTF8.GetBytes(json);

                        await ws.SendAsync(
                            new ArraySegment<byte>(buffer),
                            WebSocketMessageType.Text,
                            true,
                            CancellationToken.None);

                        await ws.CloseAsync(
                            WebSocketCloseStatus.NormalClosure,
                            "",
                            CancellationToken.None);

                        return; // 성공하면 종료
                    }
                }
                catch
                {
                    await Task.Delay(2000); // 2초 후 재시도
                }
            }

        }

        private void SyncListViewMultiSelection()
        {
            // 이미지 데이터나 리스트뷰가 비어있으면 동작 불필요
            if (carImages == null || carImages.Count == 0 || lstFiles.Items.Count == 0) return;

            // 🌟 윈폼 내부의 연쇄적인 이벤트 오작동을 차단하기 위해 리스트뷰 핸들러를 일시 해제합니다.
            this.lstFiles.SelectedIndexChanged -= new System.EventHandler(this.lstFiles_SelectedIndexChanged);
            this.lstFiles.ItemSelectionChanged -= new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.lstFiles_ItemSelectionChanged);

            // 리스트뷰 UI 레이아웃 업데이트 시작 (화면 깜빡임 원천 차단)
            lstFiles.BeginUpdate();

            try
            {
                // 1. 기존 화면에 선택된 리스트뷰 하이라이트를 완전히 깔끔하게 지웁니다.
                lstFiles.SelectedItems.Clear();

                // 현재 리스트뷰 페이지에 뿌려져 있는 전역 인덱스의 시작점과 끝점 계산
                int pageStartIdx = currentPage * pageSize;
                int pageEndIdx = pageStartIdx + lstFiles.Items.Count;

                // 2. 다중 선택 바구니(selectedGlobalIndices)에 보관된 모든 인덱스를 순회
                foreach (int globalIdx in selectedGlobalIndices)
                {
                    // 보관된 전역 인덱스가 '현재 떠 있는 화면 페이지'의 범위 안에 속하는지 검사
                    if (globalIdx >= pageStartIdx && globalIdx < pageEndIdx)
                    {
                        // 현재 화면 내에서의 상대 인덱스(0 ~ 19번)로 역산
                        int localIdx = globalIdx % pageSize;

                        if (localIdx >= 0 && localIdx < lstFiles.Items.Count)
                        {
                            // 🌟 해당 리스트뷰 항목에 파란색 선택 불을 켭니다!
                            lstFiles.Items[localIdx].Selected = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"다중선택 동기화 실패: {ex.Message}");
            }
            finally
            {
                // 레이아웃 업데이트 종료 및 그래픽 반영
                lstFiles.EndUpdate();

                // 🌟 작업이 완벽히 완료되었으므로 안전하게 이벤트를 다시 연결합니다.
                this.lstFiles.SelectedIndexChanged += new System.EventHandler(this.lstFiles_SelectedIndexChanged);
                this.lstFiles.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.lstFiles_ItemSelectionChanged);
                // 다중 선택 상태에 따라 btnmp 보이기/숨기기 갱신
                UpdateBtnMpVisibility();
            }
        }

        // AI 스트리밍 전역 변수
        private System.Threading.CancellationTokenSource aiStreamCts;
        private System.Net.Http.HttpClient aiHttpClient;
        private PictureBox aiPictureBox;
        private bool aiStreaming = false;

        // button1 클릭 이벤트: 토글형으로 aiPictureBox 생성/스트림 시작 또는 중지/제거
        private async void button1_Click(object sender, EventArgs e)
        {


            string linuxUser = txtLinuxUser.Text.Trim();

            string cmd =
            $"source /home/{linuxUser}/miniconda3/bin/activate e2e_env && " +
            $"cd /home/{linuxUser}/mysim && " +
            $"python manage.py drive --model models/mypilot.h5";

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "wsl.exe",
                Arguments = $"-d Ubuntu-22.04 bash -c \"{cmd}\"",
                UseShellExecute = false,
                CreateNoWindow = false
            };



            if (!aiStreaming)
            {
                Process.Start(psi);

                // Donkey 서버가 준비될 때까지 기다림
                await WaitForDonkeyServer();

                // 웹페이지 자동 열기
                Process.Start(new ProcessStartInfo(
                    "http://localhost:8887/drive")
                {
                    UseShellExecute = true
                });

                await Task.Delay(5000);

                // PictureBox 지정
                aiPictureBox = picCurFrame;

                // 스트림 객체 생성
                aiStreamCts = new System.Threading.CancellationTokenSource();
                aiHttpClient = new System.Net.Http.HttpClient();
                aiStreaming = true;

                // AutoSteer 설정
                await SetAutoSteer();

                // 버튼 상태 변경
                btnStartAuto.Text = "자율주행 종료";

                // 영상 스트림 시작
                await Task.Run(() =>
                    RunMjpegStream(
                        "http://localhost:8887/video",
                        aiStreamCts.Token));
            }
            else
            {
                // 중지 및 제거
                try { aiStreamCts?.Cancel(); aiHttpClient?.CancelPendingRequests(); } catch { }
                aiStreamCts?.Dispose(); aiStreamCts = null;
                aiHttpClient?.Dispose(); aiHttpClient = null;
                aiStreaming = false;

                if (this.IsHandleCreated)
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            if (aiPictureBox != null)
                            {

                                aiPictureBox.Image = null;


                            }
                        }
                        catch { }
                        // 버튼 텍스트 복원
                        btnStartAuto.Text = "자율주행 시작";
                    }));
                }
            }
        }



        // MJPEG 스트림 읽기 (SOI/EOI로 프레임 분리)
        private async Task RunMjpegStream(string url, System.Threading.CancellationToken token)
        {
            try
            {
                using (var client = aiHttpClient ?? new System.Net.Http.HttpClient())
                using (var resp = await client.GetAsync(
                           url,
                           System.Net.Http.HttpCompletionOption.ResponseHeadersRead,
                           token))
                {
                    if (!resp.IsSuccessStatusCode)
                        throw new Exception("HTTP error");

                    using (var stream = await resp.Content.ReadAsStreamAsync(token))
                    {
                        var buf = new byte[4096];
                        var ms = new System.IO.MemoryStream();

                        while (!token.IsCancellationRequested)
                        {
                            int read = await stream.ReadAsync(
                                buf,
                                0,
                                buf.Length,
                                token);

                            if (read <= 0)
                                break;

                            ms.Write(buf, 0, read);

                            var data = ms.ToArray();

                            int soi = IndexOf(data, new byte[] { 0xFF, 0xD8 });
                            int eoi = IndexOf(data, new byte[] { 0xFF, 0xD9 });

                            if (soi >= 0 && eoi > soi)
                            {
                                int len = eoi - soi + 2;

                                byte[] jpeg = new byte[len];

                                Array.Copy(data, soi, jpeg, 0, len);

                                int remaining = data.Length - (soi + len);

                                ms.SetLength(0);

                                if (remaining > 0)
                                    ms.Write(data, soi + len, remaining);

                                ShowImageOnAiPictureBox(jpeg);
                            }
                        }
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // 종료 버튼 눌렀을 때 정상 종료
            }
            catch (OperationCanceledException)
            {
                // 정상 종료
            }
            catch (Exception ex)
            {
                if (this.IsHandleCreated)
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        MessageBox.Show(
                            ex.Message,
                            "MJPEG 스트림 오류",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }));
                }
            }
        }

        // UI 스레드에서 aiPictureBox에 이미지 표시
        private void ShowImageOnAiPictureBox(byte[] jpeg)
        {
            try
            {
                using (var ms = new System.IO.MemoryStream(jpeg))
                {
                    var img = Image.FromStream(ms);
                    if (this.IsHandleCreated)
                    {
                        this.BeginInvoke(new Action(() =>
                        {
                            try
                            {
                                if (aiPictureBox != null)
                                {
                                    var prev = aiPictureBox.Image;
                                    aiPictureBox.Image = (Image)img.Clone();
                                    prev?.Dispose();
                                }
                            }
                            catch { }
                        }));
                    }
                }
            }
            catch { }
        }

        private static int IndexOf(byte[] haystack, byte[] needle)
        {
            if (haystack == null || needle == null || haystack.Length < needle.Length) return -1;
            for (int i = 0; i <= haystack.Length - needle.Length; i++)
            {
                bool ok = true;
                for (int j = 0; j < needle.Length; j++)
                {
                    if (haystack[i + j] != needle[j]) { ok = false; break; }
                }
                if (ok) return i;
            }
            return -1;
        }
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);
        private const int EM_SETCUEBANNER = 0x1501;
        // 🌟 현재 실행 중인 학습 프로세스를 기억하고 제어하기 위한 변수
        private Process trainProcess = null;
        // 🌟 현재 사용자가 파일 열기로 연 폴더의 윈도우 절대 경로를 기억할 변수
        private string currentSelectedFolderPath = string.Empty;
        // 🌟 선택된 윈도우 동키카 시뮬레이터 파일(.exe)의 절대 경로를 기억할 변수
        private string selectedSimFilePath = string.Empty;
        public class CarFileInfo
        {
            public string FilePath { get; set; }
            public DateTime WriteTime { get; set; }
            public float Angle { get; set; }      // -1.0 ~ 1.0
            public float Throttle { get; set; }   // 0.0 ~ 1.0 (혹은 전후진 포함 -1.0 ~ 1.0)
        }

        // Designer에 연결된 Load 이벤트 핸들러 (빈 구현)
        // Designer에 연결된 Load 이벤트 핸들러
        private void Form1_Load(object sender, EventArgs e) // 기존 코드
        {
            // 🌟 [추가] 텍스트박스 클릭 시 사라지는 회색 워터마크 힌트 텍스트 설정
            SendMessage(txtLinuxUser.Handle, EM_SETCUEBANNER, 0, "우분투 사용자명");
            SendMessage(txtMyCarFolder.Handle, EM_SETCUEBANNER, 0, "동키카 폴더명");
        }
        //  이 코드를 새로 넣어줍니다.
        private List<CarFileInfo> carImages = new List<CarFileInfo>();
        // 🌟 페이지가 바뀌어도 사용자가 Ctrl로 선택한 모든 파일의 '전체 인덱스'를 기억할 바구니
        private HashSet<int> selectedGlobalIndices = new HashSet<int>();

        // 정렬 상태(오름차순/내림차순)를 기억할 패스포트 변수도 같이 추가해 줍니다.
        private bool isAscending = true;
        // 1. 기존 이미지 및 페이징 변수들
        //private List<string> carImages = new List<string>();
        private int currentImageIndex = 0;
        private int pageSize = 20;
        private int currentPage = 0;
        private int totalPages = 0;
        // 화면에 한 번에 표시되는 이미지 개수 (visibleCount)
        private int visibleCount = 20;

        // 2. 기록창 확장을 위한 전역 변수들 (클래스 바로 아래 정상 배치)
        private bool isDebugExpanded = false;
        private int normalWidth = 1147;
        private int expandedWidth = 1600;
        private int targetWidth = 1147;
        // 🌟 기존 전역 변수들이 있는 곳(class Form1 바로 아래)에 추가하세요!
        private bool isPlaying = false;
        // 재생용: 사용자가 Shift로 트랙바에서 선택한 전역 인덱스들만 재생하기 위한 큐
        private List<int> playQueue = null;
        private int playQueuePos = 0;
        private bool isPlayingSelectedRange = false;

        // 🌟 코드 창 빈 곳(메서드 바깥)에 추가하세요! 자동 넘기기를 안전하게 끄는 메서드입니다.
        private void StopAutoPlay()
        {
            timerPlay.Stop();
            btnAutoPic.Text = "자동 넘기기";
            btnAutoPic.BackColor = SystemColors.Control; // 버튼 색상 원상복구
                                                         // 버튼 원래대로 복구
            btnmp.Text = "다중 선택된 부분만 재생";      // 원래 버튼에 쓰여있던 글자로 바꿔
            btnmp.BackColor = SystemColors.Control;

            isPlaying = false;
            // 다중 선택 재생 모드 초기화
            isPlayingSelectedRange = false;
            playQueue = null;
            playQueuePos = 0;
        }
        // 3. 프로그램 생성자 (대괄호 짝을 완벽히 맞춤)
        public Form1()
        {
            InitializeComponent();
            this.lstFiles.MouseWheel += new MouseEventHandler(lstFiles_MouseWheel);
            this.lstFiles.HideSelection = false;
            // 🌟 픽처박스의 깜빡임과 이미지 튀는 현상을 방지하는 더블 버퍼링 활성화
            System.Reflection.PropertyInfo aProp = typeof(System.Windows.Forms.Control)
                .GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            aProp.SetValue(picCurFrame, true, null);
            // Init chart 영역
            InitDriveChart();
            // Ensure chart is brought to front after form is shown and report status
            this.Shown += Form1_Shown;
            // 파일을 열기 전에는 visible range 라벨을 숨김
            try { if (lblFilenumber != null) lblFilenumber.Visible = false; } catch { }
            // Delete 키로 선택된 파일을 삭제할 수 있도록 폼에서 키 프리뷰를 허용하고 핸들러 등록
            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;
            picCurFrame.Paint += picCurFrame_Paint;
        }

        // 폼 KeyDown 핸들러: Delete 키로 선택된 파일 삭제 트리거
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            // 텍스트 입력창 등에서 키 입력을 방해하지 않도록 예외 처리
            if (this.ActiveControl is System.Windows.Forms.TextBoxBase || this.ActiveControl is System.Windows.Forms.ComboBox || this.ActiveControl is System.Windows.Forms.NumericUpDown)
            {
                return;
            }

            // Delete 키: 기존 다중 삭제 로직 트리거
            if (e.KeyCode == Keys.Delete)
            {
                bool hasSelected = (selectedGlobalIndices != null && selectedGlobalIndices.Count > 0) || (lstFiles != null && lstFiles.SelectedIndices.Count > 0);
                if (hasSelected)
                {
                    try { btnFileMultiDel_Click(btnFileMultiDel, EventArgs.Empty); } catch { }
                    e.Handled = true;
                }
                return;
            }

            // I/O 키: 재생 중인 경우에만 동작 (I = in(start), O = out(end))
            if (e.KeyCode == Keys.I || e.KeyCode == Keys.O)
            {
                // 영상(재생)이 실행 중인 경우에만 동작
                if (!(isPlaying || aiStreaming)) return;

                if (e.KeyCode == Keys.I)
                {
                    // 시작 마크
                    multiStartIndex = currentImageIndex;
                    multiEndIndex = multiStartIndex; // 즉시 시각적으로 표시되게 함
                    // 초기화된 커밋 범위 제거 (새로 시작)
                    committedRangeStart = -1;
                    committedRangeEnd = -1;

                    UpdateRangeHighlight();
                    UpdateSelectedFileLabel();
                    UpdatePlaySelectLabel();
                    UpdateBtnMpVisibility();
                }
                else if (e.KeyCode == Keys.O)
                {
                    // 끝 마크: 시작이 설정되어 있어야 작동
                    if (multiStartIndex == -1)
                    {
                        // 시작 없이 끝만 누르면 현재 인덱스를 시작으로 설정
                        multiStartIndex = currentImageIndex;
                    }

                    multiEndIndex = currentImageIndex;

                    int start = Math.Min(multiStartIndex, multiEndIndex);
                    int end = Math.Max(multiStartIndex, multiEndIndex);

                    // 선택 범위 추가
                    for (int gi = start; gi <= end; gi++)
                    {
                        if (gi >= 0 && gi < (carImages?.Count ?? 0))
                        {
                            selectedGlobalIndices.Add(gi);
                        }
                    }

                    // 커밋된 범위로 보관
                    committedRangeStart = start;
                    committedRangeEnd = end;

                    // 임시 인덱스 리셋
                    multiStartIndex = -1;
                    multiEndIndex = -1;

                    // UI 동기화
                    SyncListViewMultiSelection();
                    UpdateRangeHighlight();
                    UpdateSelectedFileLabel();
                    UpdatePlaySelectLabel();
                    UpdateBtnMpVisibility();
                }

                e.Handled = true;
                return;
            }
        }
        // 동키카 그래프를 위한 코드
        private Chart chartDriveData; // 전역 변수로 차트 선언
        // 차트의 실시간 스크롤(슬라이딩 윈도우) 설정
        private int chartWindowSize = 100; // 화면에 표시할 최신 포인트 수
        private bool trimOldChartData = false; // 오래된 데이터를 실제로 삭제할지 여부

        private void InitDriveChart()
        {
            // Chart 생성
            chartDriveData = new Chart();
            chartDriveData.Name = "chartDriveData";

            // dgvDebug 위치/크기/앵커 그대로 복사
            if (dgvDebug != null)
            {
                chartDriveData.Location = dgvDebug.Location;
                chartDriveData.Size = dgvDebug.Size;
                chartDriveData.Anchor = dgvDebug.Anchor;
            }

            // 다크 테마
            chartDriveData.BackColor = Color.FromArgb(18, 18, 18);

            // 차트 내부 영역 설정
            chartDriveData.Series.Clear();
            chartDriveData.ChartAreas.Clear();
            chartDriveData.Legends.Clear();

            ChartArea area = new ChartArea("DriveArea");
            area.BackColor = Color.FromArgb(18, 18, 18);
            area.AxisX.Title = "프레임 번호";
            area.AxisX.TitleForeColor = Color.WhiteSmoke;
            area.AxisX.LabelStyle.ForeColor = Color.LightGray;
            area.AxisX.MajorGrid.LineColor = Color.FromArgb(60, 60, 60);
            area.AxisX.LineColor = Color.LightGray;

            area.AxisY.Title = "값";
            area.AxisY.TitleForeColor = Color.WhiteSmoke;
            area.AxisY.LabelStyle.ForeColor = Color.LightGray;
            area.AxisY.MajorGrid.LineColor = Color.FromArgb(60, 60, 60);
            area.AxisY.Minimum = -1.0;
            area.AxisY.Maximum = 1.0;

            chartDriveData.ChartAreas.Add(area);

            // Legend
            Legend legend = new Legend();
            legend.BackColor = Color.FromArgb(30, 30, 30);
            legend.ForeColor = Color.WhiteSmoke;
            chartDriveData.Legends.Add(legend);

            // Series: user/angle (red)
            Series sAngle = new Series("user/angle");
            sAngle.ChartType = SeriesChartType.Line;
            sAngle.Color = Color.Red;
            sAngle.BorderWidth = 2;
            sAngle.ChartArea = "DriveArea";
            sAngle.IsXValueIndexed = true;
            sAngle.LegendText = "조향각";

            // Series: user/throttle (blue)
            Series sThrottle = new Series("user/throttle");
            sThrottle.ChartType = SeriesChartType.Line;
            sThrottle.Color = Color.Blue;
            sThrottle.BorderWidth = 2;
            sThrottle.ChartArea = "DriveArea";
            sThrottle.IsXValueIndexed = true;
            sThrottle.LegendText = "속도";

            chartDriveData.Series.Add(sAngle);
            chartDriveData.Series.Add(sThrottle);

            // 기존 dgvDebug 숨기기
            if (dgvDebug != null)
            {
                dgvDebug.Visible = false;
                // 폼의 Controls에 남아 있으면 제거하여 차트가 가려지지 않도록 함
                if (this.Controls.Contains(dgvDebug)) this.Controls.Remove(dgvDebug);
            }

            // 폼에 차트 추가
            this.Controls.Add(chartDriveData);
            chartDriveData.Visible = true;
            chartDriveData.BringToFront();

        }

        private void AddDriveDataToChart(int frameIndex, double angle, double throttle)
        {
            if (chartDriveData == null) return;
            if (!chartDriveData.Series.IsUniqueName("user/angle") || !chartDriveData.Series.IsUniqueName("user/throttle"))
            {
                // 정상적으로 시리즈가 존재하면 추가
            }

            Series sA = chartDriveData.Series["user/angle"];
            Series sT = chartDriveData.Series["user/throttle"];
            // 새 데이터 추가
            sA.Points.AddXY(frameIndex, angle);
            sT.Points.AddXY(frameIndex, throttle);

            // 실시간 슬라이딩 윈도우: AxisX 범위를 최신 chartWindowSize 포인트로 이동
            ChartArea area = chartDriveData.ChartAreas.IndexOf("DriveArea") >= 0 ? chartDriveData.ChartAreas["DriveArea"] : null;
            if (area != null)
            {
                double latest = frameIndex;
                double minVisible;
                double maxVisible = latest;

                if (latest >= chartWindowSize - 1)
                {
                    minVisible = latest - (chartWindowSize - 1);
                }
                else
                {
                    minVisible = 0;
                    // ensure maximum shows at least the window size early on
                    maxVisible = Math.Max(latest, chartWindowSize - 1);
                }

                try
                {
                    area.AxisX.Minimum = minVisible;
                    area.AxisX.Maximum = maxVisible;
                }
                catch { }

                // 필요 시 오래된 포인트를 실제로 제거하여 성능 최적화
                if (trimOldChartData && chartWindowSize > 0)
                {
                    double threshold = minVisible;
                    foreach (var s in chartDriveData.Series)
                    {
                        // while 첫 포인트가 임계값보다 작으면 제거
                        while (s.Points.Count > 0 && s.Points[0].XValue < threshold)
                        {
                            s.Points.RemoveAt(0);
                        }
                    }
                }

                // 차트 갱신
                try { chartDriveData.Invalidate(); } catch { }
            }
        }
        //메세지박스 뜨는거 주석처리했습니다
        private void Form1_Shown(object sender, EventArgs e)
        {
            if (chartDriveData == null)
            {
                //MessageBox.Show("chartDriveData == null", "진단");
            }
            else
            {
                string parentName = chartDriveData.Parent != null ? chartDriveData.Parent.Name : "null";
                //MessageBox.Show($"Visible={chartDriveData.Visible}, Parent={parentName}, Bounds={chartDriveData.Bounds}, InControls={this.Controls.Contains(chartDriveData)}", "진단");
                chartDriveData.BringToFront();
                chartDriveData.Refresh();
            }
        }

        // 4. 폼 로드 이벤트
        /*private void Form1_Load(object sender, EventArgs e)
        {
            // 시작할 때는 기본 크기로 설정
            this.Width = normalWidth;
            targetWidth = normalWidth;

            //디버깅 차트를 위한 코드
            dgvDebug.Columns.Clear();
            dgvDebug.Columns.Add("Time", "시간 (Ticks)");
            dgvDebug.Columns.Add("Steer", "조향값 (Steering)");
            dgvDebug.Columns.Add("Throttle", "쓰로틀 (Throttle)");

            // 보기 좋게 정렬 및 크기 자동 맞춤
            dgvDebug.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }*/
        private int tickCount = 0;

        private void UpdateDataGridView(double steer, double throttle)
        {
            // 새로운 행 추가
            dgvDebug.Rows.Add(tickCount++, steer.ToString("F3"), throttle.ToString("F3"));

            // 데이터가 너무 많아지면 스크롤을 가장 아래로 자동으로 내려줌
            if (dgvDebug.Rows.Count > 0)
            {
                dgvDebug.FirstDisplayedScrollingRowIndex = dgvDebug.Rows.Count - 1;
            }

            // 메모리 관리를 위해 너무 옛날 데이터(예: 100개 이전)는 삭제하고 싶다면 추가
            if (dgvDebug.Rows.Count > 100)
            {
                dgvDebug.Rows.RemoveAt(0);
            }
        }
        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btnFileOpen_Click(object sender, EventArgs e)
        {
            // 혹시 자동 재생(넘기기) 중이었다면 안전하게 멈추기
            if (isPlaying) StopAutoPlay();

            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    currentSelectedFolderPath = fbd.SelectedPath; //경로를 연 폴더의 위치를 저장
                    // 1. 선택한 폴더에서 JPG 이미지 파일 목록을 배열로 가져옵니다.
                    // 대소문자 .jpg, .JPG는 물론이고 .jpeg까지 알아서 다 찾아주는 코드입니다.
                    string imageFolder = Path.Combine(fbd.SelectedPath, "images");

                    if (!Directory.Exists(imageFolder))
                    {
                        MessageBox.Show("선택한 폴더 안에 images 폴더가 없습니다.");
                        return;
                    }

                    string[] files = Directory.GetFiles(imageFolder, "*.*")

                                            .Where(s => s.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                    s.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                                    s.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                                    s.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
                        .ToArray();

                    // 2. 아까 만든 새 바구니(carImages)를 깨끗하게 비워줍니다.
                    carImages.Clear();

                    // 3. 🌟 핵심: 파일을 로드하는 이 시점에 '딱 한 번만' 디스크에서 수정 시간을 읽어와 저장합니다.
                    // 이렇게 메모리에 미리 다 올려두어야 나중에 정렬할 때 렉이 전혀 안 걸립니다.
                    // 파일 오픈 시 반복문 내부 예시
                    foreach (string file in files)
                    {
                        float angle = 0f;
                        float throttle = 0f;

                        // 예시: 동키카 기본 저장 포맷 파싱 (파일명 구조: _user_angle_XXX_user_throttle_YYY_)
                        // 파일명 규칙에 맞게 정규식(Regex)이나 Split 문으로 조향/쓰로틀 값을 파싱해줍니다.
                        string fileName = Path.GetFileNameWithoutExtension(file);
                        // [개발자님의 파일네임 파싱 알고리즘 추가 공간]

                        carImages.Add(new CarFileInfo
                        {
                            FilePath = file,
                            WriteTime = System.IO.File.GetLastWriteTime(file),
                            Angle = angle,
                            Throttle = throttle
                        });
                    }

                    // 4. 처음 폴더를 열었을 때는 파일 이름순(오름차순)으로 기본 정렬해 줍니다.
                    carImages = carImages.OrderBy(f => System.IO.Path.GetFileName(f.FilePath)).ToList();

                    // 5. 페이징 및 인덱스 변수 초기화 계산
                    totalPages = (int)Math.Ceiling((double)carImages.Count / pageSize);
                    currentImageIndex = 0;
                    currentPage = 0;

                    // 6. 트랙바(슬라이더) 범위 지정 (데이터가 없으면 0, 있으면 개수 - 1)
                    trbFrame.Maximum = Math.Max(0, carImages.Count - 1);
                    trbFrame.Value = 0;

                    // visibleCount 설정(화면에 한 번에 보이는 이미지 수)
                    visibleCount = pageSize;
                    UpdateVisibleRangeLabel();

                    // 7. 새로 바뀐 데이터를 화면(ListView와 PictureBox)에 그려줍니다.
                    UpdateListPage();
                    ShowImage(currentImageIndex);

                    MessageBox.Show($"총 {carImages.Count}개의 프레임 이미지를 성공적으로 로드했습니다.", "완료");
                }
            }
        }

        private void trbFrame_Scroll(object sender, EventArgs e)
        {
            if (carImages != null && carImages.Count > 0)
            {
                // 트랙바의 현재 위치 값을 인덱스로 설정
                currentImageIndex = trbFrame.Value;
                ShowImage(currentImageIndex);

                // 현재 이미지가 속한 페이지 계산
                currentPage = currentImageIndex / pageSize;


                // 리스트 새로고침
                UpdateListPage();

                // 페이지 안에서 몇 번째인지 계산
                int localIndex =
                    currentImageIndex % pageSize;


                if (localIndex >= 0 &&
                    localIndex < lstFiles.Items.Count)
                {
                    lstFiles.Items[localIndex].Selected = true;
                    lstFiles.Items[localIndex].Focused = true;
                    lstFiles.EnsureVisible(localIndex);

                }


            }
        }
        private void ShowImage(int index)
        {
            // 인덱스가 범위를 벗어나면 아무것도 하지 않음
            if (index < 0 || index >= carImages.Count) return;

            using (System.IO.FileStream fs = new System.IO.FileStream(carImages[index].FilePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                picCurFrame.Image = Image.FromStream(fs);

                Random r = new Random();

                double angle =
                    r.NextDouble() * 2 - 1;

                double throttle =
                    r.NextDouble();

                AddDriveDataToChart(
                    chartDriveData.Series["user/angle"].Points.Count,
                    angle,
                    throttle);
                // 🌟 [추가] 새로 계산된 조향값을 바늘(인디케이터)에 전달하고 픽처박스를 다시 그리게 합니다.
                UpdateSteeringIndicator((float)angle);
                chartDriveData.Refresh();
            }

            // 🌟 [추가] 트랙바나 자동 재생 시에도 상단 라벨이 "프레임 번호 : X / Y" 형태로 실시간 동기화되도록 설정
            lblFrameNum.Text = $"프레임 번호 : {index + 1} / {carImages.Count}";
            // visible range label 업데이트
            UpdateVisibleRangeLabel();
        }

        private int multiStartIndex = -1;
        private int multiEndIndex = -1;
        // 마지막으로 확정된(커밋된) 트랙바 범위(시각적 표시용)
        private int committedRangeStart = -1;
        private int committedRangeEnd = -1;




        private void UpdateRangeHighlight()
        {
            // 범위가 설정되지 않았으면 숨김
            // 단, 커밋된 범위(committedRange)가 있으면 그 범위를 표시해야 함
            if ((committedRangeStart == -1 || committedRangeEnd == -1) && (multiStartIndex == -1 || multiEndIndex == -1))
            {
                panelRange.Visible = false;
                return;
            }

            // 우선적으로 커밋된 범위를 사용
            int start, end;
            if (committedRangeStart != -1 && committedRangeEnd != -1)
            {
                start = committedRangeStart;
                end = committedRangeEnd;
            }
            else
            {
                start = Math.Min(multiStartIndex, multiEndIndex);
                end = Math.Max(multiStartIndex, multiEndIndex);
            }

            // 트랙바의 최소/최대값 범위 확보 (0으로 나누는 것을 방지)
            int tbMin = trbFrame.Minimum;
            int tbMax = trbFrame.Maximum;
            int span = Math.Max(1, tbMax - tbMin);

            double ratioStart = (double)(start - tbMin) / span;
            double ratioEnd = (double)(end - tbMin) / span;

            int x1 = trbFrame.Left + (int)(trbFrame.Width * ratioStart);
            int x2 = trbFrame.Left + (int)(trbFrame.Width * ratioEnd);

            // 패널 세로 위치를 트랙바 중앙에 맞춤
            int desiredHeight = Math.Max(6, panelRange.Height);
            panelRange.Height = desiredHeight;
            panelRange.Top = trbFrame.Top + Math.Max(0, trbFrame.Height / 2 - panelRange.Height / 2);
            panelRange.Left = Math.Min(x1, x2);
            panelRange.Width = Math.Max(5, Math.Abs(x2 - x1));

            panelRange.Visible = true;
            panelRange.BringToFront();
        }

        // 트랙바에 보이는 현재 이미지 범위를 label2와 lblVisibleRange에 표시
        private void UpdateVisibleRangeLabel()
        {
            int total = carImages?.Count ?? 0;
            if (total == 0)
            {
                try { if (lblFilenumber != null) lblFilenumber.Text = "0 ~ 0"; } catch { }
                return;
            }

            // lstFiles에 실제로 표시된 항목 기준으로 계산
            int startIdx = currentPage * pageSize;
            int shownCount = lstFiles?.Items?.Count ?? 0;
            if (shownCount <= 0)
            {
                try { if (lblFilenumber != null) lblFilenumber.Text = "0 ~ 0"; } catch { }
                return;
            }

            int firstShown = startIdx;
            int lastShown = startIdx + shownCount - 1;
            // 범위가 전체 개수 내에 있도록 조정
            firstShown = Math.Max(0, Math.Min(total - 1, firstShown));
            lastShown = Math.Max(0, Math.Min(total - 1, lastShown));

            string text = $"{firstShown + 1} ~ {lastShown + 1}";
            try { if (lblFilenumber != null) lblFilenumber.Text = text; } catch { }
        }


        // selectedGlobalIndices 기반으로 lblFilenumber에 선택된 파일 번호를 표시
        private void UpdateSelectedFileLabel()
        {
            if (lblFilenumber == null) return;

            if (selectedGlobalIndices == null || selectedGlobalIndices.Count == 0)
            {
                UpdateVisibleRangeLabel();
                return;
            }

            var sorted = selectedGlobalIndices.OrderBy(i => i).ToList();

            List<string> ranges = new List<string>();

            int start = sorted[0];
            int end = sorted[0];

            for (int i = 1; i < sorted.Count; i++)
            {
                if (sorted[i] == end + 1)
                {
                    end = sorted[i];
                }
                else
                {
                    ranges.Add(start == end
                        ? $"{start + 1}"
                        : $"{start + 1} ~ {end + 1}");

                    start = end = sorted[i];
                }
            }

            ranges.Add(start == end
                ? $"{start + 1}"
                : $"{start + 1} ~ {end + 1}");

            lblFilenumber.Text = string.Join(", ", ranges);
        }

        // 선택 상태에 따라 btnmp(선택 재생 버튼)의 Visible을 갱신한다.
        private void UpdateBtnMpVisibility()
        {
            bool shouldShow = false;

            if (selectedGlobalIndices != null && selectedGlobalIndices.Count > 1)
            {
                shouldShow = true;
            }
            else if (lstFiles != null && lstFiles.SelectedIndices != null && lstFiles.SelectedIndices.Count > 1)
            {
                shouldShow = true;
            }

            try
            {
                if (btnmp != null)
                    btnmp.Visible = true;
            }
            catch { }
        }

        private void trbFrame_MouseDown(object sender, MouseEventArgs e)
        {
            panelRange.Height = 6; //
            panelRange.BackColor = Color.DeepSkyBlue; //

            int newValue = trbFrame.Minimum + (trbFrame.Maximum - trbFrame.Minimum) * e.X / trbFrame.Width; //

            trbFrame.Value = Math.Max(trbFrame.Minimum, Math.Min(trbFrame.Maximum, newValue)); //

            if ((ModifierKeys & Keys.Shift) == Keys.Shift) //
            {
                if (multiStartIndex == -1) //
                {
                    // 첫 번째 클릭: 새로 시작하므로 이전 선택 모두 초기화
                    selectedGlobalIndices.Clear(); //
                    committedRangeStart = -1; //
                    committedRangeEnd = -1; //
                                            // 시작 인덱스 설정
                    multiStartIndex = newValue; //
                }
                else
                {
                    // 두 번째 클릭: 범위 확정
                    multiEndIndex = newValue; //

                    int start = Math.Min(multiStartIndex, multiEndIndex); //
                    int end = Math.Max(multiStartIndex, multiEndIndex); //

                    // 선택 범위의 모든 전역 인덱스를 selectedGlobalIndices에 추가
                    for (int gi = start; gi <= end; gi++) //
                    {
                        if (gi >= 0 && gi < (carImages?.Count ?? 0)) //
                        {
                            selectedGlobalIndices.Add(gi); //
                        }
                    }

                    // 커밋된 범위로 보관하여 시각적으로 표시
                    committedRangeStart = start; //
                    committedRangeEnd = end; //

                    // 다음 선택을 위해 임시 시작 인덱스 초기화
                    multiStartIndex = -1; //
                    multiEndIndex = -1; //

                    // 🌟 [수정] 전체를 매번 다 그리는 UpdateListPage() 대신, 리스트뷰에 선택 불만 켭니다.
                    SyncListViewMultiSelection();
                }

                UpdateRangeHighlight(); //
                UpdateSelectedFileLabel();
                UpdatePlaySelectLabel();

                UpdateBtnMpVisibility();
            }
            else
            {
                // Shift가 눌려있지 않은 일반 클릭: 이전의 다중 선택을 취소
                if (selectedGlobalIndices != null && selectedGlobalIndices.Count > 0) //
                {
                    selectedGlobalIndices.Clear(); //
                }
                committedRangeStart = -1; //
                committedRangeEnd = -1; //
                multiStartIndex = -1; //
                multiEndIndex = -1; //
                panelRange.Visible = false; //

                UpdateSelectedFileLabel(); //

                try { trbFrame_Scroll(trbFrame, EventArgs.Empty); } catch { } //
            }
        }
        private void UpdatePlaySelectLabel()
        {
            if (lblPlaySelect == null) return;

            lblPlaySelect.Text = "개수 : " + selectedGlobalIndices.Count;

            if (selectedGlobalIndices == null || selectedGlobalIndices.Count == 0)
            {
                lblPlaySelect.Text = "선택없음";
                return;
            }

            var sorted = selectedGlobalIndices.OrderBy(i => i).ToList();

            List<string> ranges = new List<string>();

            int start = sorted[0];
            int end = sorted[0];

            for (int i = 1; i < sorted.Count; i++)
            {
                if (sorted[i] == end + 1)
                {
                    end = sorted[i];
                }
                else
                {
                    ranges.Add(start == end
                        ? $"{start + 1}"
                        : $"{start + 1} ~ {end + 1}");

                    start = end = sorted[i];
                }
            }

            ranges.Add(start == end
                ? $"{start + 1}"
                : $"{start + 1} ~ {end + 1}");

            lblPlaySelect.Text = string.Join(", ", ranges);
        }
        private void UpdateListPage()
        {
            if (carImages == null || carImages.Count == 0) return;

            lstFiles.BeginUpdate();
            lstFiles.Items.Clear();

            int startIdx = currentPage * pageSize;
            int endIdx = Math.Min(startIdx + pageSize, carImages.Count);

            for (int i = startIdx; i < endIdx; i++)
            {
                var fileInfo = carImages[i];
                string fileName = Path.GetFileName(fileInfo.FilePath);
                string writeTime = fileInfo.WriteTime.ToString("yyyy-MM-dd HH:mm:ss");

                ListViewItem item = new ListViewItem((i + 1).ToString());
                item.SubItems.Add(fileName);
                item.SubItems.Add(writeTime);

                // 🌟 [핵심] 현재 그리는 파일이 이전에 Ctrl로 선택해 둔 글로벌 인덱스 목록에 있다면 다시 선택 상태로 만듭니다.
                if (selectedGlobalIndices.Contains(i))
                {
                    item.Selected = true;
                }

                lstFiles.Items.Add(item);
            }

            lstFiles.EndUpdate();

            // 라벨 업데이트
            lblCurFilePage.Text = $"{currentPage + 1} / {totalPages}";
            // visible range 라벨 갱신 (lstFiles에 표시된 항목 기준)
            try { if (lblFilenumber != null) lblFilenumber.Visible = true; } catch { }
            UpdateVisibleRangeLabel();
            // 트랙바 위의 범위 표시도 갱신
            UpdateRangeHighlight();
            // 선택된 파일 라벨 갱신
            UpdateSelectedFileLabel();
            SyncListViewMultiSelection();
        }

        private void btnPageUp_Click(object sender, EventArgs e)
        {
            if (currentPage > 0)
            {
                currentPage--;
                UpdateListPage();

                // 페이지가 바뀔 때, 해당 페이지의 첫 번째 이미지로 자동 이동 (선택 사항)
                currentImageIndex = currentPage * pageSize;
                trbFrame.Value = currentImageIndex;
                ShowImage(currentImageIndex);
            }
        }

        private void btnPageDown_Click(object sender, EventArgs e)
        {
            if (currentPage < totalPages - 1)
            {
                currentPage++;
                UpdateListPage();

                // 페이지가 바뀔 때, 해당 페이지의 첫 번째 이미지로 자동 이동 (선택 사항)
                currentImageIndex = currentPage * pageSize;
                trbFrame.Value = currentImageIndex;
                ShowImage(currentImageIndex);
            }
        }

        // [프레임 이동] 버튼 클릭 이벤트
        private void btnFrameMove_Click(object sender, EventArgs e)
        {
            // 텍스트박스에 입력된 값이 숫자인지 확인 (1부터 시작하는 프레임 번호 기준)
            if (int.TryParse(txbFrame.Text, out int targetFrame))
            {
                // 인덱스는 0부터 시작하므로 입력값에서 1을 뺍니다.
                int targetIndex = targetFrame - 1;

                // 범위 검사
                if (targetIndex >= 0 && targetIndex < carImages.Count)
                {
                    currentImageIndex = targetIndex;
                    trbFrame.Value = currentImageIndex;

                    // 입력한 프레임이 몇 번째 페이지에 속하는지 계산해서 페이지 이동
                    currentPage = targetIndex / pageSize;

                    UpdateListPage();  // 리스트박스 갱신
                    ShowImage(currentImageIndex); // 이미지 갱신
                }
                else
                {
                    MessageBox.Show($"1부터 {carImages.Count} 사이의 숫자를 입력해주세요.", "알림");
                }
            }
            else
            {
                MessageBox.Show("올바른 숫자를 입력해주세요.", "알림");
            }
        }
        /*private void lstFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 리스트뷰에서 아무것도 선택되지 않았다면 안전하게 리턴
            if (lstFiles.SelectedIndices.Count == 0) return;

            // 선택된 첫 번째 행의 인덱스를 가져옴
            int listBoxIndex = lstFiles.SelectedIndices[0];

            // 현재 페이지를 고려한 실제 이미지 위치 계산
            int actualIndex = (currentPage * pageSize) + listBoxIndex;

            if (actualIndex >= 0 && actualIndex < carImages.Count)
            {
                currentImageIndex = actualIndex;
                trbFrame.Value = currentImageIndex;
                ShowImage(currentImageIndex);
            }
        }*/

        private void txbFrame_KeyPress(object sender, KeyPressEventArgs e)
        {

        }


        private void txbFrame_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // 엔터 키 입력 시 발생하는 '띵~' 소리 방지
                e.SuppressKeyPress = true;

                // [프레임 이동] 버튼의 클릭 이벤트 강제 실행
                // (올려주신 코드를 보니 버튼 이름이 btnFrameMove이네요!)
                btnFrameMove_Click(this, new EventArgs());
            }
        }
        private void txbFileNum_KeyDown(object sender, KeyEventArgs e)
        {
            // 엔터 키를 눌렀을 때 작동
            if (e.KeyCode == Keys.Enter)
            {
                // 엔터 키 기본 경고음('띵~') 방지
                e.SuppressKeyPress = true;

                // 입력된 값이 숫자인지 확인 (사용자가 보는 페이지는 1페이지부터 시작)
                if (int.TryParse(txbFileNum.Text, out int targetPage))
                {
                    // 프로그램 내부 인덱스는 0부터 시작하므로 1을 뺍니다.
                    int pageIndex = targetPage - 1;

                    // 입력한 페이지가 유효한 범위 내에 있는지 검사 (0 ~ 전체페이지-1)
                    if (pageIndex >= 0 && pageIndex < totalPages)
                    {
                        // 현재 페이지 번호 변경
                        currentPage = pageIndex;

                        // 리스트박스와 페이지 라벨 갱신
                        UpdateListPage();

                        // 페이지가 바뀔 때, 해당 페이지의 첫 번째 이미지로 자동 이동 (선택 사항)
                        if (carImages != null && carImages.Count > 0)
                        {
                            currentImageIndex = currentPage * pageSize;
                            trbFrame.Value = currentImageIndex;
                            ShowImage(currentImageIndex);
                        }
                    }
                    else
                    {
                        MessageBox.Show($"1부터 {totalPages} 사이의 페이지 번호를 입력해주세요.", "알림");
                    }
                }
                else
                {
                    MessageBox.Show("올바른 숫자를 입력해주세요.", "알림");
                }
            }
        }
        private void lstFiles_MouseWheel(object sender, MouseEventArgs e)
        {
            if (carImages == null || carImages.Count == 0) return;
            if (isPlaying) StopAutoPlay(); // 자동 재생 중이면 정지

            // e.Delta가 0보다 크면 마우스 휠을 위로(Up) 굴린 것, 0보다 작으면 아래로(Down) 굴린 것
            if (e.Delta > 0)
            {
                // 휠을 위로 굴리면 -> 이전 페이지로 이동
                if (currentPage > 0)
                {
                    currentPage--;
                    currentImageIndex = currentPage * pageSize; // 새 페이지의 첫 번째 이미지로 인덱스 설정

                    UpdateListPage();
                    ShowImage(currentImageIndex);
                    trbFrame.Value = currentImageIndex; // 트랙바 동기화
                }
            }
            else if (e.Delta < 0)
            {
                // 휠을 아래로 굴리면 -> 다음 페이지로 이동
                if (currentPage < totalPages - 1)
                {
                    currentPage++;
                    currentImageIndex = currentPage * pageSize; // 새 페이지의 첫 번째 이미지로 인덱스 설정

                    UpdateListPage();
                    ShowImage(currentImageIndex);
                    trbFrame.Value = currentImageIndex; // 트랙바 동기화
                }
            }

            // 🌟 핵심: 리스트뷰 자체의 기본 세로 스크롤바가 멋대로 움직이는 것을 방지합니다.
            ((HandledMouseEventArgs)e).Handled = true;
        }
        private void btnFileDelete_Click(object sender, EventArgs e)
        {
            // 1. 데이터 바구니(carImages) 상태 확인 및 범위 검사
            if (carImages == null || carImages.Count == 0 || currentImageIndex < 0 || currentImageIndex >= carImages.Count)
            {
                MessageBox.Show("삭제할 프레임 이미지가 로드되지 않았거나 선택되지 않았습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 자동 재생 중이면 안전을 위해 잠시 멈춤
            if (isPlaying) StopAutoPlay();

            // 2. 무조건 현재 픽처박스/트랙바가 가리키는 단일 인덱스 정보 추출
            int globalIndex = currentImageIndex;
            string targetFilePath = carImages[globalIndex].FilePath;
            string fileName = Path.GetFileName(targetFilePath); // 예: "123_cam-image_array.jpg"
            string targetDirectory = Path.GetDirectoryName(targetFilePath); // 데이터가 담긴 Tub 폴더 경로

            // 3. 사용자 확인 다이얼로그
            DialogResult result = MessageBox.Show(
                $"현재 프레임({fileName})을 삭제하시겠습니까?\n이미지 디스크 삭제 및 카탈로그 레코드 수정이 동시에 진행됩니다.",
                "동키카 단일 프레임 영구 삭제",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes) return;

            // UI 이벤트 핸들러 일시 차단 (화면 꼬임 및 연쇄 반응 방지)
            this.lstFiles.SelectedIndexChanged -= new System.EventHandler(this.lstFiles_SelectedIndexChanged);
            this.lstFiles.ItemSelectionChanged -= new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.lstFiles_ItemSelectionChanged);
            this.txbFileNum.TextChanged -= new System.EventHandler(this.txbFileNum_TextChanged);

            try
            {
                // 4. 픽처박스 파일 잠금(Lock) 완벽 해제 (윈폼 파일 삭제 에러 방지)
                if (picCurFrame.Image != null)
                {
                    picCurFrame.Image.Dispose();
                    picCurFrame.Image = null;
                }
                GC.Collect();
                GC.WaitForPendingFinalizers();

                // 5. 🌟 카탈로그(catalog_*.catalog) 파일에서 해당 이미지 레코드만 삭제
                if (!string.IsNullOrEmpty(targetDirectory) && Directory.Exists(targetDirectory))
                {
                    string[] catalogFiles = Directory.GetFiles(targetDirectory, "catalog_*.catalog");

                    foreach (string catalogPath in catalogFiles)
                    {
                        if (File.Exists(catalogPath))
                        {
                            string[] lines = File.ReadAllLines(catalogPath, Encoding.UTF8);
                            List<string> updatedLines = new List<string>();
                            bool isModified = false;

                            foreach (string line in lines)
                            {
                                // 카탈로그 파일 내용 중 현재 이미지 파일명(fileName)이 포함된 줄인지 검사
                                if (line.Contains(fileName))
                                {
                                    isModified = true;
                                    continue; // 👈 일치하는 한 줄은 건너뛰어 카탈로그에서 제외시킴
                                }
                                updatedLines.Add(line);
                            }

                            // 수정사항이 있다면 카탈로그 파일 덮어쓰기 저장
                            if (isModified)
                            {
                                File.WriteAllLines(catalogPath, updatedLines, Encoding.UTF8);
                            }
                        }
                    }
                }

                // 6. 실제 하드디스크(디스크)에서 이미지 파일 제거
                if (File.Exists(targetFilePath))
                {
                    File.Delete(targetFilePath);
                }

                // 7. C# 프로그램 메모리 데이터 바구니(carImages)에서 제거
                carImages.RemoveAt(globalIndex);

                // 8. 페이지 수 및 트랙바 슬라이더 범위 재계산
                totalPages = (int)Math.Ceiling((double)carImages.Count / pageSize);
                trbFrame.Maximum = Math.Max(0, carImages.Count - 1);

                // 현재 인덱스가 데이터 범위를 넘어가지 않도록 보정
                if (currentImageIndex >= carImages.Count)
                {
                    currentImageIndex = Math.Max(0, carImages.Count - 1);
                }
                currentPage = (carImages.Count > 0) ? (currentImageIndex / pageSize) : 0;

                // 트랙바 위치 동기화
                trbFrame.Value = (carImages.Count > 0) ? currentImageIndex : 0;

                // 9. 화면 UI 및 리스트뷰 새로고침
                lstFiles.SelectedItems.Clear();
                UpdateListPage();

                if (carImages.Count > 0)
                {
                    ShowImage(currentImageIndex);
                    int localIndex = currentImageIndex % pageSize;
                    if (localIndex >= 0 && localIndex < lstFiles.Items.Count)
                    {
                        lstFiles.Items[localIndex].Focused = true;
                        lstFiles.Items[localIndex].EnsureVisible();
                        UpdatePageUI(localIndex);
                    }
                }
                else
                {
                    lblFrameNum.Text = "프레임 번호 : 0 / 0";
                    if (lblFilenumber != null) lblFilenumber.Text = "0 ~ 0";
                    MessageBox.Show("폴더 내의 모든 프레임 데이터가 삭제되었습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"단일 파일 삭제 및 카탈로그 동기화 중 오류 발생:\n{ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (carImages != null && carImages.Count > 0) ShowImage(currentImageIndex);
            }
            finally
            {
                // 10. 해제했던 리스트뷰/텍스트박스 이벤트 핸들러 안전하게 재연결
                this.lstFiles.SelectedIndexChanged += new System.EventHandler(this.lstFiles_SelectedIndexChanged);
                this.lstFiles.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.lstFiles_ItemSelectionChanged);
                this.txbFileNum.TextChanged += new System.EventHandler(this.txbFileNum_TextChanged);
            }
        }

        private void btnFileMultiDel_Click(object sender, EventArgs e)
        {
            // 0. 전체 데이터 바구니 상태 확인
            if (carImages == null || carImages.Count == 0)
            {
                MessageBox.Show("삭제할 데이터가 로드되지 않았습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 자동 재생 중이면 안전을 위해 잠시 멈춤
            if (isPlaying) StopAutoPlay();

            // 실제 삭제 처리를 할 '전역 인덱스 목록'을 담을 바구니
            List<int> indicesToDelete = new List<int>();

            // [분기 1] 우선적으로 Shift+트랙바로 설정한 전역 선택(selectedGlobalIndices)을 사용
            if (selectedGlobalIndices != null && selectedGlobalIndices.Count > 0)
            {
                indicesToDelete = selectedGlobalIndices.Distinct().ToList();
            }
            // [분기 2] selectedGlobalIndices가 비어있으면 리스트뷰 선택 기반으로 인덱스 수집
            else if (lstFiles.SelectedIndices.Count > 0)
            {
                foreach (int localIdx in lstFiles.SelectedIndices)
                {
                    int globalIdx = (currentPage * pageSize) + localIdx;
                    indicesToDelete.Add(globalIdx);
                }
            }

            // 두 가지 방법 모두 선택된 게 없다면 안내 멘트 후 리턴
            if (indicesToDelete.Count == 0)
            {
                MessageBox.Show("다중 삭제할 파일을 트랙바 영역이나 리스트뷰에서 선택해주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int deleteCount = indicesToDelete.Count;
            DialogResult result = MessageBox.Show($"선택한 {deleteCount}개의 프레임을 영구 삭제하시겠습니까?\n이미지 디스크 삭제 및 카탈로그 레코드 수정이 동시에 진행됩니다.",
                                                  "다중 삭제 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result != DialogResult.Yes) return;

            // UI 연쇄 반응 및 갱신 꼬임을 차단하기 위해 이벤트 일시 해제
            this.lstFiles.SelectedIndexChanged -= new System.EventHandler(this.lstFiles_SelectedIndexChanged);
            this.lstFiles.ItemSelectionChanged -= new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.lstFiles_ItemSelectionChanged);
            this.txbFileNum.TextChanged -= new System.EventHandler(this.txbFileNum_TextChanged);

            try
            {
                // 1. 현재 화면에 띄워진 이미지 프로세스 해제 (파일 삭제를 위한 Lock 방출)
                if (picCurFrame.Image != null)
                {
                    picCurFrame.Image.Dispose();
                    picCurFrame.Image = null;
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();

                // 2. 삭제할 파일들의 순수한 파일명 수집 및 디렉터리 식별
                List<string> targetFileNames = new List<string>();
                string targetDirectory = "";

                foreach (int idx in indicesToDelete)
                {
                    if (idx >= 0 && idx < carImages.Count)
                    {
                        targetFileNames.Add(Path.GetFileName(carImages[idx].FilePath));
                        if (string.IsNullOrEmpty(targetDirectory))
                        {
                            targetDirectory = Path.GetDirectoryName(carImages[idx].FilePath);
                        }
                    }
                }

                // 3. 🌟 [핵심 기능] 카탈로그(catalog_*.catalog) 파일 내용 동시 수정
                if (!string.IsNullOrEmpty(targetDirectory) && Directory.Exists(targetDirectory))
                {
                    string dataDirectory = Path.GetDirectoryName(targetDirectory);
                    string[] catalogFiles = Directory.GetFiles(dataDirectory, "catalog_*.catalog");

                    foreach (string catalogPath in catalogFiles)
                    {
                        if (File.Exists(catalogPath))
                        {
                            string[] lines = File.ReadAllLines(catalogPath, Encoding.UTF8);
                            List<string> updatedLines = new List<string>();
                            bool isModified = false;

                            foreach (string line in lines)
                            {
                                bool shouldDeleteLine = false;

                                // 카탈로그의 한 줄에 삭제 대상 파일명 중 하나라도 묻어있는지 체크
                                foreach (string fname in targetFileNames)
                                {
                                    if (line.Contains(fname))
                                    {
                                        shouldDeleteLine = true;
                                        isModified = true;
                                        break;
                                    }
                                }

                                // 삭제 대상이 아닌 라인만 유지
                                if (!shouldDeleteLine)
                                {
                                    updatedLines.Add(line);
                                }
                            }

                            // 내용 변화가 생겼다면 카탈로그 파일 오버라이트(저장)
                            if (isModified)
                            {
                                File.WriteAllLines(catalogPath, updatedLines, Encoding.UTF8);
                            }
                        }
                    }
                }

                // 4. 🌟 실제 이미지 파일 디스크 삭제 및 데이터 바구니(carImages) 제거
                // 내림차순(역순)으로 정렬하여 삭제해야 데이터 인덱스가 당겨져 발생하는 버그를 차단합니다.
                var toDeleteSorted = indicesToDelete.OrderByDescending(i => i).ToList();
                foreach (int globalIdx in toDeleteSorted)
                {
                    if (globalIdx >= 0 && globalIdx < carImages.Count)
                    {
                        string path = carImages[globalIdx].FilePath;
                        if (System.IO.File.Exists(path))
                        {
                            System.IO.File.Delete(path);
                        }
                        carImages.RemoveAt(globalIdx);
                    }
                }

                // 5. 다중 선택 바구니 청소
                if (selectedGlobalIndices != null)
                {
                    selectedGlobalIndices.Clear();
                }

                MessageBox.Show($"{deleteCount}개의 파일 및 카탈로그 레코드 삭제를 완료했습니다.", "완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"파일 다중 삭제 작업 중 오류가 발생했습니다:\n{ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // 6. UI 재건 및 갱신 프로세스 수행
                if (carImages.Count == 0)
                {
                    trbFrame.Maximum = 0;
                    trbFrame.Value = 0;
                    currentPage = 0;
                    totalPages = 0;
                    currentImageIndex = 0;

                    lstFiles.Items.Clear();
                    lblCurFilePage.Text = "0 / 0";
                    lblFrameNum.Text = "프레임 번호 : 0 / 0";
                    if (lblFilenumber != null) lblFilenumber.Text = "0 ~ 0";
                }
                else
                {
                    totalPages = (int)Math.Ceiling((double)carImages.Count / pageSize);

                    if (currentPage >= totalPages)
                    {
                        currentPage = totalPages - 1;
                    }

                    if (currentImageIndex >= carImages.Count)
                    {
                        currentImageIndex = carImages.Count - 1;
                    }

                    trbFrame.Maximum = Math.Max(0, carImages.Count - 1);
                    trbFrame.Value = Math.Min(trbFrame.Value, trbFrame.Maximum);

                    lstFiles.SelectedItems.Clear();
                    UpdateListPage();
                    ShowImage(currentImageIndex);
                }

                // 7. 해제했던 이벤트 핸들러 다시 복구 및 활성화
                this.lstFiles.SelectedIndexChanged += new System.EventHandler(this.lstFiles_SelectedIndexChanged);
                this.lstFiles.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.lstFiles_ItemSelectionChanged);
                this.txbFileNum.TextChanged += new System.EventHandler(this.txbFileNum_TextChanged);
            }
        }
        //기록창 확장 버튼 이전 코드
        /*private void btnExtend_Click(object sender, EventArgs e)
        {
            if (!isDebugExpanded)
            {
                // 닫혀있던 상태 -> 한 번에 확장 상태로 변경
                this.Width = expandedWidth;     // 1450 크기로 바로 변경
                btnExtend.Text = "기록창 닫기";  // 버튼 텍스트 변경
                isDebugExpanded = true;
            }
            else
            {
                // 열려있던 상태 -> 한 번에 기본 상태로 변경
                this.Width = normalWidth;       // 1147 크기로 바로 변경
                btnExtend.Text = "기록창 열기";
                isDebugExpanded = false;
            }
        }*/

        private void timerPlay_Tick(object sender, EventArgs e)
        {
            if (isPlayingSelectedRange && playQueue != null && playQueue.Count > 0)
            {
                // 재생 큐 기반 이동
                if (playQueuePos >= playQueue.Count)
                {
                    playQueuePos = 0;   // 처음부터 다시
                }

                int nextIndex = playQueue[playQueuePos++];
                currentImageIndex = nextIndex;
                trbFrame.Value = currentImageIndex;
            }
            else
            {
                // 다음으로 넘어갈 프레임 인덱스 계산
                int nextIndex = currentImageIndex + 1;

                // 만약 마지막 프레임에 도달했다면 자동으로 정지
                if (nextIndex >= carImages.Count)
                {
                    StopAutoPlay();
                    MessageBox.Show("마지막 프레임에 도달하여 자동 넘기기를 종료합니다.", "알림");
                    return;
                }

                // 인덱스를 1 증가시키고 트랙바(슬라이더) 위치 변경
                currentImageIndex = nextIndex;
                trbFrame.Value = currentImageIndex;
            }

            // 이미지 출력 및 페이지 리스트박스 동기화 처리
            ShowImage(currentImageIndex);

            // 🌟 2번 기능: 프레임 자동 넘기기 시 하단 리스트뷰 항목도 실시간 추적 선택
            if (carImages != null && carImages.Count > 0)
            {
                // 현재 글로벌 인덱스가 몇 번째 페이지에 속하는지 계산
                int targetPage = currentImageIndex / pageSize;

                // 자동 넘기기 도중 페이지 임계점을 넘어가면 페이지를 강제로 전환합니다.
                if (targetPage != currentPage)
                {
                    currentPage = targetPage;
                    UpdateListPage();
                }

                // 현재 페이지 안에서의 상대적인 리스트뷰 인덱스 계산
                int targetLocalIndex = currentImageIndex % pageSize;

                if (targetLocalIndex >= 0 && targetLocalIndex < lstFiles.Items.Count)
                {
                    // 기존 선택 해제 후 새로운 아이템 포커스 및 자동 스크롤 추적
                    lstFiles.SelectedItems.Clear();
                    lstFiles.Items[targetLocalIndex].Selected = true;
                    lstFiles.Items[targetLocalIndex].Focused = true;
                    lstFiles.Items[targetLocalIndex].EnsureVisible();
                }
            }
        }

        private void btnAutoPic_Click(object sender, EventArgs e)
        {
            // 1. 재생할 이미지 파일이 없는 경우 예외 처리
            if (carImages == null || carImages.Count == 0)
            {
                MessageBox.Show("이미지 폴더를 먼저 열어주세요.", "알림");
                return;
            }

            if (!isPlaying)
            {
                // 2. txbFPS 텍스트박스에서 사용자가 입력한 FPS 값 읽어오기
                // 숫자가 아니거나 0 이하를 입력하면 기본값인 10 FPS로 강제 설정
                if (!int.TryParse(txbFPS.Text, out int fps) || fps <= 0)
                {
                    fps = 10;
                    txbFPS.Text = "10";
                }

                // 3. FPS를 타이머 작동 주기(밀리초, ms)로 계산하여 대입 (Interval = 1000 / FPS)
                // 예: 20 FPS를 입력하면 1000 / 20 = 50ms 마다 타이머가 신호를 주어 화면이 넘어갑니다.
                timerPlay.Interval = 1000 / fps;

                // 4. 타이머 가동 및 버튼 상태 변경
                timerPlay.Start();
                btnAutoPic.Text = "정지";
                btnAutoPic.BackColor = Color.Tomato; // 작동 중임을 알리기 위해 버튼을 붉은색으로 변경
                isPlaying = true;
            }
            else
            {
                // 5. 이미 재생 중일 때 버튼을 누르면 정지
                StopAutoPlay();
            }
        }

        private void lstFiles_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (carImages == null || carImages.Count == 0) return;
            if (isPlaying) StopAutoPlay();

            isAscending = !isAscending;

            if (e.Column == 1)
            {
                // 1열: 파일명 정렬 (🌟 1, 2, 3, 10, 100 숫자 순서대로 정렬하도록 수정)
                if (isAscending)
                {
                    carImages = carImages.OrderBy(f =>
                    {
                        string fileName = System.IO.Path.GetFileNameWithoutExtension(f.FilePath);
                        // 혹시 파일명에 문자(cam-image_ 등)가 섞여 있을 것을 대비해 숫자만 추출합니다.
                        string numStr = System.Text.RegularExpressions.Regex.Replace(fileName, @"\D", "");
                        // 숫자로 변환 성공하면 정수 크기로 비교, 실패하면 안전하게 0으로 처리
                        return int.TryParse(numStr, out int num) ? num : 0;
                    }).ToList();
                }
                else
                {
                    carImages = carImages.OrderByDescending(f =>
                    {
                        string fileName = System.IO.Path.GetFileNameWithoutExtension(f.FilePath);
                        string numStr = System.Text.RegularExpressions.Regex.Replace(fileName, @"\D", "");
                        return int.TryParse(numStr, out int num) ? num : 0;
                    }).ToList();
                }
            }
            else if (e.Column == 2)
            {
                // 2열: 날짜 정렬 (기존 코드 유지)
                if (isAscending)
                    carImages = carImages.OrderBy(f => f.WriteTime).ThenBy(f => System.IO.Path.GetFileName(f.FilePath)).ToList();
                else
                    carImages = carImages.OrderByDescending(f => f.WriteTime).ThenByDescending(f => System.IO.Path.GetFileName(f.FilePath)).ToList();
            }
            else
            {
                return;
            }

            // 정렬 후 인덱스 초기화 및 화면 갱신
            currentImageIndex = 0;
            currentPage = 0;
            trbFrame.Value = 0;

            UpdateListPage();
            ShowImage(currentImageIndex);
        }

        private void txbFileNum_TextChanged(object sender, EventArgs e)
        {

        }

        // 1. 리스트뷰에서 항목을 마우스나 방향키로 클릭할 때 실행되는 이벤트
        private void lstFiles_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            // 현재 아이템의 전체 파일 기준 실제(글로벌) 인덱스 계산
            int globalIndex = (currentPage * pageSize) + e.ItemIndex;

            if (e.IsSelected)
            {
                // 선택되었다면 바구니에 추가
                selectedGlobalIndices.Add(globalIndex);
            }
            else
            {
                // 선택 해제되었다면 바구니에서 제거
                selectedGlobalIndices.Remove(globalIndex);
            }
            // 선택 라벨 갱신
            UpdateSelectedFileLabel();
            UpdateBtnMpVisibility();
        }

        // 2. 디자이너 매핑 안정성을 위한 서브 이벤트
        private void lstFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstFiles.SelectedIndices.Count > 0)
            {
                int localIndex = lstFiles.SelectedIndices[0];
                int actualIndex = (currentPage * pageSize) + localIndex;

                if (carImages != null && actualIndex >= 0 && actualIndex < carImages.Count)
                {
                    currentImageIndex = actualIndex;
                    trbFrame.Value = currentImageIndex;
                    ShowImage(currentImageIndex);

                    // 🌟 UI 전체 갱신 호출
                    UpdatePageUI(localIndex);
                }
            }
            UpdateBtnMpVisibility();
        }

        // 3. 모든 UI 요소(페이지 정보, 상단 프레임 번호)를 정확하게 동기화하는 핵심 메서드
        // 모든 UI 요소(상단 프레임 번호 라벨, 우측 페이지 텍스트박스, 좌측 페이지 라벨)를 동기화하는 메서드
        private void UpdatePageUI(int pageListItemIndex)
        {
            if (carImages == null || carImages.Count == 0) return;

            // 1. 전체 이미지 기준의 실제 현재 프레임 번호 계산 (1부터 시작하므로 +1)
            int globalFileNumber = (currentPage * pageSize) + pageListItemIndex + 1;

            // 2. 전체 로드된 총 이미지(프레임) 개수
            int totalFramesCount = carImages.Count;

            // 🌟 [상단 라벨] 원하시는 포맷 "프레임 번호 : 현재 프레임 / 전체 프레임"으로 완벽 매핑
            lblFrameNum.Text = $"프레임 번호 : {globalFileNumber} / {totalFramesCount}";

            // 3. [우측 텍스트박스] 유저가 페이지 단위로 건너뛸 수 있도록 현재 페이지 번호 표시
            int displayPage = currentPage + 1;
            txbFileNum.Text = displayPage.ToString();

            // 4. [좌측 라벨] 현재 페이지 / 전체 페이지 형식 유지
            lblCurFilePage.Text = $"{displayPage} / {totalPages}";
        }

        private void picCurFrame_Click(object sender, EventArgs e)
        {

        }
        private void btnStartCollection_Click(object sender, EventArgs e)
        {
            // =================================================================
            // 1 구역: 유저가 선택한 시뮬레이터(.exe) 경로 유효성 검사 및 확보
            // =================================================================
            if (string.IsNullOrEmpty(selectedSimFilePath) || !System.IO.File.Exists(selectedSimFilePath))
            {
                MessageBox.Show("동키카 시뮬레이터 파일(donkey_sim.exe)이 아직 지정되지 않았거나 파일을 찾을 수 없습니다.\n시뮬레이터 실행 파일을 먼저 선택해 주세요.",
                                "시뮬레이터 지정 필요", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Title = "동키카 시뮬레이터 실행 파일(donkey_sim.exe)을 선택해 주세요";
                    openFileDialog.Filter = "실행 파일 (*.exe)|*.exe|모든 파일 (*.*)|*.*";
                    openFileDialog.InitialDirectory = @"C:\";

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        selectedSimFilePath = openFileDialog.FileName;

                        if (btnSelectSim != null)
                        {
                            btnSelectSim.Text = "시뮬레이터 선택완료";
                            btnSelectSim.ForeColor = Color.Blue;
                        }
                    }
                    else
                    {
                        MessageBox.Show("시뮬레이터 프로그램이 선택되지 않아 구동을 취소합니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
            }

            // =================================================================
            // 2 구역: 🌟 리눅스(WSL) 동키카 서버 구동 세팅 (cmd 우회법으로 에러 원천 차단)
            // =================================================================
            ProcessStartInfo wslInfo = new ProcessStartInfo();

            // 🌟 wsl.exe를 직접 부르는 대신, 윈도우의 cmd.exe를 실행하여 우회 구동합니다.
            // 이렇게 하면 리눅스 창이 100% 확률로 무조건 시각적으로 열립니다.
            wslInfo.FileName = "cmd.exe";

            // 고정 데이터 보존
            string linuxUser = txtLinuxUser.Text.Trim();

            if (string.IsNullOrWhiteSpace(linuxUser))
            {
                MessageBox.Show(
                    "우분투 사용자명을 입력해주세요.",
                    "알림",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            string condaPath =
                $"/home/{linuxUser}/miniconda3";

            string linuxCommand =
                $"cd /home/{linuxUser}/mysim && " +
                $"source {condaPath}/bin/activate e2e_env && " +
                $"python3 manage.py drive";

            // 🌟 cmd창을 열어(/c start) 제목이 "Donkeycar Server"인 새 터미널을 독립시키고 WSL 명령을 하달합니다.
            // 마지막에 ; exec bash를 주어 파이썬 에러가 나도 창이 안 닫히고 멈춰있게 만듭니다.
            wslInfo.Arguments = $"/c start \"Donkeycar Server\" wsl.exe -d Ubuntu-22.04 -u {linuxUser} bash -c \"{linuxCommand}; exec bash\"";

            wslInfo.UseShellExecute = true;
            wslInfo.CreateNoWindow = false;

            // =================================================================
            // 3 구역: 윈도우 동키카 시뮬레이터 실행 세팅 (유저 선택 경로 반영)
            // =================================================================
            ProcessStartInfo simInfo = new ProcessStartInfo();
            simInfo.FileName = selectedSimFilePath;
            simInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(selectedSimFilePath);
            simInfo.UseShellExecute = true;

            // =================================================================
            // 4 구역: 프로세스 순차 기동 및 웹 연동
            // =================================================================
            try
            {

                // 1. 유저가 선택한 유니티 시뮬레이터 프로그램 실행
                Process simProcess = new Process();
                simProcess.StartInfo = simInfo;
                simProcess.Start();

                // 2. 시뮬레이터와 서버가 웹소켓 결합을 완료할 수 있도록 2초 대기
                System.Threading.Thread.Sleep(2000);

                // 3. 기본 브라우저를 통해 최종 제어 웹 사이트 오픈




            }
            catch (Exception ex)
            {
                MessageBox.Show($"시뮬레이터 구동 중 오류가 발생했습니다:\n{ex.Message}", "오류",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnStartLearning_Click(object sender, EventArgs e)
        {
            // 1. 🔍 예외 처리: 사용자가 아직 파일을 한 번도 안 열었다면 경고 후 리턴
            if (string.IsNullOrEmpty(currentSelectedFolderPath))
            {
                MessageBox.Show("먼저 [파일 열기] 버튼을 통해 정제할 데이터 폴더(tub)를 선택해 주세요.",
                                "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. 🌟 윈도우 경로를 WSL 리눅스 경로 형식으로 자동 변환합니다.
            // 예: "C:\donkeycar\mycar\data\tub_1" ➡️ "/mnt/c/donkeycar/mycar/data/tub_1"
            string winPath = currentSelectedFolderPath;
            string linuxTubPath = winPath.Replace(@"\", "/")
                                         .Replace("C:", "/mnt/c")
                                         .Replace("c:", "/mnt/c");
            // 혹시 D드라이브도 쓰신다면 .Replace("D:", "/mnt/d") 등 추가 가능

            // 3. 본인의 우분투 환경 설정값
            string linuxUser = "username";       // 👈 본인의 우분투 사용자 이름
            string mycarFolder = "mycar";       // 👈 train.py가 들어있는 동키카 프로젝트 폴더명
            string condaPath = $"/home/{linuxUser}/anaconda3";
            string linuxPath = $"/home/{linuxUser}/{mycarFolder}";
            string modelName = "mypilot.h5";    // 생성될 AI 모델 파일 이름

            // 4. 🌟 변환된 리눅스 튜브 경로(linuxTubPath)를 --tub= 뒤에 그대로 주입합니다!
            string linuxCommand = $"cd {linuxPath} && source {condaPath}/bin/activate donkeycar && python3 train.py --tub={linuxTubPath} --model=./models/{modelName}";

            // 5. 프로세스 실행 설정 (우분투 터미널 창을 직접 띄움)
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "wsl.exe";
            startInfo.Arguments = $"-d Ubuntu bash -c \"{linuxCommand}; exec bash\"";
            startInfo.UseShellExecute = true;
            startInfo.CreateNoWindow = false;

            Process trainProcess = new Process();
            trainProcess.StartInfo = startInfo;

            try
            {
                DialogResult confirm = MessageBox.Show(
                    $"현재 열려 있는 폴더 데이터로 학습을 시작하시겠습니까?\n\n" +
                    $"윈도우 경로: {winPath}\n" +
                    $"리눅스 경로: {linuxTubPath}\n\n" +
                    "※ 확인을 누르면 AI 학습을 진행하는 우분투 창이 새로 열립니다.",
                    "학습 시작", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (confirm == DialogResult.Yes)
                {
                    trainProcess.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"AI 학습 프로세스 구동 중 오류 발생:\n{ex.Message}", "오류",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void lstFiles_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                btnFileDelete_Click(sender, e);
                return;
            }

            if (lstFiles.SelectedIndices.Count > 0)
            {
                int minSelectedIndex = lstFiles.SelectedIndices.Cast<int>().Min();
                int maxSelectedIndex = lstFiles.SelectedIndices.Cast<int>().Max();

                // [위쪽 방향키] 선택 영역의 맨 윗줄이 0번일 때 상단 페이지로 점프
                if (e.KeyCode == Keys.Up && minSelectedIndex == 0)
                {
                    if (currentPage > 0)
                    {
                        // 🌟 [핵심] 윈폼 리스트뷰가 이전 포커스 위치를 기억해서 
                        // 원치 않는 다중 선택을 해버리는 버그를 막기 위해 포커스 링크를 끊어줍니다.
                        lstFiles.FocusedItem = null;

                        currentPage--;
                        UpdateListPage();

                        // Ctrl을 누른 채 이전 페이지로 넘어갔다면, 이전 페이지의 '맨 하단' 아이템을 포커싱합니다.
                        if (lstFiles.Items.Count > 0)
                        {
                            int lastIdx = lstFiles.Items.Count - 1;

                            // 바구니 동기화 및 강제 선택
                            selectedGlobalIndices.Add((currentPage * pageSize) + lastIdx);

                            lstFiles.Items[lastIdx].Selected = true;
                            lstFiles.Items[lastIdx].Focused = true;
                            lstFiles.Items[lastIdx].EnsureVisible();
                        }

                        // 키 입력 처리를 완료했음을 선언하여 윈폼 고유의 오작동 방지
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                    }
                }
                // [아래쪽 방향키] 선택 영역의 맨 아랫줄이 리스트뷰 끝일 때 하단 페이지로 점프
                else if (e.KeyCode == Keys.Down && maxSelectedIndex == lstFiles.Items.Count - 1)
                {
                    if (currentPage < totalPages - 1)
                    {
                        // 🌟 [핵심] 다음 페이지로 넘어가기 전 현재 리스트뷰의 포커스 앵커를 리셋합니다.
                        // 이 처리를 해주면 Ctrl을 누르고 있어도 엉뚱한 파일들이 줄줄이 선택되지 않습니다.
                        lstFiles.FocusedItem = null;

                        currentPage++;
                        UpdateListPage();

                        // Ctrl을 누른 채 다음 페이지로 넘어갔다면, 다음 페이지의 '맨 상단(0번)' 아이템을 포커싱합니다.
                        if (lstFiles.Items.Count > 0)
                        {
                            selectedGlobalIndices.Add(currentPage * pageSize); // 바구니 동기화

                            lstFiles.Items[0].Selected = true;
                            lstFiles.Items[0].Focused = true;
                            lstFiles.Items[0].EnsureVisible();
                        }

                        // 키 입력 처리를 완료했음을 선언하여 윈폼 고유의 오작동 방지
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                    }
                }
            }
        }

        private void btnStartLearning_Click_1(object sender, EventArgs e)
        {
            // 🌟 모호한 참조(CS0104) 에러를 방지하기 위해 명확하게 System.Windows.Forms.Button으로 지정합니다.
            System.Windows.Forms.Button learningButton = sender as System.Windows.Forms.Button;

            // =================================================================
            // 🛑 [상태 1] 이미 학습이 진행 중일 때 -> "학습 중지" 로직 작동
            // =================================================================
            if (trainProcess != null && !trainProcess.HasExited)
            {
                DialogResult stopConfirm = MessageBox.Show("현재 진행 중인 AI 학습을 강제로 중지하시겠습니까?",
                                                           "학습 중지 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (stopConfirm == DialogResult.Yes)
                {

                    rtbLearningLog.Visible = false;
                    picCurFrame.Visible = true;

                    try
                    {
                        // WSL 백그라운드 프로세스 강제 종료
                        trainProcess.Kill();
                        trainProcess.Dispose();
                        trainProcess = null;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"프로세스 종료 중 에러가 발생했습니다: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    // UI 상태를 즉시 원상복구
                    if (learningButton != null)
                    {
                        learningButton.Text = "학습 시작";
                        learningButton.BackColor = SystemColors.Control; // 기본 버튼 색상
                    }

                    // 프로그레스 바가 디자인에 존재할 때만 안전하게 리셋
                    if (pbLearningProgress != null)
                    {
                        pbLearningProgress.Value = 0;
                    }

                    MessageBox.Show("AI 학습이 사용자에 의해 중지되었습니다.", "중지 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return; // 중지 처리를 완료했으므로 함수를 종료합니다.
            }

            // =================================================================
            // 🚀 [상태 2] 학습이 실행 중이 아닐 때 -> "학습 시작" 로직 작동
            // =================================================================

            // 1. 🔍 예외 처리: 사용자가 아직 파일을 한 번도 안 열었다면 경고 후 리턴
            if (string.IsNullOrEmpty(currentSelectedFolderPath))
            {
                MessageBox.Show("먼저 [파일 열기] 버튼을 통해 정제할 데이터 폴더(tub)를 선택해 주세요.",
                                "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. 기존 코드에 있던 사용자 설정값을 안전하게 그대로 유지합니다.
            string linuxUser = "leetjdgus";       // 👈 기존 고정값 유지
            string mycarFolder = "mysim";         // 👈 기존 고정값 유지
            string condaPath = $"/home/{linuxUser}/miniconda3";
            string linuxPath = $"/home/{linuxUser}/{mycarFolder}";
            string modelName = "mypilot.h5";
            string linuxTubPath = $"/home/{linuxUser}/{mycarFolder}/data";

            // 3. 파이썬 출력이 실시간으로 C#으로 전달되도록 python3 뒤에 '-u' 옵션을 추가하여 조립합니다.
            string linuxCommand =
                $"cd {linuxPath} && " +
                $"source {condaPath}/bin/activate e2e_env && " +
                $"python3 -u train.py " +
                $"--tubs {linuxTubPath} " +
                $" --model ./models/{modelName}";

            // 4. 📊 프로그레스 바 게이지 리셋
            if (pbLearningProgress != null)
            {
                pbLearningProgress.Value = 0;
            }

            // 5. 프로세스 실행 설정 (인터페이스 뒤에서 조용히 구동하도록 설정)
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "wsl.exe";
            startInfo.Arguments = $"-d Ubuntu-22.04 bash -c \"{linuxCommand}\"";

            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.CreateNoWindow = true;

            // 전역 변수로 선언해 둔 인스턴스에 할당
            trainProcess = new Process();
            trainProcess.StartInfo = startInfo;
            trainProcess.EnableRaisingEvents = true;

            // 6. 🌟 [비동기 데이터 수신 이벤트] 리눅스가 글자를 출력할 때마다 실시간 백그라운드 파싱
            trainProcess.OutputDataReceived += (s, args) =>
            {
                if (args.Data == null) return;

                this.BeginInvoke(new Action(() =>
                {
                    string log = args.Data;

                    // Epoch 표시
                    if (log.Contains("Epoch") && log.Contains("/"))
                    {
                        lblEpoch.Text = log;
                    }

                    if (log.Contains("val_loss improved"))
                    {
                        try
                        {
                            string[] parts = log.Split(" to ");

                            if (parts.Length > 1)
                            {
                                string loss =
                                    parts[1].Split(',')[0].Trim();

                                lblLoss.Text =
                                    $"현재 Loss : {loss}";
                            }
                        }
                        catch
                        {
                        }
                    }

                    if (log.Contains("Epoch") && log.Contains("/"))
                    {
                        lblEpoch.Text =
                            $"현재 Epoch : {log.Replace("Epoch ", "")}";
                    }

                    if (log.Contains("Epoch"))
                    {
                        rtbLearningLog.AppendText(log + Environment.NewLine);

                        rtbLearningLog.SelectionStart =
                            rtbLearningLog.Text.Length;

                        rtbLearningLog.ScrollToCaret();
                    }
                }));

                // 진행률 계산 부분 그대로
                if (pbLearningProgress != null)
                {
                    if (args.Data.Contains("Epoch") && args.Data.Contains("/"))
                    {
                        try
                        {
                            string cleanData = args.Data.Replace("Epoch", "").Trim();
                            string[] epochParts = cleanData.Split(' ')[0].Split('/');

                            if (epochParts.Length == 2)
                            {
                                int currentEpoch = int.Parse(epochParts[0]);
                                int totalEpochs = int.Parse(epochParts[1]);

                                int percent =
                                    (int)(((double)currentEpoch / totalEpochs) * 100);

                                if (percent >= 0 && percent <= 100)
                                {
                                    this.BeginInvoke(new Action(() =>
                                    {
                                        pbLearningProgress.Value = percent;
                                    }));
                                }
                            }
                        }
                        catch { }
                    }
                }


                if (args.Data != null && pbLearningProgress != null)
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        // 📊 "Epoch 5/20" 문자열을 감지하여 퍼센트 계산 후 게이지바 제어
                        if (args.Data.Contains("Epoch") && args.Data.Contains("/"))
                        {
                            try
                            {
                                string cleanData = args.Data.Replace("Epoch", "").Trim();
                                string[] epochParts = cleanData.Split(' ')[0].Split('/');

                                if (epochParts.Length == 2)
                                {
                                    int currentEpoch = int.Parse(epochParts[0]);
                                    int totalEpochs = int.Parse(epochParts[1]);

                                    int percent = (int)(((double)currentEpoch / totalEpochs) * 100);

                                    if (percent >= 0 && percent <= 100)
                                    {
                                        pbLearningProgress.Value = percent;
                                    }
                                }
                            }
                            catch
                            {
                                // 파싱 오류 시 프로그램 튕김 방지
                            }
                        }
                    }));
                }
            };

            // trainProcess.ErrorDataReceived += (s, args) =>


            // 7. 🌟 [학습 최종 완료 이벤트] AI 학습이 정상적으로 끝났을 때 실행
            trainProcess.Exited += (s, args) =>
            {
                this.BeginInvoke(new Action(() =>
                {

                    rtbLearningLog.Visible = false;
                    picCurFrame.Visible = true;

                    if (pbLearningProgress != null)
                    {
                        pbLearningProgress.Value = 100;
                    }

                    if (learningButton != null)
                    {
                        learningButton.Text = "학습 시작";
                        learningButton.BackColor = SystemColors.Control;
                    }

                    MessageBox.Show("동키카 AI 모델 학습이 성공적으로 완료되었습니다!", "학습 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    if (trainProcess != null)
                    {
                        trainProcess.Dispose();
                        trainProcess = null;
                    }
                }));
            };

            try
            {
                // 8. 학습 시작 최종 확인 다이얼로그
                DialogResult confirm = MessageBox.Show($"위 설정으로 백그라운드 AI 학습을 시작하시겠습니까?", "학습 시작 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (confirm == DialogResult.Yes)
                {
                    // 백그라운드 리눅스 프로세스 기동
                    trainProcess.Start();
                    rtbLearningLog.Visible = true;
                    rtbLearningLog.Clear();
                    rtbLearningLog.AppendText("학습 시작됨\r\n");

                    // 비동기 텍스트 가로채기 스트림 동작 시작
                    trainProcess.BeginOutputReadLine();
                    trainProcess.BeginErrorReadLine();

                    // [UI 전환] 학습이 성공적으로 시작되었으므로 버튼을 중지 모드로 바꿉니다.
                    if (learningButton != null)
                    {
                        learningButton.Text = "학습 중지";
                        learningButton.BackColor = Color.Tomato; // 시각적 구분을 위한 토마토 색상 변경
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"학습 프로세스 실행 중 에러가 발생했습니다:\n{ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (learningButton != null)
                {
                    learningButton.Text = "학습 시작";
                    learningButton.BackColor = SystemColors.Control;
                }
                trainProcess = null;
            }
        }

        private void dgvDebug_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dgvDebug_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void btnSelectSim_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "동키카 시뮬레이터 실행 파일(donkey_sim.exe)을 선택해 주세요";
                openFileDialog.Filter = "실행 파일 (*.exe)|*.exe|모든 파일 (*.*)|*.*";
                openFileDialog.InitialDirectory = @"C:\"; // 기본 시작 위치

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // 선택한 파일의 절대 경로를 전역 변수에 저장
                    selectedSimFilePath = openFileDialog.FileName;

                    // 유저가 직관적으로 확인하도록 버튼 텍스트를 파일명으로 변경 (라벨 대용)
                    btnSelectSim.Text = "시뮬레이터 선택완료";
                    btnSelectSim.ForeColor = Color.Blue; // 선택 완료 표시

                    MessageBox.Show($"시뮬레이터 경로가 지정되었습니다:\n{selectedSimFilePath}", "경로 지정 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            // 사용자에게 진짜 초기화할 것인지 확인 (선택 사항)
            DialogResult result = MessageBox.Show("앱을 초기화하고 다시 시작하시겠습니까?", "알림", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // 애플리케이션 재시작
                Application.Restart();

                // 현재 프로세스 완전히 종료 (안전을 위해 추가)
                Environment.Exit(0);
            }
        }

        private void panelRange_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnmp_Click(object sender, EventArgs e)
        {
            // btnmp: 현재 선택된 전역 인덱스들만 재생
            if (carImages == null || carImages.Count == 0) return;

            // 재생 중이면 중지
            if (isPlaying && isPlayingSelectedRange)
            {
                StopAutoPlay();
                return;
            }

            // 선택된 인덱스 수집: 우선적으로 트랙바 Shift 선택 바구니 사용
            List<int> indices = new List<int>();
            if (selectedGlobalIndices != null && selectedGlobalIndices.Count > 0)
            {
                indices = selectedGlobalIndices.OrderBy(i => i).ToList();
            }
            else if (lstFiles.SelectedIndices.Count > 0)
            {
                foreach (int local in lstFiles.SelectedIndices)
                {
                    int global = currentPage * pageSize + local;
                    indices.Add(global);
                }
                indices = indices.OrderBy(i => i).ToList();
            }

            if (indices.Count == 0)
            {
                MessageBox.Show("재생할 프레임을 선택하세요 (Shift+트랙바 또는 리스트뷰).", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 재생 큐 생성
            playQueue = indices;
            // 즉시 첫 프레임을 보여주고 다음 tick에서 두번째 요소부터 재생되게 설정
            playQueuePos = 0;
            currentImageIndex = playQueue[0];
            if (currentImageIndex >= 0 && currentImageIndex < carImages.Count)
            {
                trbFrame.Value = currentImageIndex;
                ShowImage(currentImageIndex);
            }
            playQueuePos = 1;
            isPlayingSelectedRange = true;

            // 타이머 설정(FPS 적용)
            if (!int.TryParse(txbFPS.Text, out int fps) || fps <= 0)
            {
                fps = 10;
                txbFPS.Text = "10";
            }
            timerPlay.Interval = 1000 / fps;
            timerPlay.Start();
            btnmp.Text = "정지";
            btnmp.BackColor = Color.Tomato;
            isPlaying = true;
        }
        /*private void UpdateSteeringIndicator(float steeringValue)
        {
            // 1. 최신 조향값 업데이트
            currentSteering = steeringValue;

            // 2. 픽처박스에게 "화면이 바뀌었으니 Paint 이벤트를 다시 실행해라"고 명령 (강제 새로고침)
            picCurFrame.Invalidate();
        }*/

        private void picCurFrame_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // 선을 부드럽게 그리기 위한 안티앨리어싱 설정
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // 1. 조향계의 중심축(회전 중심) 설정 (픽처박스 하단 중앙)
            int centerX = picCurFrame.Width / 2;
            int centerY = picCurFrame.Height - 20; // 하단에서 20픽셀 위

            // 두 바늘의 길이를 살짝 다르게 하면 겹쳤을 때도 구분이 쉽습니다.
            int currentLineLength = 60; // 현재 조향 바늘 길이
            int correctLineLength = 75; // 올바른 조향 바늘 길이 (더 길게 처리)

            // 최대 좌우 회전 각도 (예: 45도)
            float maxRotationAngle = 45.0f;

            // ------------------------------------------------------------
            // 2. [그리기 1] 올바른 조향값 (바탕/기준선 역할을 하도록 먼저 그리기)
            // ------------------------------------------------------------
            float correctAngle = correctSteering * maxRotationAngle;

            // 현재 그래픽 상태 저장 (회전 후 원상복구하기 위함)
            System.Drawing.Drawing2D.GraphicsState basicState = g.Save();

            // 그래픽 좌표계를 중심축으로 이동 후 각도만큼 회전
            g.TranslateTransform(centerX, centerY);
            g.RotateTransform(correctAngle);

            // 두께 5의 주황색(Orange) 선으로 올바른 조향값 그리기
            using (Pen correctPen = new Pen(Color.Orange, 5))
            {
                // 끝부분을 둥글게 마감처리
                correctPen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                correctPen.EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor; // 화살표 모양 종점

                // (0,0)에서 출발하여 직선 위쪽 방향인 (0, -correctLineLength)까지 선을 긋습니다.
                g.DrawLine(correctPen, 0, 0, 0, -correctLineLength);
            }

            // 그래픽 상태를 처음 상태로 리셋
            g.Restore(basicState);

            // ------------------------------------------------------------
            // 3. [그리기 2] 현재 조향값 (그 위에 덮어쓰기)
            // ------------------------------------------------------------
            float currentAngle = currentSteering * maxRotationAngle;

            // 다시 그래픽 좌표계를 중심축으로 이동 후 각도만큼 회전
            g.TranslateTransform(centerX, centerY);
            g.RotateTransform(currentAngle);

            // 두께 4의 형광 녹색(LimeGreen) 선으로 현재 조향값 그리기
            using (Pen steeringPen = new Pen(Color.LimeGreen, 4))
            {
                steeringPen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                steeringPen.EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;

                // (0,0)에서 출발하여 직선 위쪽 방향인 (0, -lineLength)까지 선을 긋습니다.
                g.DrawLine(steeringPen, 0, 0, 0, -currentLineLength);
            }

            // 중심축에 작은 원을 그려서 앵커 포인트를 깔끔하게 마감
            g.ResetTransform();
            using (Brush centerBrush = new SolidBrush(Color.White))
            {
                g.FillEllipse(centerBrush, centerX - 5, centerY - 5, 10, 10);
            }
        }
        /// <summary>
        /// 조향값 및 쓰로틀 기반 오토 필터링 기능 수행
        /// </summary>
        /// <summary>
        /// 조향값 및 쓰로틀 기반 오토 필터링 기능 수행
        /// </summary>
        private void RunAutoFiltering()
        {
            // 1. 전체 데이터 바구니 상태 확인
            if (carImages == null || carImages.Count == 0)
            {
                MessageBox.Show("오토 필터링을 진행할 데이터가 로드되지 않았습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 대량 변경을 위한 리스트뷰 화면 업데이트 일시 중지
            lstFiles.BeginUpdate();

            try
            {
                // 2. 기존 다중 선택 바구니 및 리스트뷰 선택 초기화
                if (selectedGlobalIndices == null)
                {
                    selectedGlobalIndices = new HashSet<int>();
                }
                selectedGlobalIndices.Clear();
                lstFiles.SelectedItems.Clear();

                // 3. 필터링 감지용 임계값 세팅 (상황에 맞게 수치를 미세 조절하셔도 됩니다)
                float maxDeltaAngle = 0.6f;     // 프레임 간 허용할 최대 조향 변화량 (노이즈 판별)
                float minThrottle = 0.05f;      // 차량이 주행 중이라고 판단할 최소 쓰로틀
                float extremeAngle = 0.8f;      // 정지 상태에서 불량으로 판정할 조향각 임계값

                int anomalyCount = 0;

                // 4. 전역 데이터(carImages) 전체 순회하며 이상치 탐색
                for (int i = 0; i < carImages.Count; i++)
                {
                    var currentFrame = carImages[i];
                    bool isAnomaly = false;

                    // [조건 A] 차량이 거의 멈춰있는데(쓰로틀 로우) 핸들만 비정상적으로 크게 꺾인 경우
                    if (Math.Abs(currentFrame.Throttle) < minThrottle && Math.Abs(currentFrame.Angle) > extremeAngle)
                    {
                        isAnomaly = true;
                    }

                    // [조건 B] 직전 프레임과 비교해 조향각이 순간적으로 순간이동하듯 확 튄 경우
                    if (i > 0)
                    {
                        var prevFrame = carImages[i - 1];
                        float delta = Math.Abs(currentFrame.Angle - prevFrame.Angle);
                        if (delta > maxDeltaAngle)
                        {
                            isAnomaly = true;
                        }
                    }

                    // 5. 이상 데이터로 판별 시 바구니에 전역 인덱스 추가
                    if (isAnomaly)
                    {
                        selectedGlobalIndices.Add(i);
                        anomalyCount++;

                        // [UI 연동] 감지된 불량 프레임이 '현재 보고 있는 페이지' 범위 안의 파일이라면 화면 리스트뷰에서도 선택 처리
                        int pageStartIndex = currentPage * pageSize;
                        int pageEndIndex = pageStartIndex + lstFiles.Items.Count;

                        if (i >= pageStartIndex && i < pageEndIndex)
                        {
                            int localIdx = i - pageStartIndex;
                            if (localIdx >= 0 && localIdx < lstFiles.Items.Count)
                            {
                                lstFiles.Items[localIdx].Selected = true;
                            }
                        }
                    }
                }

                // 6. 결과 알림 및 후속 처리
                if (anomalyCount > 0)
                {
                    // 사용 중이신 라벨 및 다중 삭제 버튼 활성화 함수가 있다면 호출해 줍니다.
                    // (예: UpdateSelectedFileLabel(); 함수명이 있다면 주석을 해제하세요)
                    // UpdateSelectedFileLabel(); 

                    if (btnmp != null) btnmp.Visible = true;

                    MessageBox.Show($"오토 필터링 결과 총 {anomalyCount}개의 불량 의심 프레임이 감지 및 전역 선택되었습니다.\n\n" +
                                    $"'파일 다중 삭제' 버튼을 누르시면 디스크 파일과 카탈로그 레코드가 안전하게 일괄 삭제됩니다.",
                                    "필터 완료", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show("조향 노이즈나 비정상 정지 프레임이 감지되지 않았습니다. 깨끗한 데이터셋입니다!", "필터 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"오토 필터링 실행 중 오류가 발생했습니다:\n{ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // 리스트뷰 업데이트 재개
                lstFiles.EndUpdate();
            }
        }

        private void btnAutoFilter_Click(object sender, EventArgs e)
        {
            // 오토 필터링 시작
            RunAutoFiltering();
        }
    }
}