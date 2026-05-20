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

                    // 해당 폴더에서 jpg, png, jpeg 확장자를 가진 파일들을 가져옵니다.
                    imageFiles = System.IO.Directory.GetFiles(selectedFolder, "*.*")
                        .Where(file => file.ToLower().EndsWith(".jpg") ||
                                       file.ToLower().EndsWith(".png") ||
                                       file.ToLower().EndsWith(".jpeg") ||
                                       file.ToLower().EndsWith(".bmp"))
                        .ToList();

                    if (imageFiles.Count > 0)
                    {
                        // 1. 트랙바 설정 초기화 (최솟값 0, 최댓값은 이미지 개수 - 1)
                        trbFrame.Minimum = 0;
                        trbFrame.Maximum = imageFiles.Count - 1;
                        trbFrame.Value = 0;

                        // 2. 첫 번째 이미지 보여주기
                        currentImageIndex = 0;
                        ShowImage(currentImageIndex);
                    }
                    else
                    {
                        MessageBox.Show("선택한 폴더에 이미지 파일이 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
    
    }
}
