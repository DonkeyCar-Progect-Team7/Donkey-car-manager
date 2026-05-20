using System.Diagnostics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
namespace Donkey_car_manager
{
    public partial class Form1 : Form
    {
        // 이미지 파일들의 전체 경로를 담을 리스트
        private List<string> imageFiles = new List<string>();

        // 현재 표시 중인 이미지의 인덱스 (0부터 시작)
        private int currentImageIndex = 0;

        private int pageSize = 20;     // 한 페이지에 보여줄 파일(프레임) 개수
        private int currentPage = 0;   // 현재 페이지 번호 (0부터 시작)
        private int totalPages = 0;    // 전체 페이지 개수

        public Form1()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btnFileOpen_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    string selectedFolder = fbd.SelectedPath;

                    imageFiles = System.IO.Directory.GetFiles(selectedFolder, "*.*")
                        .Where(file => file.ToLower().EndsWith(".jpg") ||
                                       file.ToLower().EndsWith(".png") ||
                                       file.ToLower().EndsWith(".jpeg") ||
                                       file.ToLower().EndsWith(".bmp"))
                        .ToList();

                    if (imageFiles.Count > 0)
                    {
                        // 트랙바 초기화
                        trbFrame.Minimum = 0;
                        trbFrame.Maximum = imageFiles.Count - 1;
                        trbFrame.Value = 0;

                        // --- 페이징 계산 추가 ---
                        currentPage = 0;
                        // 전체 페이지 수 = 올림(전체 파일 수 / 페이지당 개수)
                        totalPages = (int)Math.Ceiling((double)imageFiles.Count / pageSize);

                        currentImageIndex = 0;

                        // 리스트박스 및 이미지 업데이트 메서드 호출
                        UpdateListPage();
                        ShowImage(currentImageIndex);
                    }
                    else
                    {
                        MessageBox.Show("이미지 파일이 없습니다.");
                    }
                }
            }
        }

        private void trbFrame_Scroll(object sender, EventArgs e)
        {
            if (imageFiles != null && imageFiles.Count > 0)
            {
                // 트랙바의 현재 위치 값을 인덱스로 설정
                currentImageIndex = trbFrame.Value;
                ShowImage(currentImageIndex);
            }
        }
        private void ShowImage(int index)
        {
            if (index < 0 || index >= imageFiles.Count) return;

            // 픽쳐박스 크기에 맞게 이미지 비율을 유지하며 보여주도록 설정 (디자인 창 속성에서 바꿔도 됩니다)
            picCurFrame.SizeMode = PictureBoxSizeMode.Zoom;

            // 기존에 메모리에 올라간 이미지가 있다면 해제 (메모리 누수 방지)
            if (picCurFrame.Image != null)
            {
                picCurFrame.Image.Dispose();
            }

            // 새로운 이미지 로드
            picCurFrame.Image = Image.FromFile(imageFiles[index]);

            // (선택 사항) 화면 좌측 상단 라벨에 프레임 번호나 파일명을 띄워주면 좋습니다.
            lblFrameNum.Text = $"프레임 번호 : {index + 1} / {imageFiles.Count}";

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
            if (imageFiles == null || imageFiles.Count == 0) return;

            // 리스트박스 비우기
            lstFiles.Items.Clear();

            // 현재 페이지의 시작 인덱스와 끝 인덱스 계산
            int startIndex = currentPage * pageSize;
            int endIndex = Math.Min(startIndex + pageSize, imageFiles.Count);

            // 해당 범위의 파일명을 리스트박스에 추가
            for (int i = startIndex; i < endIndex; i++)
            {
                string fileName = System.IO.Path.GetFileName(imageFiles[i]);
                lstFiles.Items.Add($"[{i + 1}] {fileName}");
            }

            // 🌟 [여기에 추가] 라벨에 현재 페이지와 전체 페이지 정보를 띄워줍니다.
            // 인덱스는 0부터 시작하므로 화면에 보여줄 때는 currentPage + 1을 해줍니다.
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
                if (targetIndex >= 0 && targetIndex < imageFiles.Count)
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
                    MessageBox.Show($"1부터 {imageFiles.Count} 사이의 숫자를 입력해주세요.", "알림");
                }
            }
            else
            {
                MessageBox.Show("올바른 숫자를 입력해주세요.", "알림");
            }
        }
        private void lstFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstFiles.SelectedIndex != -1)
            {
                // 현재 페이지의 시작 인덱스 + 리스트박스에서 선택된 행 번호 = 실제 이미지 인덱스
                int actualIndex = (currentPage * pageSize) + lstFiles.SelectedIndex;

                if (actualIndex >= 0 && actualIndex < imageFiles.Count)
                {
                    currentImageIndex = actualIndex;
                    trbFrame.Value = currentImageIndex;
                    ShowImage(currentImageIndex);
                }
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
                        if (imageFiles != null && imageFiles.Count > 0)
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
            if (imageFiles == null || imageFiles.Count == 0 || currentImageIndex < 0 || currentImageIndex >= imageFiles.Count)
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
                    string fileToDelete = imageFiles[currentImageIndex];
                    if (System.IO.File.Exists(fileToDelete))
                    {
                        System.IO.File.Delete(fileToDelete);
                    }

                    // 3. 프로그램 내부 이미지 데이터 리스트에서 해당 경로 삭제
                    imageFiles.RemoveAt(currentImageIndex);

                    // 4. 파일을 지운 후 데이터 수에 따른 UI 예외 처리
                    if (imageFiles.Count == 0)
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
                        if (currentImageIndex >= imageFiles.Count)
                        {
                            currentImageIndex = imageFiles.Count - 1;
                        }

                        // 전체 페이지 수 재계산
                        totalPages = (int)Math.Ceiling((double)imageFiles.Count / pageSize);

                        // 프레임이 줄어들면서 현재 페이지 범위를 벗어났다면 현재 페이지 번호를 조절합니다.
                        if (currentPage >= totalPages)
                        {
                            currentPage = totalPages - 1;
                        }

                        // 트랙바 범위 및 값 최신화
                        trbFrame.Maximum = imageFiles.Count - 1;
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
            // 1. 리스트박스에서 선택된 항목이 하나도 없는지 검사
            if (lstFiles.SelectedIndices.Count == 0)
            {
                MessageBox.Show("다중 삭제할 파일을 리스트에서 선택해주세요.\n(Ctrl 이나 Shift를 누른 채 클릭하면 여러 개 선택이 가능합니다.)",
                                "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int deleteCount = lstFiles.SelectedIndices.Count;

            // 사용자에게 진짜 삭제할 것인지 최종 확인 (안전장치)
            DialogResult result = MessageBox.Show($"리스트에서 선택한 {deleteCount}개의 프레임 이미지를 정말로 디스크에서 영구 삭제하시겠습니까?",
                                                  "다중 삭제 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    // 2. 현재 픽쳐박스에 띄워진 이미지 리소스 해제 (파일 잠금 현상 방지)
                    if (picCurFrame.Image != null)
                    {
                        picCurFrame.Image.Dispose();
                        picCurFrame.Image = null;
                    }

                    // 가비지 컬렉터를 구동하여 파일 스트림 리소스를 확실하게 정리합니다.
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    // 3. 🌟 핵심: 선택된 리스트박스 행 번호들을 '역순(큰 숫자부터)'으로 정렬하여 추출
                    // 인덱스 뒤에서부터 지워야 리스트가 앞으로 당겨지면서 데이터가 꼬이는 것을 막을 수 있습니다.
                    List<int> selectedIndices = lstFiles.SelectedIndices.Cast<int>().OrderByDescending(i => i).ToList();

                    foreach (int listBoxIndex in selectedIndices)
                    {
                        // (현재 페이지 시작점) + (리스트박스 안에서의 선택 위치) = 실제 imageFiles 리스트의 인덱스
                        int actualIndex = (currentPage * pageSize) + listBoxIndex;

                        if (actualIndex >= 0 && actualIndex < imageFiles.Count)
                        {
                            string fileToDelete = imageFiles[actualIndex];

                            // 실제 컴퓨터 디스크(HDD/SSD)에서 파일 삭제
                            if (System.IO.File.Exists(fileToDelete))
                            {
                                System.IO.File.Delete(fileToDelete);
                            }

                            // 프로그램 내부 데이터 리스트에서 경로 제거
                            imageFiles.RemoveAt(actualIndex);
                        }
                    }

                    MessageBox.Show($"{deleteCount}개의 파일 삭제를 완료했습니다.", "완료", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // 4. 데이터가 삭제된 후 남은 수에 따라 UI 최신화
                    if (imageFiles.Count == 0)
                    {
                        // 모든 파일이 삭제되어 데이터가 아예 없을 때 초기화
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
                        // 전체 페이지 수 다시 계산
                        totalPages = (int)Math.Ceiling((double)imageFiles.Count / pageSize);

                        // 프레임 대량 삭제로 인해 현재 페이지 번호가 최대 페이지를 넘어가면 마지막 페이지로 고정
                        if (currentPage >= totalPages)
                        {
                            currentPage = totalPages - 1;
                        }

                        // 현재 보고 있던 이미지 포인터(인덱스)가 삭제 후의 리스트 범위를 벗어나지 않도록 조정
                        if (currentImageIndex >= imageFiles.Count)
                        {
                            currentImageIndex = imageFiles.Count - 1;
                        }

                        // 트랙바 맥스값 및 슬라이더 위치 최신화
                        trbFrame.Maximum = imageFiles.Count - 1;
                        trbFrame.Value = currentImageIndex;

                        // 화면 UI(리스트박스 목록, 라벨, 이미지 뷰어) 리프레시
                        UpdateListPage();
                        ShowImage(currentImageIndex);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"파일 다중 삭제 작업 중 오류가 발생했습니다:\n{ex.Message}", "오류",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                    // 오류가 발생하더라도 현재 상태로 화면을 복구시킵니다.
                    UpdateListPage();
                    ShowImage(currentImageIndex);
                }
            }
        }
    }
}