using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;
using VidSubRenamer.Properties;

namespace VidSubRenamer
{
    public enum Target { Video, Subtitle };
    public partial class MainForm : Form
    {
        public static string path;
        public static Target renameTarget;
        private readonly string[] videoExtensions = { ".mp4", ".mkv", ".webm", ".flv", ".vob", ".avi", ".mts", ".ts", ".m2ts", ".mov", ".wmv", ".rm", ".rmvb", ".asf", ".amv", ".m4v", ".mpg", ".mpeg", ".mpv", ".m4v" };
        private readonly string[] subtitleExtentions = { ".smi", ".srt", ".ass", ".ssa", ".idx", ".sub", ".sup", ".psb" };
        private List<FileInfo> videoFiles, subtitleFiles;
        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            btnRename.Enabled = false;
            refreshFolderToolStripMenuItem.Enabled = false;
            moveItemsToTopToolStripMenuItem.Enabled = false;
            moveItemsUpToolStripMenuItem.Enabled = false;
            moveItemsDownToolStripMenuItem.Enabled = false;
            moveItemsToBottomToolStripMenuItem.Enabled = false;
            deleteItemsToolStripMenuItem.Enabled = false;
            tsbRefresh.Enabled = false;
            tsbToTop.Enabled = false;
            tsbUp.Enabled = false;
            tsbDown.Enabled = false;
            tsbToBottom.Enabled = false;
            tsbRemove.Enabled = false;
            comboBox1.SelectedIndex = 0;
            renameTarget = Target.Subtitle;
            listView1.ColumnClick += new ColumnClickEventHandler(ColumnClick);
            listView2.ColumnClick += new ColumnClickEventHandler(ColumnClick);

            preferencesToolStripMenuItem.Enabled = false;   // TODO: implement preference form
        }

        private void ColumnClick(object senderef, ColumnClickEventArgs e)
        {
            if (e.Column == 1 || e.Column == 2)
                return;

            ListView listView = (ListView)senderef;

            if (listView.Items.Count == 0)
                return;

            listView.Columns[e.Column].Text = listView.Columns[e.Column].Text.Replace(" ▼", "");
            listView.Columns[e.Column].Text = listView.Columns[e.Column].Text.Replace(" ▲", "");

            if (listView.Sorting == SortOrder.None || listView.Sorting == SortOrder.Ascending)
            {
                listView.ListViewItemSorter = new ListViewItemComparer(e.Column, "desc");
                listView.Sorting = SortOrder.Descending;
                listView.Columns[e.Column].Text += " ▼";

                if (listView == listView1)
                    videoFiles = videoFiles.OrderByDescending(f => f.Name).ToList();
                else if (listView == listView2)
                    subtitleFiles = subtitleFiles.OrderByDescending(f => f.Name).ToList();
            }
            else
            {
                listView.ListViewItemSorter = new ListViewItemComparer(e.Column, "asc");
                listView.Sorting = SortOrder.Ascending;
                listView.Columns[e.Column].Text += " ▲";

                if (listView == listView1)
                    videoFiles = videoFiles.OrderBy(f => f.Name).ToList();
                else if (listView == listView2)
                    subtitleFiles = subtitleFiles.OrderBy(f => f.Name).ToList();
            }
            listView.Sort();
        }

        private string FormatSize(long _size)
        {
            double size = _size;
            string[] units = { "B", "KB", "MB", "GB", "TB" };
            int idx = 0;

            while (size > 1024)
            {
                idx++;
                size /= 1024;
            }

            return String.Format("{0:0.##} {1}", size, units[idx]);
        }

        void ListFiles(ListView _listView, IEnumerable<FileInfo> _files)
        {
            _listView.Clear();
            _listView.BeginUpdate();

            _listView.Sorting = SortOrder.Ascending;
            _listView.Sort();

            // Add files to ListView
            foreach (var item in _files)
            {
                ListViewItem lvi = new ListViewItem(item.Name);
                lvi.SubItems.Add(FormatSize(item.Length));
                lvi.SubItems.Add(item.Extension);
                _listView.Items.Add(lvi);
            }

            // set columns
            _listView.Columns.Add("Name", 300, HorizontalAlignment.Center);
            _listView.Columns.Add("Size", 70, HorizontalAlignment.Center);
            _listView.Columns.Add("Type", 50, HorizontalAlignment.Center);

            // Refresh listviews
            _listView.EndUpdate();
        }

        private void OpenPath(string _path)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            FileInfo[] files = di.GetFiles();

            // clear lists
            if (videoFiles != null)
                videoFiles.Clear();
            if (subtitleFiles != null)
                subtitleFiles.Clear();

            // Get video files from path 
            videoFiles = (files.Where(
               f => videoExtensions.Contains(f.Extension.ToLower())).ToList());

            // Get subtitle files from path 
            subtitleFiles = (files.Where(
                f => subtitleExtentions.Contains(f.Extension.ToLower()))).ToList();

            // sort
            videoFiles = videoFiles.OrderBy(f => f.Name).ToList();
            subtitleFiles = subtitleFiles.OrderBy(f => f.Name).ToList();

            // reset sort order
            listView1.Sorting = SortOrder.None;
            listView2.Sorting = SortOrder.None;

            // list files to each ListViews
            ListFiles(listView1, videoFiles);
            ListFiles(listView2, subtitleFiles);

            UpdateCounterLabels();

            if (listView1.Items.Count > 0 && listView2.Items.Count > 0)
                btnRename.Enabled = true;
            else
                btnRename.Enabled = false;

            tsbRefresh.Enabled = true;
            refreshFolderToolStripMenuItem.Enabled = true;

            OnAndOffMoveButtons(listView1);
            OnAndOffMoveButtons(listView2);
        }

        private void UpdateCounterLabels()
        {
            lblVideoCount.Text = listView1.Items.Count + " item" + (listView1.Items.Count > 1 ? "s" : "");
            lblSubtitleCount.Text = listView2.Items.Count + " item" + (listView2.Items.Count > 1 ? "s" : "");
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            // FileOpen style FolderOpen dialog
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                path = dialog.FileName;
                txtPath.Text = path;
                OpenPath(path);
            }
        }

        private void MoveToTop(ListView _listView, List<FileInfo> _file)
        {
            if (_listView.SelectedItems.Count == 0)
                return;

            _listView.Sorting = SortOrder.None;
            _listView.BeginUpdate();

            int[] indexes = _listView.SelectedIndices.Cast<int>().ToArray();

            for (int i = 0; i < indexes.Length; i++)
            {
                if (indexes[i] == 0)
                    continue;

                // moving down in ListView
                ListViewItem selectedItem = _listView.Items[indexes[i]];
                _listView.Items.Remove(selectedItem);
                _listView.Items.Insert(i, selectedItem);

                // moving down in file list
                FileInfo selectedFile = _file[indexes[i]];
                _file.Remove(selectedFile);
                _file.Insert(i, selectedFile);
            }

            _listView.EndUpdate();
        }

        private void MoveUpItems(ListView _listView, List<FileInfo> _file)
        {
            if (_listView.SelectedItems.Count == 0)
                return;

            _listView.Sorting = SortOrder.None;
            _listView.BeginUpdate();

            int[] indexes = _listView.SelectedIndices.Cast<int>().ToArray();
            int firstIndex = indexes[0] - 1;

            for (int i = 0; i < indexes.Length; i++)
            {
                if (indexes[i] == 0)
                {
                    firstIndex = 0;
                    continue;
                }

                // moving up in ListView
                ListViewItem selectedItem = _listView.Items[indexes[i]];
                _listView.Items.Remove(selectedItem);
                _listView.Items.Insert(firstIndex + i, selectedItem);

                // moving up in file list
                FileInfo selectedFile = _file[indexes[i]];
                _file.Remove(selectedFile);
                _file.Insert(firstIndex + i, selectedFile);
            }

            _listView.EndUpdate();
        }

        private void MoveDownItems(ListView _listView, List<FileInfo> _file)
        {
            if (_listView.SelectedItems.Count == 0)
                return;

            _listView.Sorting = SortOrder.None;
            _listView.BeginUpdate();

            int[] indexes = _listView.SelectedIndices.Cast<int>().ToArray();
            int lastIndex = indexes[indexes.Length - 1] + 1;

            for (int i = indexes.Length - 1; i >= 0; i--)
            {
                if (indexes[i] == _listView.Items.Count - 1)
                    continue;

                // moving down in ListView
                ListViewItem selectedItem = _listView.Items[indexes[i]];
                _listView.Items.Remove(selectedItem);
                _listView.Items.Insert(lastIndex - (indexes.Length - 1 - i), selectedItem);

                // moving down in file list
                FileInfo selectedFile = _file[indexes[i]];
                _file.Remove(selectedFile);
                _file.Insert(lastIndex - (indexes.Length - 1 - i), selectedFile);
            }

            _listView.EndUpdate();
        }

        private void MoveToBottom(ListView _listView, List<FileInfo> _file)
        {
            if (_listView.SelectedItems.Count == 0)
                return;

            _listView.Sorting = SortOrder.None;
            _listView.BeginUpdate();

            int[] indexes = _listView.SelectedIndices.Cast<int>().ToArray();

            for (int i = indexes.Length - 1; i >= 0; i--)
            {
                if (indexes[i] == _listView.Items.Count - 1)
                    continue;

                // moving down in ListView
                ListViewItem selectedItem = _listView.Items[indexes[i]];
                _listView.Items.Remove(selectedItem);
                _listView.Items.Insert(_listView.Items.Count + 1 - (indexes.Length - i), selectedItem);

                // moving down in file list
                FileInfo selectedFile = _file[indexes[i]];
                _file.Remove(selectedFile);
                _file.Insert(_file.Count + 1 - (indexes.Length - i), selectedFile);
            }

            _listView.EndUpdate();
        }

        private void RemoveItems(ListView _listView, List<FileInfo> _file)
        {
            if (_listView.SelectedItems.Count == 0)
                return;

            _listView.BeginUpdate();

            int[] indexes = _listView.SelectedIndices.Cast<int>().ToArray();

            for (int i = indexes.Length - 1; i >= 0; i--)
            {
                // Remove in ListView
                _listView.Items.RemoveAt(indexes[i]);
                // Remove in file list
                _file.RemoveAt(indexes[i]);
            }

            _listView.EndUpdate();
        }

        private bool CheckDuplicate(List<FileInfo> _file)
        {
            for (int i = 0; i < _file.Count - 1; i++)
            {
                for (int j = i + 1; j < _file.Count; j++)
                {
                    string temp = _file[i].Name.Substring(0, _file[i].Name.LastIndexOf('.'));
                    string next = _file[j].Name.Substring(0, _file[j].Name.LastIndexOf('.'));

                    if (temp.Equals(next))
                    {
                        string message = string.Format("{0} 목록에 같은 이름이 있습니다.\n{1}\n{2}", renameTarget == Target.Subtitle ? "자막" : "비디오", _file[i].Name, _file[j].Name);
                        MessageBox.Show(message, "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return true;
                    }
                }
            }
            return false;
        }
        private void btnRename_Click(object sender, EventArgs e)
        {
            if (listView1.Items.Count == 0)
            {
                const string message = "비디오 목록에 파일이 없습니다.";
                MessageBox.Show(message, "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (listView2.Items.Count == 0)
            {
                const string message = "자막 목록에 파일이 없습니다.";
                MessageBox.Show(message, "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (listView1.Items.Count != listView2.Items.Count)
            {
                const string message = "선택한 비디오와 자막 파일의 갯수가 다릅니다.";
                MessageBox.Show(message, "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // check duplicated name in subtitle list(when rename target is video)
            // ex) if there's e03.smi, e03.srt in subtitle list, it will make two e03.mp4 file <- error
            if(renameTarget == Target.Video)
            {
                if (CheckDuplicate(subtitleFiles))
                    return;
            }
            // check duplicated name in video list(when rename target is subtitle)
            else if (renameTarget == Target.Subtitle)
            {
                if (CheckDuplicate(videoFiles))
                    return;
            }

            PreviewForm confirmForm = new PreviewForm(videoFiles, subtitleFiles);
            confirmForm.Owner = this;
            confirmForm.ShowDialog();

            // refresh lists after rename
            OpenPath(path);
        }

        private void OnAndOffMoveButtons(ListView _listView)
        {
            // init
            tsbToTop.Enabled = true;
            tsbUp.Enabled = true;
            tsbDown.Enabled = true;
            tsbToBottom.Enabled = true;
            tsbRemove.Enabled = true;
            moveItemsToTopToolStripMenuItem.Enabled = true;
            moveItemsUpToolStripMenuItem.Enabled = true;
            moveItemsDownToolStripMenuItem.Enabled = true;
            moveItemsToBottomToolStripMenuItem.Enabled = true;
            deleteItemsToolStripMenuItem.Enabled = true;

            if (_listView.SelectedItems.Count == 0)
            {
                tsbToTop.Enabled = false;
                tsbUp.Enabled = false;
                tsbDown.Enabled = false;
                tsbToBottom.Enabled = false;
                tsbRemove.Enabled = false;
                moveItemsToTopToolStripMenuItem.Enabled = false;
                moveItemsUpToolStripMenuItem.Enabled = false;
                moveItemsDownToolStripMenuItem.Enabled = false;
                moveItemsToBottomToolStripMenuItem.Enabled = false;
                deleteItemsToolStripMenuItem.Enabled = false;
                return;
            }

            if (_listView.SelectedItems.Count == _listView.Items.Count)
            {
                tsbToTop.Enabled = false;
                tsbUp.Enabled = false;
                tsbDown.Enabled = false;
                tsbToBottom.Enabled = false;
                moveItemsToTopToolStripMenuItem.Enabled = false;
                moveItemsUpToolStripMenuItem.Enabled = false;
                moveItemsDownToolStripMenuItem.Enabled = false;
                moveItemsToBottomToolStripMenuItem.Enabled = false;
                return;
            }

            int[] indexes = _listView.SelectedIndices.Cast<int>().ToArray();
            bool isOnTop = false;
            bool isOnBottom = false;

            for (int i = 0; i < indexes.Length; i++)
            {
                if (indexes[i] == i)
                    isOnTop = true;
                else
                    isOnTop = false;
            }
            tsbUp.Enabled = isOnTop ? false : true;
            tsbToTop.Enabled = isOnTop ? false : true;
            moveItemsToTopToolStripMenuItem.Enabled = isOnTop ? false : true;
            moveItemsUpToolStripMenuItem.Enabled = isOnTop ? false : true;

            for (int i = indexes.Length - 1; i >= 0; i--)
            {
                if (indexes[i] == _listView.Items.Count - 1 - (indexes.Length - 1 - i))
                    isOnBottom = true;
                else
                    isOnBottom = false;
            }
            tsbDown.Enabled = isOnBottom ? false : true;
            tsbToBottom.Enabled = isOnBottom ? false : true;
            moveItemsDownToolStripMenuItem.Enabled = isOnBottom ? false : true;
            moveItemsToBottomToolStripMenuItem.Enabled = isOnBottom ? false : true;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 0)
            {
                renameTarget = Target.Subtitle;
                pictureBox1.Image = Resources.pointing_right;
                toolStripStatusLabel1.Text = "비디오 파일명을 따라 자막의 파일명을 바꿉니다.";

            }
            else if (comboBox1.SelectedIndex == 1)
            {
                renameTarget = Target.Video;
                pictureBox1.Image = Resources.pointing_left;
                toolStripStatusLabel1.Text = "자막 파일명을 따라 비디오의 파일명을 바꿉니다.";
            }
        }

        private void listView1_Enter(object sender, EventArgs e)
        {
            listView2.SelectedItems.Clear();
        }

        private void listView2_Enter(object sender, EventArgs e)
        {
            listView1.SelectedItems.Clear();
        }

        private void tsbToTop_Click(object sender, EventArgs e)
        {
            MoveToTop(listView1, videoFiles);
            MoveToTop(listView2, subtitleFiles);
        }

        private void tsbUp_Click(object sender, EventArgs e)
        {
            MoveUpItems(listView1, videoFiles);
            MoveUpItems(listView2, subtitleFiles);
        }

        private void tsbDown_Click(object sender, EventArgs e)
        {
            MoveDownItems(listView1, videoFiles);
            MoveDownItems(listView2, subtitleFiles);
        }

        private void tsbToBottom_Click(object sender, EventArgs e)
        {
            MoveToBottom(listView1, videoFiles);
            MoveToBottom(listView2, subtitleFiles);
        }

        private void tsbRemove_Click(object sender, EventArgs e)
        {
            RemoveItems(listView1, videoFiles);
            RemoveItems(listView2, subtitleFiles);
            UpdateCounterLabels();
        }

        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            OnAndOffMoveButtons(listView1);
        }

        private void listView2_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            OnAndOffMoveButtons(listView2);
        }

        private void tsbRefresh_Click(object sender, EventArgs e)
        {
            if (path != null)
                OpenPath(path);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 0)
                comboBox1.SelectedIndex = 1;
            else
                comboBox1.SelectedIndex = 0;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void viewHelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/uiyoung/VidSub-Renamer#how-to-use");
        }

        private void aboutSubtitleRenamerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox about = new AboutBox();
            about.Show();
        }
    }
}
