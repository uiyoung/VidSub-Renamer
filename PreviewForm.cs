using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace VidSubRenamer
{
    public partial class PreviewForm : Form
    {
        private List<FileInfo> videoFiles, subtitleFiles;
        private string tempDirName;
        private int skipCount, renameCount, overwriteCount;
        //private enum Target { Subtitle, Video }
        //Target renameTarget;

        //public PreviewForm(List<FileInfo> _videoFiles, List<FileInfo> _subtitleFiles, int _renameTarget)
        //{
        //    InitializeComponent();

        //    videoFiles = _videoFiles;
        //    subtitleFiles = _subtitleFiles;
        //    renameTarget = (Target)_renameTarget;
        //}

        public PreviewForm(List<FileInfo> _videoFiles, List<FileInfo> _subtitleFiles)
        {
            InitializeComponent();

            videoFiles = _videoFiles;
            subtitleFiles = _subtitleFiles;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            this.KeyPreview = true;
            linkLabel1.Text = MainForm.path;

            if (MainForm.renameTarget == Target.Subtitle)
            {
                ListRenames(subtitleFiles, videoFiles);
                lblConfirmMessage.Text = "위와 같이 자막의 파일명을 변경할까요?";
            }
            else if (MainForm.renameTarget == Target.Video)
            {
                ListRenames(videoFiles, subtitleFiles);
                lblConfirmMessage.Text = "위와 같이 비디오의 파일명을 변경할까요?";
            }

            // reset
            tempDirName = "_working_";
            skipCount = 0;
            renameCount = 0;
            overwriteCount = 0;
        }

        private void ListRenames(List<FileInfo> _original, List<FileInfo> _new)
        {
            for (int i = 0; i < _original.Count; i++)
            {
                string oldName = _original[i].Name;
                string newName = _new[i].Name.Substring(0, _new[i].Name.LastIndexOf('.'));
                string ext = _original[i].Extension;

                ListViewItem lvi = new ListViewItem(oldName);
                lvi.SubItems.Add(newName + ext);

                if (oldName.Equals(newName + ext))
                    lvi.SubItems.Add("skip");
                else
                {
                    lvi.SubItems.Add("rename");
                    lvi.BackColor = Color.LightPink;
                }
                listView1.Items.Add(lvi);
            }
        }

        private void MoveFilesToTempDir(List<FileInfo> _files)
        {
            int idx = 0;
            while (Directory.Exists(MainForm.path + "\\" + tempDirName + idx))
            {
                idx++;
            }
            tempDirName += idx;

            Directory.CreateDirectory(MainForm.path + "\\" + tempDirName);

            // move files to temp directory
            foreach (var item in _files)
            {
                item.MoveTo(MainForm.path + "\\" + tempDirName + "\\" + item.Name);
            }
        }

        private void RenameFiles(List<FileInfo> _target, List<FileInfo> _to)
        {
            for (int i = 0; i < _target.Count; i++)
            {
                string newName = _to[i].Name.Substring(0, _to[i].Name.LastIndexOf('.'));
                string ext = _target[i].Extension;

                // if there's a file with a new name already, replace it from temp directory or just skip
                if (File.Exists(MainForm.path + "\\" + newName + ext))
                {
                    string existMessage = newName + ext + "파일이 이미 존재합니다. 덮어 쓸까요?";
                    var existResult = MessageBox.Show(existMessage, "warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (existResult == DialogResult.Yes)
                    {
                        File.Delete(MainForm.path + "\\" + newName + ext);
                        _target[i].MoveTo(MainForm.path + "\\" + newName + ext);
                        overwriteCount++;
                    }
                    else
                    {
                        skipCount++;
                        continue;
                    }
                }
                else
                {
                    try
                    {
                        if (newName.Equals(_target[i].Name.Substring(0, _target[i].Name.LastIndexOf('.'))))
                            skipCount++;
                        else
                            renameCount++;

                        _target[i].MoveTo(MainForm.path + "\\" + newName + ext);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                        throw;
                    }
                }
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (MainForm.renameTarget == Target.Subtitle)
            {
                // create temp directory and move subtitle files
                MoveFilesToTempDir(subtitleFiles);
                // rename subtitle files
                RenameFiles(subtitleFiles, videoFiles);
            }
            else if (MainForm.renameTarget == Target.Video)
            {
                // create temp directory and move video files
                MoveFilesToTempDir(videoFiles);
                // rename video files
                RenameFiles(videoFiles, subtitleFiles);
            }

            // delete temp directory after jobs done
            Directory.Delete(MainForm.path + "\\" + tempDirName);

            // result message
            string successMessage = string.Format("success\n\nskip:{0}, rename:{1}, overwrite:{2}", skipCount, renameCount, overwriteCount);
            MessageBox.Show(successMessage, ":)", MessageBoxButtons.OK, MessageBoxIcon.Information);

            this.Close();

            // open explorer
            if (checkBox1.Checked)
                System.Diagnostics.Process.Start(MainForm.path);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(MainForm.path);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form2_KeyPress(object sender, KeyPressEventArgs e)
        {
            const char ESC_KEY = (char)Keys.Escape;
            if (e.KeyChar == ESC_KEY)
                this.Close();
        }
    }
}
