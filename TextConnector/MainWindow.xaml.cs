using Microsoft.Win32;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;

namespace BitmapEqualer
{
    public partial class MainWindow : Window
    {
        Bitmap firstBitmap;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void FirstFile_onClick(object sender, RoutedEventArgs e)
        {
            ShowFirstFileChooseDialog();
        }

        private void SecondFile_Click(object sender, RoutedEventArgs e)
        {
            ShowSecondFileChooseDialog();
        }

        private void ShowFirstFileChooseDialog()
        {
            IsSuccesCheckBox.IsChecked = false;
            ResultTextBlock.Text = "Выберите первое изображение";

            var dialog = new OpenFileDialog();
            dialog.Filter = "Png Files (.png)|*.png";
            dialog.FileOk += (o, e) =>
            {
                var path = dialog.FileName;

                firstBitmap = LoadBitmap(path);
                if (firstBitmap == null)
                    ResultTextBlock.Text = "Не удалось загрузить изображение";
                else
                {
                    FirstFileButton.IsEnabled = false;
                    SecondFileButton.IsEnabled = true;
                    IsSuccesCheckBox.IsChecked = false;

                    ResultTextBlock.Text = "Выберите второе изображение";
                }
            };
            dialog.ShowDialog();
        }

        private void ShowSecondFileChooseDialog()
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Png Files (.png)|*.png";
            dialog.FileOk += (o, e) =>
            {
                var path = dialog.FileName;

                using (var secondBitmap = LoadBitmap(path))
                {
                    if (secondBitmap == null)
                        ResultTextBlock.Text = "Не удалось загрузить изображение";
                    else
                    {
                        IsSuccesCheckBox.IsChecked = CheckIsBitmapEqual(firstBitmap, secondBitmap);
                        ResultTextBlock.Text = (bool)IsSuccesCheckBox.IsChecked ? "Изображения идентичны" : "Изображения не идентичны";

                        firstBitmap.Dispose();
                    }
                }

                FirstFileButton.IsEnabled = true;
                SecondFileButton.IsEnabled = false;
            };
            dialog.ShowDialog();
        }

        private Bitmap LoadBitmap(string path)
        {
            Bitmap bitmap = null;
            try
            {
                var fileStream = File.OpenRead(path);
                bitmap = new Bitmap(fileStream);
            }
            catch { }

            return bitmap;
        }

        private unsafe bool CheckIsBitmapEqual(Bitmap first, Bitmap second)
        {
            var isEqual = false;

            var width = first.Width;
            var height = first.Height;

            if (width != second.Width || height != second.Height)
                goto finish;

            var newBitmapData = first.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            var userBitmapData = second.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            try
            {
                var userBitmapDataStartPosition = ((byte*)userBitmapData.Scan0);
                var newBitmapDataStartPosition = ((byte*)newBitmapData.Scan0);

                byte* userBitmapCurrentPosition, newBitmapCurrentPosition;
                for (int h = 0; h < height; h++)
                {
                    userBitmapCurrentPosition = userBitmapDataStartPosition + h * userBitmapData.Stride;
                    newBitmapCurrentPosition = newBitmapDataStartPosition + h * newBitmapData.Stride;

                    for (int w = 0; w < width; w++)
                    {
                        for (int i = 0; i < 4; i++)
                            if (*(userBitmapCurrentPosition++) != *(newBitmapCurrentPosition++))
                                goto finish;
                    }
                }
                userBitmapCurrentPosition = newBitmapCurrentPosition = userBitmapDataStartPosition = newBitmapDataStartPosition = null;
                isEqual = true;
            }
            finally { }

            finish:
            return isEqual;
        }
    }
}
