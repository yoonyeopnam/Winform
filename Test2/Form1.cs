using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test2
{
    public partial class Form1 : Form
    {
        private string backupFolderPath; // 백업 폴더의 경로
        private string selectedPath; // 선택한 폴더의 경로

        public Form1()
        {
            InitializeComponent();
        }
        private void mybutton1_Click(object sender, EventArgs e)
        {
            // 폴더 선택 대화 상자 생성
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedPath = folderBrowserDialog.SelectedPath;

                    // 선택된 폴더의 상위 폴더에 "backup" 폴더 경로 설정
                    string parentDirectory = Directory.GetParent(selectedPath).FullName;
                    backupFolderPath = Path.Combine(parentDirectory, "backup");
                    MessageBox.Show(backupFolderPath); //debug용

                    // backup 폴더가 없으면 생성
                    if (!Directory.Exists(backupFolderPath))
                    {
                        Directory.CreateDirectory(backupFolderPath);
                    }

                    // 선택한 폴더의 모든 .txt 및 .dcm 파일 절대 경로를 가져와 CheckedListBox에 추가
                    string[] txtFiles = Directory.GetFiles(selectedPath, "*.txt", SearchOption.AllDirectories);
                    string[] dcmFiles = Directory.GetFiles(selectedPath, "*.dcm", SearchOption.AllDirectories);

                    // CheckedListBox에 절대 경로 추가
                    checkedListBox1.Items.Clear();
                    foreach (string filePath in txtFiles.Concat(dcmFiles))
                    {
                        // 절대 경로를 그대로 추가
                        checkedListBox1.Items.Add(filePath);
                    }

                }
            }
        }

        private void BackupFiles()
        {
            // 각 파일을 백업 폴더에 복사하고 동일한 폴더 구조 생성
            foreach (var selectedItem in checkedListBox1.CheckedItems)
            {
                string originalFilePath = selectedItem.ToString();
                // 상대 경로 생성
                Uri baseUri = new Uri(Directory.GetParent(backupFolderPath).FullName + Path.DirectorySeparatorChar);
                
                Uri fileUri = new Uri(originalFilePath);
                string relativePath = Uri.UnescapeDataString(baseUri.MakeRelativeUri(fileUri).ToString()).Replace('/', Path.DirectorySeparatorChar);


                // 첫 번째 하위 폴더 제거
                string[] pathParts = relativePath.Split(Path.DirectorySeparatorChar);
                if (pathParts.Length > 1)
                {
                    relativePath = Path.Combine(pathParts.Skip(1).ToArray()); // 첫 번째 폴더를 제거하고 경로 다시 결합
                }

                string backupFilePath = Path.Combine(backupFolderPath, relativePath);

                // 백업 폴더 내 파일이 위치한 디렉터리 생성
                string backupDir = Path.GetDirectoryName(backupFilePath);
                if (!Directory.Exists(backupDir))
                {
                    Directory.CreateDirectory(backupDir);
                }

                // 파일을 백업 폴더에 복사
                File.Copy(originalFilePath, backupFilePath, true);
            }
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void textModbutton_Click(object sender, EventArgs e)
        {
            BackupFiles(); // 파일 변경 전에 백업 생성
            string textToAdd = textBox1.Text;

            foreach (var selectedItem in checkedListBox1.CheckedItems)
            {
                string filePath = selectedItem.ToString();
                try
                {
                    // 파일 끝에 한 줄 내린 후 textBox1에 입력된 텍스트 추가
                    File.AppendAllText(filePath, Environment.NewLine + textToAdd);
                    MessageBox.Show("선택된 파일에 텍스트가 추가되었습니다.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"파일을 수정하는 중 오류 발생: {ex.Message}");
                }
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void undobutton_Click(object sender, EventArgs e)
        {
            // 백업된 파일을 원래 파일로 복구
            foreach (var selectedItem in checkedListBox1.CheckedItems)
            {
                string originalFilePath = selectedItem.ToString();

                // 백업 파일 경로 생성
                Uri baseUri = new Uri(Directory.GetParent(backupFolderPath).FullName + Path.DirectorySeparatorChar);
                Uri fileUri = new Uri(originalFilePath);
                string relativePath = Uri.UnescapeDataString(baseUri.MakeRelativeUri(fileUri).ToString()).Replace('/', Path.DirectorySeparatorChar);

                string backupFilePath = Path.Combine(backupFolderPath, relativePath);

                try
                {
                    if (File.Exists(backupFilePath))
                    {
                        File.Copy(backupFilePath, originalFilePath, true); // 백업 파일을 원래 위치에 덮어쓰기
                        MessageBox.Show("backupFilePath"+ backupFilePath+"\roriginalFilePaht"+ backupFilePath);
                        MessageBox.Show("선택된 파일이 원상복구되었습니다.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"파일을 복구하는 중 오류 발생: {ex.Message}");
                }
            }
            MessageBox.Show("선택된 파일이 원상복구되었습니다.");
        }

        private void CompareButton_Click(object sender, EventArgs e)
        {
            string winMergePath = @"C:\Program Files\WinMerge\WinMergeU.exe"; // WinMerge 실행 파일 경로

            if (!File.Exists(winMergePath))
            {
                MessageBox.Show("WinMerge가 설치되어 있지 않거나 경로가 잘못되었습니다.");
                return;
            }

            if (!Directory.Exists(backupFolderPath) || string.IsNullOrEmpty(selectedPath))
            {
                MessageBox.Show("백업 폴더 또는 원본 폴더 경로가 잘못되었습니다.");
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = winMergePath, // 실행할 프로그램의 경로
                Arguments = $"\"{backupFolderPath}\" \"{selectedPath}\"", // 명령줄 인수, 두 경로를 전달
                UseShellExecute = false // 프로세스를 쉘 없이 직접 실행
            });
        }
    }
}
