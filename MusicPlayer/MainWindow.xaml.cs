﻿using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace MusicPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<string> _paths = new();
        private MediaPlayer _player = new();
        private DispatcherTimer _timer = new();
        public MainWindow()
        {
            InitializeComponent();
            Volume.Value = _player.Volume;
            MemoryReader();
            _timer.Tick += DispatcherTimer_tick;
            _timer.Interval = TimeSpan.FromSeconds(1);
        }

        private void DispatcherTimer_tick(object? sender, EventArgs e)
        {
            if( _player.Source != null && _player.NaturalDuration.HasTimeSpan)
            {
                ProgressBar.Minimum = 0;
                ProgressBar.Maximum = _player.NaturalDuration.TimeSpan.TotalSeconds;
                ProgressBar.Value = _player.Position.TotalSeconds;
                //PassedTimeTB.Text = TimeSpan.FromSeconds(_player.NaturalDuration.TimeSpan.TotalSeconds).ToString(@"hh\:mm\:ss");
            }
            if(_player.Position == _player.NaturalDuration && PlayList.SelectedIndex < PlayList.Items.Count-1)
            {
                NextBtn_Click(sender, null);
            }
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            ofd.Filter = "mp3 files(*.mp3)|*.mp3|wav files(*.wav)|*.wav|aiff files(*.aiff)|*.aiff|ape files(*.ape)|*.ape|flac files(*.flac)|*.flac|ogg files(*.ogg)|*.ogg";
            if (ofd.ShowDialog() == false)
                return;
            foreach (var path in ofd.FileNames) 
            { 
                _paths.Add(path);
            }
            foreach (var name in ofd.SafeFileNames)
            {
                PlayList.Items.Add(name);
            }
        }

        private void PlayList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (PlayList.SelectedIndex == -1)
                return;
            //Uri 
            _player.Open(new Uri(_paths[PlayList.SelectedIndex], UriKind.RelativeOrAbsolute));
            _player.Play();
        }

        private void PlayList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PlayList.SelectedIndex == -1)
                _player.Stop();
            else
            { 
                _player.Open(new Uri(_paths[PlayList.SelectedIndex], UriKind.RelativeOrAbsolute));
                _player.Play();
            }
            _timer.Start();
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            _player.Stop();        }

        private void PlayBtn_Click(object sender, RoutedEventArgs e) => _player.Play();

        private void PauseBtn_Click(object sender, RoutedEventArgs e)
        {
            _player.Pause();
        }

        private void NextBtn_Click(object sender, RoutedEventArgs e)
        {
            if(PlayList.SelectedIndex < PlayList.Items.Count-1)
            {
                PlayList.SelectedIndex += 1;
            }
            else
            {
                PlayList.SelectedIndex = 0;
            }
        }

        private void PrevBtn_Click(object sender, RoutedEventArgs e)
        {
            if (PlayList.SelectedIndex > 0)
            {
                PlayList.SelectedIndex -= 1;
            }
            else
            {
                PlayList.SelectedIndex = PlayList.Items.Count-1;
            }
        }

        private void Volume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _player.Volume = Volume.Value;
            VolumeTB.Text = (Math.Round(_player.Volume * 100)) + "%";
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            DriveInfo drive = DriveInfo.GetDrives()[0];
            string path = drive.Name + @"\MusicPlayer";
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            if (!directoryInfo.Exists) 
            {
                directoryInfo.Create();
            }
            string fileName = drive.Name + @"\MusicPlayer\tmp.dat";
            File.WriteAllLines(fileName, _paths);
        }
        private void MemoryReader()
        {
            DriveInfo drive = DriveInfo.GetDrives()[0];
            string path = drive.Name + @"\MusicPlayer\tmp.dat";
            try
            {
                using(FileStream fs = File.OpenRead(path))
                {
                    string line;
                    StreamReader file = new StreamReader(path);
                    while ((line = file.ReadLine()) != null)
                    { 
                        _paths.Add(line);
                    }
                    file.Close();
                }
                foreach(var item in _paths)
                {
                    PlayList.Items.Add(System.IO.Path.GetFileName(item));
                }
            }
            catch
            {

            }
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            PlayList.Items.Clear();
            _paths = new List<string>();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PlayList files(*.plst)|*.plst";
            if (saveFileDialog.ShowDialog() == false)
            {
                return;
            }
            StreamWriter streamWriter = new StreamWriter(saveFileDialog.FileName);
            foreach(var item in _paths)
            {
                streamWriter.WriteLine(item);
            }
            MessageBox.Show("PlayList has been saved");
            streamWriter.Close();
        }

        private void LoadBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "PlayList files(*.plst)|*.plst";
            if (openFileDialog.ShowDialog() == false)
            {
                return;
            }
            using(StreamReader streamReader = new StreamReader(openFileDialog.FileName))
            {
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    _paths.Add(line);
                    PlayList.Items.Add(System.IO.Path.GetFileName(line));

                }
            }        
        }

        private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            PassedTimeTB.Text = TimeSpan.FromSeconds(ProgressBar.Value).ToString(@"hh\:mm\:ss");
        }

        private void ProgressBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _player.Position = TimeSpan.FromSeconds((e.GetPosition(ProgressBar).X * ProgressBar.Maximum) / ProgressBar.ActualWidth);
        }
    }
}