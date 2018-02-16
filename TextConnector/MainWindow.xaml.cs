﻿using Microsoft.Win32;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;

namespace BitmapEqualer
{
    public partial class MainWindow : Window
    {
        string firstPath = "", secondPath = "";
        Bitmap firstBitmap, secondBitmap;

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
            var dialog = new OpenFileDialog();
            dialog.Filter = "Png Files (.png)|*.png";
            dialog.FileOk += (o, e) =>
            {
                firstPath = dialog.FileName;
                FirstFileButton.IsEnabled = false;
                SecondFileButton.IsEnabled = true;

                var fileStream = File.OpenRead(firstPath);
                firstBitmap = new Bitmap(fileStream);
            };
            dialog.ShowDialog();
        }

        private void ShowSecondFileChooseDialog()
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Png Files (.png)|*.png";
            dialog.FileOk += (o, e) =>
            {
                secondPath = dialog.FileName;

                var fileStream = File.OpenRead(secondPath);
                secondBitmap = new Bitmap(fileStream);

                IsSuccesCheckBox.IsChecked = CheckIsBitmapEqual(firstBitmap, secondBitmap);
                firstBitmap.Dispose();
                secondBitmap.Dispose();

                FirstFileButton.IsEnabled = true;
                SecondFileButton.IsEnabled = false;
            };
            dialog.ShowDialog();
        }

        private unsafe bool CheckIsBitmapEqual(Bitmap first, Bitmap second)
        {
            var isEqual = true;

            var width = first.Width < second.Width ? first.Width : second.Width;
            var height = first.Height < second.Height ? first.Height : second.Height;

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
                            {
                                isEqual = false;
                                goto finish;
                            }
                    }
                }
                userBitmapCurrentPosition = newBitmapCurrentPosition = userBitmapDataStartPosition = newBitmapDataStartPosition = null;
            }
            finally { }

            finish:
            return isEqual;
        }
    }
}
