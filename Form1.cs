using System.Diagnostics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
namespace Donkey_car_manager
{
    public partial class Form1 : Form
    {
        public class CarFileInfo
        {
            public string FilePath { get; set; }
            public DateTime WriteTime { get; set; }
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
        }

        // 4. 폼 로드 이벤트
        private void Form1_Load(object sender, EventArgs e)
        {
            // 시작할 때는 기본 크기로 설정
            this.Width = normalWidth;
            targetWidth = normalWidth;
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
                    // 1. 선택한 폴더에서 JPG 이미지 파일 목록을 배열로 가져옵니다.
                    string[] files = System.IO.Directory.GetFiles(fbd.SelectedPath, "*.jpg");

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

            // 🌟 핵심: carImages[index] 대신 새 바구니인 carImages[index].FilePath를 사용합니다.
            using (System.IO.FileStream fs = new System.IO.FileStream(carImages[index].FilePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                picCurFrame.Image = Image.FromStream(fs);
            }
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

            // 1. 기존 리스트뷰에 있던 아이템들을 싹 비워줍니다.
            lstFiles.Items.Clear();

            // 2. 현재 페이지의 시작 인덱스와 끝 인덱스 계산
            int startIndex = currentPage * pageSize;
            int endIndex = Math.Min(startIndex + pageSize, carImages.Count);

            // 3. 현재 페이지에 해당하는 파일들만 리스트뷰에 탐색기 형태로 넣어줍니다.
            for (int i = startIndex; i < endIndex; i++)
            {
                string filePath = carImages[i].FilePath;
                string fileName = System.IO.Path.GetFileName(filePath);

                // 메모리에 미리 저장해둔 수정 시간을 문자열로 예쁘게 포맷팅
                string fileTime = carImages[i].WriteTime.ToString("yyyy-MM-dd HH:mm:ss");

                // 리스트뷰의 행(Row) 생성: 첫 번째 열에는 파일명
                ListViewItem item = new ListViewItem(fileName);
                // 두 번째 열에는 수정 시간 추가
                item.SubItems.Add(fileTime);

                // 리스트뷰에 최종 추가
                lstFiles.Items.Add(item);
            }

            // 4. 페이지 표시 라벨 갱신
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
        private void lstFiles_SelectedIndexChanged(object sender, EventArgs e)
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
        }

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

        private void btnFileDelete_Click(object sender, EventArgs e)
        {
            // 삭제할 파일이 있는지 먼저 확인
            if (carImages == null || carImages.Count == 0 || currentImageIndex < 0 || currentImageIndex >= carImages.Count)
            {
                MessageBox.Show("삭제할 파일이 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 사용자에게 진짜 삭제할 것인지 한 번 더 확인 (안전장치)
            DialogResult result = MessageBox.Show("현재 프레임 이미지를 정말로 삭제하시겠습니까?", "삭제 확인",
                                                  MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    // 1. 현재 픽쳐박스에 띄워진 이미지를 해제합니다. (파일 잠금 해제 필수!)
                    if (picCurFrame.Image != null)
                    {
                        picCurFrame.Image.Dispose();
                        picCurFrame.Image = null;
                    }

                    // 가비지 컬렉터를 강제로 실행하여 파일 스트림 리소스를 완전히 풀어줍니다.
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    // 2. 실제 컴퓨터 디스크에서 파일 삭제
                    string fileToDelete = carImages[currentImageIndex].FilePath;
                    if (System.IO.File.Exists(fileToDelete))
                    {
                        System.IO.File.Delete(fileToDelete);
                    }

                    // 3. 프로그램 내부 이미지 데이터 리스트에서 해당 경로 삭제
                    carImages.RemoveAt(currentImageIndex);

                    // 4. 파일을 지운 후 데이터 수에 따른 UI 예외 처리
                    if (carImages.Count == 0)
                    {
                        // 모든 파일이 다 지워졌을 때 초기화 상태로 만듦
                        trbFrame.Maximum = 0;
                        trbFrame.Value = 0;
                        currentPage = 0;
                        totalPages = 0;
                        currentImageIndex = 0;

                        lstFiles.Items.Clear();
                        lblCurFilePage.Text = "0 / 0";
                        MessageBox.Show("폴더 내의 모든 이미지가 삭제되었습니다.", "알림");
                    }
                    else
                    {
                        // 리스트의 맨 마지막 프레임을 지운 경우라면 인덱스를 하나 앞으로 당겨줍니다.
                        if (currentImageIndex >= carImages.Count)
                        {
                            currentImageIndex = carImages.Count - 1;
                        }

                        // 전체 페이지 수 재계산
                        totalPages = (int)Math.Ceiling((double)carImages.Count / pageSize);

                        // 프레임이 줄어들면서 현재 페이지 범위를 벗어났다면 현재 페이지 번호를 조절합니다.
                        if (currentPage >= totalPages)
                        {
                            currentPage = totalPages - 1;
                        }

                        // 트랙바 범위 및 값 최신화
                        trbFrame.Maximum = carImages.Count - 1;
                        trbFrame.Value = currentImageIndex;

                        // 리스트박스, 라벨, 픽쳐박스 화면 갱신
                        UpdateListPage();
                        ShowImage(currentImageIndex);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"파일을 삭제하는 중 오류가 발생했습니다:\n{ex.Message}", "오류",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                    // 삭제 실패 시 이미지를 다시 띄워 화면을 복구합니다.
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

        private void btnExtend_Click(object sender, EventArgs e)
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
        }

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
            // 데이터가 없으면 정렬하지 않음
            if (carImages == null || carImages.Count == 0) return;
            if (isPlaying) StopAutoPlay(); // 자동 재생 중이면 정지

            // 클릭할 때마다 정렬 방향을 반대로 토글 (오름차순 <-> 내림차순)
            isAscending = !isAscending;

            // e.Column은 클릭한 컬럼의 번호입니다 (0: 파일명, 1: 수정 시간)
            if (e.Column == 0)
            {
                // 1열(파일명) 헤더 클릭 시 메모리 상에서 초고속 정렬
                carImages = isAscending
                    ? carImages.OrderBy(f => System.IO.Path.GetFileName(f.FilePath)).ToList()
                    : carImages.OrderByDescending(f => System.IO.Path.GetFileName(f.FilePath)).ToList();
            }
            else if (e.Column == 1)
            {
                // 2열(수정 시간) 헤더 클릭 시 메모리 상에서 초고속 정렬 (디스크 접근이 없어 렉이 없습니다)
                carImages = isAscending
                    ? carImages.OrderBy(f => f.WriteTime).ToList()
                    : carImages.OrderByDescending(f => f.WriteTime).ToList();
            }

            // 정렬이 완료되었으므로 첫 페이지, 첫 프레임으로 리셋
            currentImageIndex = 0;
            currentPage = 0;
            trbFrame.Value = 0;

            // 화면 갱신
            UpdateListPage();
            ShowImage(currentImageIndex);
        }
        /*private void ShowImage(int index)
{
// ... 기존 이미지 출력 로직 ...

// 우측 디버그 표에 현재 프레임 데이터 표시하기
// (파일명에서 속도와 조향각을 파싱해왔다고 가정)
double currentSpeed = 0.65;
double currentAngle = -0.15;

// 표에 행(Row) 추가해서 실시간 로그 쌓기
dgvDebug.Rows.Add(index + 1, currentSpeed, currentAngle);

// 스크롤을 맨 아래로 내려서 실시간 디버깅 느낌 주기
dgvDebug.FirstDisplayedScrollingRowIndex = dgvDebug.RowCount - 1;
}*/
    }
}