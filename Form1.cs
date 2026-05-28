using System.Diagnostics;
using System.Windows.Forms.DataVisualization.Charting;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Windows.Forms.DataVisualization.Charting;
using System.Runtime.InteropServices;

namespace Donkey_car_manager
{
    public partial class Form1 : Form
    {
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

        // 정렬 상태(오름차순/내림차순)를 기억할 패스포트 변수도 같이 추가해 줍니다.
        private bool isAscending = true;
        // 1. 기존 이미지 및 페이징 변수들
        //private List<string> carImages = new List<string>();
        private int currentImageIndex = 0;
        private int pageSize = 20;
        private int currentPage = 0;
        private int totalPages = 0;

        // 2. 기록창 확장을 위한 전역 변수들 (클래스 바로 아래 정상 배치)
        private bool isDebugExpanded = false;
        private int normalWidth = 1147;
        private int expandedWidth = 1600;
        private int targetWidth = 1147;
        // 🌟 기존 전역 변수들이 있는 곳(class Form1 바로 아래)에 추가하세요!
        private bool isPlaying = false;

        // 🌟 코드 창 빈 곳(메서드 바깥)에 추가하세요! 자동 넘기기를 안전하게 끄는 메서드입니다.
        private void StopAutoPlay()
        {
            timerPlay.Stop();
            btnAutoPic.Text = "자동 넘기기";
            btnAutoPic.BackColor = SystemColors.Control; // 버튼 색상 원상복구
            isPlaying = false;
        }
        // 3. 프로그램 생성자 (대괄호 짝을 완벽히 맞춤)
        public Form1()
        {
            InitializeComponent();
            this.lstFiles.MouseWheel += new MouseEventHandler(lstFiles_MouseWheel);
            // 🌟 픽처박스의 깜빡임과 이미지 튀는 현상을 방지하는 더블 버퍼링 활성화
            System.Reflection.PropertyInfo aProp = typeof(System.Windows.Forms.Control)
                .GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            aProp.SetValue(picCurFrame, true, null);
            // Init chart 영역
            InitDriveChart();
            // Ensure chart is brought to front after form is shown and report status
            this.Shown += Form1_Shown;
        }
        // 동키카 그래프를 위한 코드
        private Chart chartDriveData; // 전역 변수로 차트 선언

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
            sA.Points.AddXY(frameIndex, angle);
            sT.Points.AddXY(frameIndex, throttle);
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
                    string[] files = System.IO.Directory.GetFiles(fbd.SelectedPath, "*.*")
                        .Where(s => s.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                    s.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                                    s.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                                    s.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
                        .ToArray();

                    // 2. 아까 만든 새 바구니(carImages)를 깨끗하게 비워줍니다.
                    carImages.Clear();

                    // 3. 🌟 핵심: 파일을 로드하는 이 시점에 '딱 한 번만' 디스크에서 수정 시간을 읽어와 저장합니다.
                    // 이렇게 메모리에 미리 다 올려두어야 나중에 정렬할 때 렉이 전혀 안 걸립니다.
                    foreach (string file in files)
                    {
                        carImages.Add(new CarFileInfo
                        {
                            FilePath = file,
                            WriteTime = System.IO.File.GetLastWriteTime(file)
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

                chartDriveData.Refresh();
            }

            // 🌟 [추가] 트랙바나 자동 재생 시에도 상단 라벨이 "프레임 번호 : X / Y" 형태로 실시간 동기화되도록 설정
            lblFrameNum.Text = $"프레임 번호 : {index + 1} / {carImages.Count}";
        }

        private void trbFrame_MouseDown(object sender, MouseEventArgs e)
        {
            // 마우스 왼쪽 버튼을 클릭했을 때만 작동
            if (e.Button == MouseButtons.Left)
            {
                // 마우스 클릭 위치(e.X)가 트랙바 전체 가로 크기(Width) 중 어느 정도 비율인지 구합니다.
                double clickRatio = (double)e.X / (double)trbFrame.Width;

                // 트랙바의 총 범위 (최대값 - 최소값)
                int totalRange = trbFrame.Maximum - trbFrame.Minimum;

                // 비율에 맞는 새로운 프레임 값 계산
                int newValue = trbFrame.Minimum + (int)Math.Round(clickRatio * totalRange);

                // 계산된 값이 범위를 벗어나지 않도록 안전장치 설정
                if (newValue < trbFrame.Minimum) newValue = trbFrame.Minimum;
                if (newValue > trbFrame.Maximum) newValue = trbFrame.Maximum;

                // 트랙바의 위치를 클릭한 곳으로 변경
                trbFrame.Value = newValue;

                // 현재 인덱스를 업데이트하고 이미지를 화면에 띄움
                currentImageIndex = newValue;
                ShowImage(currentImageIndex);
            }
        }
        private void UpdateListPage()
        {
            if (carImages == null) return;

            // 1. 기존 리스트뷰 아이템들 초기화
            lstFiles.Items.Clear();

            int startIndex = currentPage * pageSize;
            int endIndex = Math.Min(startIndex + pageSize, carImages.Count);

            // 2. 현재 페이지의 파일들을 루프 돌며 리스트뷰에 삽입
            for (int i = startIndex; i < endIndex; i++)
            {
                string filePath = carImages[i].FilePath;
                string fileName = System.IO.Path.GetFileName(filePath);
                string fileTime = carImages[i].WriteTime.ToString("yyyy-MM-dd HH:mm:ss");

                // 🌟 1부터 시작하는 전체 순번 계산 (인덱스는 0부터 시작하므로 i + 1)
                string fileNumber = (i + 1).ToString();

                // [행 추가] 맨 첫 번째 열(번호)에 순번을 집어넣습니다.
                ListViewItem item = new ListViewItem(fileNumber);

                // 두 번째 열(파일명)과 세 번째 열(수정 시간)에 데이터를 하위 항목으로 붙입니다.
                item.SubItems.Add(fileName);
                item.SubItems.Add(fileTime);

                lstFiles.Items.Add(item);
            }

            lblCurFilePage.Text = $"{currentPage + 1} / {totalPages}";
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
            // 1. 리스트뷰에서 선택된 항목이 있는지 확인
            if (lstFiles.SelectedItems.Count == 0)
            {
                MessageBox.Show("삭제할 프레임을 리스트뷰에서 선택해 주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 자동 재생 중이면 잠시 멈춤
            if (isPlaying) StopAutoPlay();

            // 2. 현재 선택된 아이템의 '번호(순번)'를 가져와 실제 데이터 인덱스로 역산
            ListViewItem selectedItem = lstFiles.SelectedItems[0];
            int globalIndex = int.Parse(selectedItem.Text) - 1;

            if (globalIndex < 0 || globalIndex >= carImages.Count) return;

            string targetFilePath = carImages[globalIndex].FilePath;

            // 3. 사용자에게 진짜 지울지 확인
            DialogResult result = MessageBox.Show(
                $"선택한 프레임({selectedItem.SubItems[1].Text})을 정말로 삭제하시겠습니까?\n디스크에서 파일이 영구히 삭제됩니다.",
                "파일 삭제 확인",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    // 4. 픽처박스가 파일을 붙잡고 있어서 생기는 삭제 에러(유령 프레임 원인) 방지
                    if (picCurFrame.Image != null)
                    {
                        picCurFrame.Image.Dispose();
                        picCurFrame.Image = null;
                    }

                    // 5. 실제 하드디스크에서 이미지 파일 삭제
                    if (System.IO.File.Exists(targetFilePath))
                    {
                        System.IO.File.Delete(targetFilePath);
                    }

                    // 6. 데이터 바구니(carImages)에서 제거
                    carImages.RemoveAt(globalIndex);

                    // 7. 데이터 개수 기반 전체 페이지 및 트랙바 범위 재계산
                    totalPages = (int)Math.Ceiling((double)carImages.Count / pageSize);
                    trbFrame.Maximum = Math.Max(0, carImages.Count - 1);

                    // 8. 현재 인덱스가 범위를 벗어나지 않도록 보정
                    if (currentImageIndex >= carImages.Count)
                    {
                        currentImageIndex = Math.Max(0, carImages.Count - 1);
                    }
                    currentPage = currentImageIndex / pageSize;

                    // 9. 화면 즉시 새로고침 (리스트뷰와 이미지 뷰어)
                    UpdateListPage();
                    ShowImage(currentImageIndex);

                    MessageBox.Show("프레임이 성공적으로 삭제되었습니다.", "완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"파일 삭제 중 에러가 발생했습니다:\n{ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    // 에러가 나더라도 이미지는 다시 안전하게 띄워주기
                    ShowImage(currentImageIndex);
                }
            }
        }

        private void btnFileMultiDel_Click(object sender, EventArgs e)
        {
            // 1. 리스트뷰에서 선택된 항목이 없으면 리턴
            if (lstFiles.SelectedIndices.Count == 0)
            {
                MessageBox.Show("다중 삭제할 파일을 리스트에서 선택해주세요.\n(Ctrl 이나 Shift를 누른 채 클릭하면 여러 개 선택이 가능합니다.)",
                                "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int deleteCount = lstFiles.SelectedIndices.Count;

            DialogResult result = MessageBox.Show($"리스트에서 선택한 {deleteCount}개의 프레임 이미지를 정말로 디스크에서 영구 삭제하시겠습니까?",
                                                  "다중 삭제 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    // 현재 화면에 띄워진 이미지 프로세스 해제 (파일 삭제를 위해 메모리 방출)
                    if (picCurFrame.Image != null)
                    {
                        picCurFrame.Image.Dispose();
                        picCurFrame.Image = null;
                    }

                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    // 2. 🌟 핵심: 리스트뷰의 선택된 인덱스들을 역순으로 정렬하여 리스트로 가져옵니다.
                    // (앞에서부터 지우면 인덱스가 당겨져서 엉뚱한 파일이 지워지기 때문에 뒤에서부터 지워야 합니다)
                    List<int> selectedIndices = lstFiles.SelectedIndices.Cast<int>().OrderByDescending(i => i).ToList();

                    foreach (int listBoxIndex in selectedIndices)
                    {
                        int actualIndex = (currentPage * pageSize) + listBoxIndex;

                        if (actualIndex >= 0 && actualIndex < carImages.Count)
                        {
                            string fileToDelete = carImages[actualIndex].FilePath;

                            // 디스크에서 실제 파일 삭제
                            if (System.IO.File.Exists(fileToDelete))
                            {
                                System.IO.File.Delete(fileToDelete);
                            }

                            // 메모리 바구니에서도 삭제
                            carImages.RemoveAt(actualIndex);
                        }
                    }

                    MessageBox.Show($"{deleteCount}개의 파일 삭제를 완료했습니다.", "완료", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // 3. UI 데이터 갱신 로직
                    if (carImages.Count == 0)
                    {
                        trbFrame.Maximum = 0;
                        trbFrame.Value = 0;
                        currentPage = 0;
                        totalPages = 0;
                        currentImageIndex = 0;

                        lstFiles.Items.Clear();
                        lblCurFilePage.Text = "0 / 0";
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

                        trbFrame.Maximum = carImages.Count - 1;
                        trbFrame.Value = currentImageIndex;

                        UpdateListPage();
                        ShowImage(currentImageIndex);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"파일 다중 삭제 작업 중 오류가 발생했습니다:\n{ex.Message}", "오류",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                    UpdateListPage();
                    ShowImage(currentImageIndex);
                }
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

            // 이미지 출력 및 페이지 리스트박스 동기화 처리
            ShowImage(currentImageIndex);
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
            if (e.IsSelected)
            {
                // 현재 페이지와 내부 인덱스를 조합해 전체 기준의 실제 인덱스 계산
                int actualIndex = (currentPage * pageSize) + e.ItemIndex;

                if (carImages != null && actualIndex >= 0 && actualIndex < carImages.Count)
                {
                    currentImageIndex = actualIndex;
                    trbFrame.Value = currentImageIndex;
                    ShowImage(currentImageIndex);

                    // 🌟 클릭한 항목의 정보를 기반으로 UI 전체 갱신 호출
                    UpdatePageUI(e.ItemIndex);
                }
            }
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
            // 1 구역: 리눅스(WSL) 동키카 서버 구동 세팅
            // =================================================================
            ProcessStartInfo wslInfo = new ProcessStartInfo();
            wslInfo.FileName = "wsl.exe";

            // 고정 데이터 보존
            string linuxUser = "root";
            string mycarFolder = "mysim";
            string condaPath = "/root/miniconda3";
            string linuxPath = "/root/mysim";

            // 🌟 윈도우 프로세스를 블로킹하지 않도록 UseShellExecute를 true로 설정하고, 
            // 별도의 백그라운드 창에서 리눅스 서버가 독자적으로 돌도록 안전하게 분리합니다.
            wslInfo.UseShellExecute = true;
            wslInfo.CreateNoWindow = false;
            wslInfo.WorkingDirectory = @"\\wsl$\Ubuntu\root\mysim";

            // 리눅스 명령어 정제 (동키카 가상환경 활성화 및 드라이브 서버 구동)
            string linuxCommand = $"cd {linuxPath} && source {condaPath}/bin/activate donkeycar && python3 manage.py drive";
            wslInfo.Arguments = $"-d Ubuntu -c \"{linuxCommand}\"";

            // =================================================================
            // 2 구역: 🌟 윈도우 동키카 시뮬레이터(Donkeycar Sim.exe) 실행 세팅
            // =================================================================
            ProcessStartInfo simInfo = new ProcessStartInfo();

            // 사용중이신 로컬 PC의 시뮬레이터 절대 경로
            simInfo.FileName = @"D:\Nothings\DonkeySimWin\DonkeySimWin\donkey_sim.exe";

            // 시뮬레이터가 실행될 때 자기 폴더 안의 리소스를 정상적으로 참조할 수 있도록 작업 디렉토리 필수 지정
            if (System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(simInfo.FileName)))
            {
                simInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(simInfo.FileName);
            }
            simInfo.UseShellExecute = true;

            // =================================================================
            // 3 구역: 차례대로 멈춤 없이 실행하기 (프로세스 순차 기동 로직)
            // =================================================================
            try
            {
                // 1. 리눅스 동키카 파이썬 서버 구동 (이제 C# 스레드를 붙잡지 않고 즉시 넘어갑니다)
                Process wslProcess = new Process();
                wslProcess.StartInfo = wslInfo;
                wslProcess.Start();

                // 2. 파이썬 웹 소켓 서버가 최소한 기동될 수 있도록 아주 잠깐 대기 (0.5초)
                System.Threading.Thread.Sleep(500);

                // 3. 🌟 [정상화 핵심] 로컬 윈도우 유니티 시뮬레이터 프로그램 실행
                Process simProcess = new Process();
                simProcess.StartInfo = simInfo;
                simProcess.Start();

                // 4. 시뮬레이터와 서버가 웹 핸드셰이킹을 완료할 수 있도록 1.5초 대기
                System.Threading.Thread.Sleep(1500);

                // 5. 기본 브라우저를 통해 최종 제어 웹 사이트 오픈
                string donkeyUrl = "http://localhost:8887";
                Process.Start(new ProcessStartInfo(donkeyUrl) { UseShellExecute = true });

                MessageBox.Show("동키카 서버, 로컬 시뮬레이터 프로그램, 제어 웹사이트가 모두 성공적으로 연동되었습니다!",
                                "구동 성공", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                // 🌟 바뀐 버튼 이름인 btnFileDelete_Click을 정확하게 호출해 줍니다!
                btnFileDelete_Click(sender, e);
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

            // 7. 🌟 [학습 최종 완료 이벤트] AI 학습이 정상적으로 끝났을 때 실행
            trainProcess.Exited += (s, args) =>
            {
                this.BeginInvoke(new Action(() =>
                {
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
    }
}