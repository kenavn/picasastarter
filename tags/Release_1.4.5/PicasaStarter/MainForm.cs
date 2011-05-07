﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;                       // Added to be able to check if a directory exists

namespace PicasaStarter
{
    public partial class MainForm : Form
    {
        private Settings _settings;
        private string _appDataDir = "";
        private string _appSettingsDir = "";
        private bool _firstRun = false;
        
        public MainForm(Settings settings, string appDataDir, string appSettingsDir, bool firstRun)
        {
            InitializeComponent();

            _settings = settings;
            _appDataDir = appDataDir;
            _appSettingsDir = appSettingsDir;
            _firstRun = firstRun;
        }
      
        private void MainForm_Load(object sender, EventArgs e)
        {
            // Set version in title bar
            this.Text = this.Text + " " + System.Diagnostics.FileVersionInfo.GetVersionInfo(Application.ExecutablePath).FileVersion;

            // Initialise all controls on the screen with the proper data
            ReFillPicasaDBList(false);

            // If the saved defaultselectedDB is valid, select it in the list...
            int defaultSelectedDBIndex = listBoxPicasaDBs.FindStringExact(_settings.picasaDefaultSelectedDB);
            if (defaultSelectedDBIndex != ListBox.NoMatches)
                listBoxPicasaDBs.SelectedIndex = defaultSelectedDBIndex;

            if (_firstRun == true)
            {
                ShowHelp();
            }
        }

        public Settings Settings { get { return _settings; } 
            set 
            {
                _settings.picasaDBs = value.picasaDBs;
                _settings.picasaDefaultSelectedDB = value.picasaDefaultSelectedDB;
                _settings.PicasaExePath = value.PicasaExePath;
            } 
        }
        private void buttonBrowseDBBaseDir_Click(object sender, EventArgs e)
        {
            textBoxDBBaseDir.Text = AskDirectoryPath(_settings.picasaDBs[listBoxPicasaDBs.SelectedIndex].BaseDir);
        }

        private void listBoxPicasaDBs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxPicasaDBs.SelectedIndex < 0)
                return;
            if (listBoxPicasaDBs.SelectedIndex >= _settings.picasaDBs.Count)
            {
                MessageBox.Show("Invalid item choosen from the database list");
                return;
            }

            textBoxDBName.Text = _settings.picasaDBs[listBoxPicasaDBs.SelectedIndex].Name;
            textBoxDBBaseDir.Text = _settings.picasaDBs[listBoxPicasaDBs.SelectedIndex].BaseDir;
            textBoxDBDescription.Text = _settings.picasaDBs[listBoxPicasaDBs.SelectedIndex].Description;
            textBoxDBFullDir.Text = SettingsHelper.GetFullDBDirectory(_settings.picasaDBs[listBoxPicasaDBs.SelectedIndex]);

            // If it is the default database, fields should be read-only!
            if (_settings.picasaDBs[listBoxPicasaDBs.SelectedIndex].IsStandardDB == true)
            {
                textBoxDBName.ReadOnly = true;
                textBoxDBDescription.ReadOnly = true;
                buttonBrowseDBBaseDir.Enabled = false;
                buttonRemoveDB.Enabled = false;
            }
            else
            {
                textBoxDBName.ReadOnly = false;
                textBoxDBDescription.ReadOnly = false;
                buttonBrowseDBBaseDir.Enabled = true;
                buttonRemoveDB.Enabled = true;
            }
        }

        private void textBoxDBName_TextChanged(object sender, EventArgs e)
        {
            if (listBoxPicasaDBs.SelectedIndex == -1
                || listBoxPicasaDBs.SelectedIndex >= _settings.picasaDBs.Count)
            {
                MessageBox.Show("Please choose a picasa database from the list first");
                return;
            }

            // if the User is changing the field, update the settings and the list as well...
            if (_settings.picasaDBs[listBoxPicasaDBs.SelectedIndex].Name != textBoxDBName.Text)
            {
                int selectedIndexBackup = listBoxPicasaDBs.SelectedIndex;
                _settings.picasaDBs[listBoxPicasaDBs.SelectedIndex].Name = textBoxDBName.Text;
                listBoxPicasaDBs.Items.RemoveAt(selectedIndexBackup);
                listBoxPicasaDBs.Items.Insert(selectedIndexBackup, textBoxDBName.Text);
                //ReFillPicasaDBList(false);
                listBoxPicasaDBs.SelectedIndex = selectedIndexBackup;
            }
        }

        private void textBoxDBDescription_TextChanged(object sender, EventArgs e)
        {
            if (listBoxPicasaDBs.SelectedIndex == -1
                || listBoxPicasaDBs.SelectedIndex >= _settings.picasaDBs.Count)
            {
                MessageBox.Show("Please choose a picasa database from the list first");
                return;
            }
            _settings.picasaDBs[listBoxPicasaDBs.SelectedIndex].Description = textBoxDBDescription.Text;
        }

        private void textBoxDBBaseDir_TextChanged(object sender, EventArgs e)
        {
            if (listBoxPicasaDBs.SelectedIndex == -1
                || listBoxPicasaDBs.SelectedIndex >= _settings.picasaDBs.Count)
            {
                MessageBox.Show("Please choose a picasa database from the list first");
                return;
            }
            _settings.picasaDBs[listBoxPicasaDBs.SelectedIndex].BaseDir = textBoxDBBaseDir.Text;
            textBoxDBFullDir.Text = SettingsHelper.GetFullDBDirectory(_settings.picasaDBs[listBoxPicasaDBs.SelectedIndex]);
        }

        private void buttonAddDB_Click(object sender, EventArgs e)
        {
            _settings.picasaDBs.Add(new PicasaDB("New"));
            ReFillPicasaDBList(true);
        }

        private void buttonRemoveDB_Click(object sender, EventArgs e)
        {
            if (listBoxPicasaDBs.SelectedIndex == -1
                    || listBoxPicasaDBs.SelectedIndex >= _settings.picasaDBs.Count)
            {
                MessageBox.Show("Please choose a picasa database from the list first");
                return;
            }
            if (_settings.picasaDBs[listBoxPicasaDBs.SelectedIndex].IsStandardDB == true)
            {
                MessageBox.Show("The default database Picasa creates for you in you user directory cannot be removed from the list...");
            }

            DialogResult result = MessageBox.Show("Remark: This won't delete the picasa database itself, it will only remove the entry from this list!!!\n\n"
                    + "If you als want to recuperate the (little) diskspace taken by the database, it is better to do this first.\n\n"
                    + "Click \"OK\" if you want to remove the entry from the list, \"Cancel\" to... cancel",
                "Do you want to do this?", MessageBoxButtons.OKCancel);
            if (result == DialogResult.OK)
            {
                _settings.picasaDBs.RemoveAt(listBoxPicasaDBs.SelectedIndex);
                ReFillPicasaDBList(false);
            }
        }

        private void buttonDBOpenFullDir_Click(object sender, EventArgs e)
        {
            if (listBoxPicasaDBs.SelectedIndex == -1
                    || listBoxPicasaDBs.SelectedIndex >= _settings.picasaDBs.Count)
            {
                MessageBox.Show("Please choose a picasa database from the list first");
                return;
            }

            string DBFullDir = SettingsHelper.GetFullDBDirectory(_settings.picasaDBs[listBoxPicasaDBs.SelectedIndex]);

            try
            {
                Directory.CreateDirectory(DBFullDir);
                System.Diagnostics.Process.Start(DBFullDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message + ", when trying to open directory: " + DBFullDir);
            }
        }

        private string AskDirectoryPath(string InitialDirectory)
        {
            FolderBrowserDialog fd = new FolderBrowserDialog();
            fd.ShowNewFolderButton = true;
            fd.SelectedPath = InitialDirectory;

            if (fd.ShowDialog() == DialogResult.OK)
                return fd.SelectedPath;
            else
                return InitialDirectory;
        }

        private void ReFillPicasaDBList(bool selectLastItem)
        {
            listBoxPicasaDBs.BeginUpdate();
            listBoxPicasaDBs.SelectedIndex = -1;
            listBoxPicasaDBs.Items.Clear();
            for (int i = 0; i < _settings.picasaDBs.Count; i++)
            {
                listBoxPicasaDBs.Items.Add(_settings.picasaDBs[i].Name);
            }

            if (listBoxPicasaDBs.Items.Count > 0)
            {
                if (selectLastItem == true)
                    listBoxPicasaDBs.SelectedIndex = listBoxPicasaDBs.Items.Count - 1;
                else
                    listBoxPicasaDBs.SelectedIndex = 0;
            }
            listBoxPicasaDBs.EndUpdate();
        }

        private void buttonGeneralSettings_Click(object sender, EventArgs e)
        {
            GeneralSettingsDialog generalSettingsDialog = new GeneralSettingsDialog(_appSettingsDir, _settings.PicasaExePath);
            DialogResult result = generalSettingsDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                if (generalSettingsDialog.ReturnPicasaSettings != null)
                {
                    Settings = generalSettingsDialog.ReturnPicasaSettings;
                    ReFillPicasaDBList(false);
                }
                _settings.PicasaExePath = generalSettingsDialog.ReturnPicasaExePath;
            }
        }

        private void buttonHelp_Click(object sender, EventArgs e)
        {
            ShowHelp();
        }

        private void buttonRunPicasa_Click(object sender, EventArgs e)
        {
            if (listBoxPicasaDBs.SelectedIndex == -1)
            {
                MessageBox.Show("Please choose a picasa database from the list first");
                return;
            }
            if (!Directory.Exists(_settings.picasaDBs[listBoxPicasaDBs.SelectedIndex].BaseDir))
            {
                MessageBox.Show("The base directory of this database doesn't exist or you didn't choose one yet.");
                return;
            }

            PicasaRunner runner = new PicasaRunner(_appDataDir, _settings.PicasaExePath);

            // If the user wants to run his personal default database... 
            if (_settings.picasaDBs[listBoxPicasaDBs.SelectedIndex].IsStandardDB == true)
                runner.RunPicasa(null);
            // If the user wants to run a custom database...
            else
                runner.RunPicasa(_settings.picasaDBs[listBoxPicasaDBs.SelectedIndex].BaseDir);
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ShowHelp()
        {
            HelpDialog help = new HelpDialog();
            help.ShowDialog();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if((listBoxPicasaDBs.SelectedIndex > -1)
                    && listBoxPicasaDBs.SelectedIndex < _settings.picasaDBs.Count)
            {
                _settings.picasaDefaultSelectedDB = _settings.picasaDBs[listBoxPicasaDBs.SelectedIndex].Name;
            }
        }
    }
}