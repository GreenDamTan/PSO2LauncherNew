﻿using System;
using System.Collections.Generic;
using PSO2ProxyLauncherNew.Classes.Infos;
using PSO2ProxyLauncherNew.Classes.Components.WebClientManger;
using PSO2ProxyLauncherNew.Classes.Events;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading;
using MetroFramework;
using MetroFramework.Forms;
using PSO2ProxyLauncherNew.Classes;

namespace PSO2ProxyLauncherNew.Forms
{
    public partial class MyMainMenu : MetroForm
    {
        private SynchronizationContext SyncContext;
        private BackgroundWorker bWorker_tweakerWebBrowser_load;
        private BackgroundWorker bWorker_Boot;
        private Classes.Components.PSO2Controller _pso2controller;
        private Classes.Components.SelfUpdate _selfUpdater;
        Classes.Components.DirectBitmap bgImage;

        public MyMainMenu()
        {
            InitializeComponent();
            this.SyncContext = SynchronizationContext.Current;
            this.Icon = Properties.Resources._1;

            //Myself
            this._selfUpdater = new Classes.Components.SelfUpdate(this.SyncContext);
            this._selfUpdater.UpdaterUri = new Uri(DefaultValues.MyServer.Web.SelfUpdate_UpdaterUri);
            this._selfUpdater.UpdateUri = new Uri(DefaultValues.MyServer.Web.SelfUpdate_UpdateUri);
            this._selfUpdater.VersionUri = new Uri(DefaultValues.MyServer.Web.SelfUpdate_VersionUri);
            this._selfUpdater.ProgressChanged += _selfUpdater_ProgressChanged;
            this._selfUpdater.HandledException += _selfUpdater_HandledException;
            this._selfUpdater.FoundNewVersion += _selfUpdater_FoundNewVersion;
            this._selfUpdater.CurrentStepChanged += _selfUpdater_CurrentStepChanged;
            this._selfUpdater.CheckCompleted += _selfUpdater_CheckCompleted;
            this._selfUpdater.BeginDownloadPatch += _selfUpdater_BeginDownloadPatch;

            //BackgroundWorker for tweakerWebBrowser Load
            this.bWorker_Boot = new BackgroundWorker();
            this.bWorker_Boot.WorkerSupportsCancellation = false;
            this.bWorker_Boot.WorkerReportsProgress = false;
            this.bWorker_Boot.DoWork += BWorker_Boot_DoWork;
            this.bWorker_Boot.RunWorkerCompleted += BWorker_Boot_RunWorkerCompleted;

            //BackgroundWorker for tweakerWebBrowser Load
            this.bWorker_tweakerWebBrowser_load = new BackgroundWorker();
            this.bWorker_tweakerWebBrowser_load.WorkerSupportsCancellation = false;
            this.bWorker_tweakerWebBrowser_load.WorkerReportsProgress = false;
            this.bWorker_tweakerWebBrowser_load.DoWork += BWorker_tweakerWebBrowser_load_DoWork;
            this.bWorker_tweakerWebBrowser_load.RunWorkerCompleted += BWorker_tweakerWebBrowser_load_RunWorkerCompleted;

            this._pso2controller = CreatePSO2Controller();

            Bitmap asfas = PSO2ProxyLauncherNew.Properties.Resources._bgimg;
            this.bgImage = new Classes.Components.DirectBitmap(asfas.Width, asfas.Height);
            this.bgImage.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            this.bgImage.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            this.bgImage.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            //this.bgImage.Graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
            this.bgImage.Graphics.DrawImageUnscaled(asfas, 0, 0);
            //db.Bitmap.MakeTransparent(Color.Black);
            //panel1 .i.SizeMode = PictureBoxSizeMode.Zoom;//*/
            panel1.BackgroundImage = this.bgImage.Bitmap;

            Classes.Components.AbstractExtractor.SetSyncContext(this.SyncContext);
        }

        #region "SelfUpdate"
        private void _selfUpdater_CheckCompleted(object sender, EventArgs e)
        {
            this.ChangeProgressBarStatus(ProgressBarVisibleState.Infinite);
            this.bWorker_Boot.RunWorkerAsync();
        }

        private void _selfUpdater_CurrentStepChanged(object sender, StepEventArgs e)
        {
            this.PrintText(e.Step);
        }

        private void _selfUpdater_FoundNewVersion(object sender, Classes.Components.SelfUpdate.NewVersionEventArgs e)
        {
            if (MetroMessageBox.Show(this, string.Format(LanguageManager.GetMessageText("SelfUpdater_PromptToUpdateMsg", "Found new {0} version {1}.\nDo you want to update?\n\nYes=Update (Yes, please)\nNo=Skip (STRONGLY NOT RECOMMENDED)"), MyApp.AssemblyInfo.AssemblyName, e.Version), "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) != DialogResult.Yes)
            {
                e.AllowUpdate = false;
                this.ChangeProgressBarStatus(ProgressBarVisibleState.Infinite);
                this.bWorker_Boot.RunWorkerAsync();
            }
        }

        private void _selfUpdater_HandledException(object sender, HandledExceptionEventArgs e)
        {
            this.PrintText(e.Error.Message, Classes.Controls.RtfColor.Red);
            MetroMessageBox.Show(this, e.Error.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error , MessageBoxDefaultButton.Button1);
            this.Close();
        }

        private void _selfUpdater_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.mainProgressBar.Value = e.ProgressPercentage;
        }

        private void _selfUpdater_BeginDownloadPatch(object sender, EventArgs e)
        {
            this.mainProgressBar.Maximum = 100;
        }
        #endregion

        #region "English Patch"
        private void PSO2Controller_EnglishPatchNotify(object sender, PatchNotifyEventArgs e)
        {
            this.EnglishPatchButton.Text = "English Patch: " + e.PatchVer;
            if (e.PatchVer.ToLower() == DefaultValues.AIDA.Tweaker.Registries.NoPatchString.ToLower())
                this.EnglishPatchButton.FlatAppearance.BorderColor = Color.Red;
            else
                this.EnglishPatchButton.FlatAppearance.BorderColor = Color.Green;
        }
        private void EnglishPatchButton_Click(object sender, EventArgs e)
        {
            Control myself = sender as Control;
            this.englishPatchContext.Tag = Classes.Components.Task.EnglishPatch;
            this.englishPatchContext.Show(myself, 0, myself.Height);
        }
        #endregion

        #region "LargeFiles Patch"
        private void PSO2Controller_LargeFilesPatchNotify(object sender, PatchNotifyEventArgs e)
        {
            this.LargeFilesPatchButton.Text = "LargeFiles Patch: " + e.PatchVer;
            if (e.PatchVer.ToLower() == DefaultValues.AIDA.Tweaker.Registries.NoPatchString.ToLower())
                this.LargeFilesPatchButton.FlatAppearance.BorderColor = Color.Red;
            else
                this.LargeFilesPatchButton.FlatAppearance.BorderColor = Color.Green;
        }
        private void LargeFilesPatchButton_Click(object sender, EventArgs e)
        {
            Control myself = sender as Control;
            this.englishPatchContext.Tag = Classes.Components.Task.LargeFilesPatch;
            this.englishPatchContext.Show(myself, 0, myself.Height);
        }
        #endregion

        #region "Story Patch"
        private void StoryPatchButton_Click(object sender, EventArgs e)
        {
            Control myself = sender as Control;
            this.englishPatchContext.Tag = Classes.Components.Task.StoryPatch;
            this.englishPatchContext.Show(myself, 0, myself.Height);
        }

        private void PSO2Controller_StoryPatchNotify(object sender, PatchNotifyEventArgs e)
        {
            this.StoryPatchButton.Text = "Story Patch: " + e.PatchVer;
            if (e.PatchVer.ToLower() == DefaultValues.AIDA.Tweaker.Registries.NoPatchString.ToLower())
                this.StoryPatchButton.FlatAppearance.BorderColor = Color.Red;
            else
                this.StoryPatchButton.FlatAppearance.BorderColor = Color.Green;
        }
        #endregion

        #region "PSO2"
        private void Result_PSO2Installed(object sender, Classes.PSO2.PSO2UpdateManager.PSO2NotifyEventArgs e)
        {
            if (e.FailedList != null && e.FailedList.Count > 0)
            {
                if (e.Cancelled)
                    this.PrintText(string.Format(LanguageManager.GetMessageText("Mypso2updater_InstallationCancelled", "PSO2 client {0} has been cancelled. The download still have {1} files left."), e.NewClientVersion, e.FailedList.Count), Classes.Controls.RtfColor.Red);
                else
                    this.PrintText(string.Format(LanguageManager.GetMessageText("Mypso2updater_InstallationFailure", "PSO2 client version {0} has been downloaded but missing {1} files."), e.NewClientVersion, e.FailedList.Count), Classes.Controls.RtfColor.Red);
            }
            else
            {
                if (e.Installation)
                    this.PrintText(string.Format(LanguageManager.GetMessageText("Mypso2updater_InstalledSuccessfully", "PSO2 client version {0} has been installed successfully"), e.NewClientVersion));
                else
                    this.PrintText(string.Format(LanguageManager.GetMessageText("Mypso2updater_UpdatedSuccessfully", "PSO2 client has been updated to version {0} successfully"), e.NewClientVersion));
            }
        }

        private void Result_PSO2Launched(object sender, PSO2LaunchedEventArgs e)
        {
            if (e.Error != null)
                this.PrintText(string.Format(LanguageManager.GetMessageText("PSO2Launched_FailedToLaunch", "Failed to launch PSO2. Reason: {0}"), e.Error.Message), Classes.Controls.RtfColor.Red);
            else
            {
                this.PrintText(LanguageManager.GetMessageText("PSO2Launched_Launched", "GAME STARTED!!!"), Classes.Controls.RtfColor.Green);
                this.Close();
            }
        }
        #endregion

        #region "Handle Events"
        private void Result_HandledException(object sender, Classes.Components.PSO2Controller.PSO2HandledExceptionEventArgs e)
        {
            Classes.Log.LogManager.GeneralLog.Print(e.Error);
            if (e.LastTask == Classes.Components.Task.EnglishPatch)
                this.PrintText("[English Patch]" + e.Error.Message, Classes.Controls.RtfColor.Red);
            else if (e.LastTask == Classes.Components.Task.LargeFilesPatch)
                this.PrintText("[LargeFiles Patch]" + e.Error.Message, Classes.Controls.RtfColor.Red);
            else if (e.LastTask == Classes.Components.Task.StoryPatch)
                this.PrintText("[Story Patch]" + e.Error.Message, Classes.Controls.RtfColor.Red);
            else
                this.PrintText(e.Error.Message, Classes.Controls.RtfColor.Red);
        }
        private void Result_StepChanged(object sender, Classes.Components.PSO2Controller.StepChangedEventArgs e)
        {
            if (e.Final)
                this.PrintText(e.Step, Classes.Controls.RtfColor.Green);
            else
                this.PrintText(e.Step);
        }
        private void Result_ProgressBarStateChanged(object sender, ProgressBarStateChangedEventArgs e)
        {
            this.ChangeProgressBarStatus(e.ProgressBarState, e.Properties);
        }
        #endregion

        #region "Form Codes"
        public new void Dispose()
        {
            panel1.BackgroundImage = null;
            if (this.bgImage != null)
                this.bgImage.Dispose();
            base.Dispose();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            if (MetroMessageBox.Show(this, LanguageManager.GetMessageText("MyMainMenu_CancelConfirm", "Are you sure you want to cancel current operation?.\nThis is highly NOT RECOMMENDED."), "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                this._pso2controller.CancelOperation();
        }

        private void ChangeProgressBarStatus(ProgressBarVisibleState val)
        { this.ChangeProgressBarStatus(val, null); }
        private void ChangeProgressBarStatus(ProgressBarVisibleState val, object _properties)
        {
            switch (val)
            {
                case ProgressBarVisibleState.Percent:
                    if (_properties != null && _properties is CircleProgressBarProperties)
                    {
                        var asdasdasd = _properties as CircleProgressBarProperties;
                        mainProgressBar.ShowSmallText = asdasdasd.ShowSmallText;
                    }
                    this.ProgressBarPercent_Visible(true);
                    this.ProgressBarInfinite_Visible(false);
                    if (this.buttonCancel.Tag is string)
                        this.buttonCancel_Visible(true);
                    break;
                case ProgressBarVisibleState.Infinite:
                    this.ProgressBarPercent_Visible(false);
                    mainProgressBar.ShowSmallText = false;
                    this.ProgressBarInfinite_Visible(true);
                    if (this.buttonCancel.Tag is string)
                        this.buttonCancel_Visible(true);
                    break;
                default:
                    this.ProgressBarPercent_Visible(false);
                    mainProgressBar.ShowSmallText = false;
                    this.buttonCancel_Visible(false);
                    this.ProgressBarInfinite_Visible(false);
                    break;
            }
        }

        private void buttonCancel_Visible(bool myBool)
        {
            buttonCancel.Visible = myBool;
            if (myBool)
                buttonCancel.BringToFront();
            else
                buttonCancel.SendToBack();
        }

        private void ProgressBarPercent_Visible(bool myBool)
        {
            mainProgressBar.Visible = myBool;
            if (myBool)
                mainProgressBar.BringToFront();
            else
                mainProgressBar.SendToBack();
        }
        private void ProgressBarInfinite_Visible(bool myBool)
        {
            mainFormLoadingHost.Visible = myBool;
            if (myBool)
                mainFormLoadingHost.BringToFront();
            else
                mainFormLoadingHost.SendToBack();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            //AeroControl.EnableBlur(this);
            this.mainFormLoading.SetRingColor(Color.DarkRed);
            Classes.LanguageManager.TranslateForm(this);
        }

        private void Form_Shown(object sender, EventArgs e)
        {
            this.ChangeProgressBarStatus(ProgressBarVisibleState.Infinite);
            this._selfUpdater.CheckForUpdates();
            //this.bWorker_Boot.RunWorkerAsync();
        }

        public void PrintText(string msg)
        {
            Classes.Log.LogManager.GetLogDefaultPath(DefaultValues.MyInfo.Filename.Log.PrintOut, string.Empty, false).Print(msg);
            this.LogRichTextBox.AppendText(msg);
        }
        public void PrintText(string msg, Classes.Controls.RtfColor textColor)
        {
            Classes.Log.LogManager.GetLogDefaultPath(DefaultValues.MyInfo.Filename.Log.PrintOut, string.Empty, false).Print(msg);
            this.LogRichTextBox.AppendText(msg, textColor);
        }
        private Classes.Components.PSO2Controller CreatePSO2Controller()
        {
            Classes.Components.PSO2Controller result = new Classes.Components.PSO2Controller(this.SyncContext);
            result.HandledException += Result_HandledException;
            result.ProgressBarStateChanged += Result_ProgressBarStateChanged;
            result.StepChanged += Result_StepChanged;
            result.CurrentProgressChanged += Result_CurrentProgressChanged;
            result.CurrentTotalProgressChanged += Result_CurrentTotalProgressChanged;

            result.PSO2Installed += Result_PSO2Installed;
            result.PSO2Launched += Result_PSO2Launched;
            result.EnglishPatchNotify += PSO2Controller_EnglishPatchNotify;
            result.LargeFilesPatchNotify += PSO2Controller_LargeFilesPatchNotify;
            result.StoryPatchNotify += PSO2Controller_StoryPatchNotify;
            return result;
        }

        private void Result_CurrentTotalProgressChanged(object sender, ProgressEventArgs e)
        {
            
            this.mainProgressBar.Maximum = e.Progress;
        }

        private void Result_CurrentProgressChanged(object sender, ProgressEventArgs e)
        {
            if (e.Progress <= this.mainProgressBar.Maximum)
                this.mainProgressBar.Value = e.Progress;
        }

        private void installToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!this._pso2controller.IsBusy)
            {
                Classes.Components.Task currentTask = (Classes.Components.Task)this.englishPatchContext.Tag;
                switch (currentTask)
                {
                    case Classes.Components.Task.EnglishPatch:
                        if (MetroMessageBox.Show(this, Classes.LanguageManager.GetMessageText("AskInstallEnglish", "Do you want to create backups and install the English Patch ?"), "Notice", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            this.EnglishPatchButton.FlatAppearance.BorderColor = Color.Yellow;
                            this.EnglishPatchButton.Text = "English Patch: Installing";
                            this._pso2controller.InstallEnglishPatch();
                        }
                        break;
                    case Classes.Components.Task.LargeFilesPatch:
                        if (MetroMessageBox.Show(this, Classes.LanguageManager.GetMessageText("AskInstallLargeFiles", "Do you want to create backups and install the LargeFiles Patch ?"), "Notice", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            this.LargeFilesPatchButton.FlatAppearance.BorderColor = Color.Yellow;
                            this.LargeFilesPatchButton.Text = "LargeFiles Patch: Installing";
                            this._pso2controller.InstallLargeFilesPatch();
                        }
                        break;
                    case Classes.Components.Task.StoryPatch:
                        if (MetroMessageBox.Show(this, Classes.LanguageManager.GetMessageText("AskInstallStory", "Do you want to create backups and install the Story Patch ?"), "Notice", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            this.StoryPatchButton.FlatAppearance.BorderColor = Color.Yellow;
                            this.StoryPatchButton.Text = "Story Patch: Installing";
                            this._pso2controller.InstallStoryPatch();
                        }
                        break;
                }
            }
        }

        private void uninstallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!this._pso2controller.IsBusy)
            {
                Classes.Components.Task currentTask = (Classes.Components.Task)this.englishPatchContext.Tag;
                switch (currentTask)
                {
                    case Classes.Components.Task.EnglishPatch:
                        if (MetroMessageBox.Show(this, Classes.LanguageManager.GetMessageText("AskUninstallEnglish", "Do you want to uninstall the English Patch ?"), "Notice", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            this.EnglishPatchButton.FlatAppearance.BorderColor = Color.Yellow;
                            this.EnglishPatchButton.Text = "English Patch: Uninstalling";
                            this._pso2controller.UninstallEnglishPatch();
                        }
                        break;
                    case Classes.Components.Task.LargeFilesPatch:
                        if (MetroMessageBox.Show(this, Classes.LanguageManager.GetMessageText("AskUninstallLargeFiles", "Do you want to uninstall the LargeFiles Patch ?"), "Notice", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            this.LargeFilesPatchButton.FlatAppearance.BorderColor = Color.Yellow;
                            this.LargeFilesPatchButton.Text = "LargeFiles Patch: Uninstalling";
                            this._pso2controller.UninstallLargeFilesPatch();
                        }
                        break;
                    case Classes.Components.Task.StoryPatch:
                        if (MetroMessageBox.Show(this, Classes.LanguageManager.GetMessageText("AskUninstallStory", "Do you want to uninstall the Story Patch ?"), "Notice", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            this.StoryPatchButton.FlatAppearance.BorderColor = Color.Yellow;
                            this.StoryPatchButton.Text = "Story Patch: Uninstalling";
                            this._pso2controller.UninstallStoryPatch();
                        }
                        break;
                }
            }
        }

        private void gameStartButton1_Click(object sender, EventArgs e)
        {
            this._pso2controller.LaunchPSO2Game();
        }
        #endregion

        #region "Startup Codes"
        private void BWorker_Boot_DoWork(object sender, DoWorkEventArgs e)
        {
            //Ping the 7z
            string libPath = DefaultValues.MyInfo.Filename.SevenZip.SevenZipLibPath;
            //this.PrintText(Classes.LanguageManager.GetMessageText("RARLibLoaded", "RAR library loaded successfully"));
            if (!DefaultValues.MyInfo.Filename.SevenZip.IsValid)
            {
                this.PrintText(Classes.LanguageManager.GetMessageText("InvalidSevenZipLib", "SevenZip library is invalid or not existed. Redownloading"));
                //WakeUpCall for 7z
                string url = DefaultValues.MyServer.Web.GetDownloadLink + "/" + System.IO.Path.GetFileNameWithoutExtension(DefaultValues.MyInfo.Filename.SevenZip.SevenZipLibName) + ".rar";
                WebClientPool.GetWebClient(DefaultValues.MyServer.Web.GetDownloadLink).DownloadFile(url, libPath + ".rar");
                using (SharpCompress.Archives.Rar.RarArchive libPathArchive = SharpCompress.Archives.Rar.RarArchive.Open(libPath + ".rar"))
                    Classes.Components.AbstractExtractor.Unrar(libPathArchive, MyApp.AssemblyInfo.DirectoryPath, null);
                try { System.IO.File.Delete(libPath + ".rar"); } catch { }
            }
            Classes.Components.AbstractExtractor.SetSevenZipLib(libPath);
            this.PrintText(Classes.LanguageManager.GetMessageText("SevenZipLibLoaded", "SevenZip library loaded successfully"), Classes.Controls.RtfColor.Green);
            
            //Ping AIDA for the server
            Classes.AIDA.GetIdeaServer();
            this.SyncContext?.Send(new SendOrPostCallback(delegate { this.refreshToolStripMenuItem.PerformClick(); }), null);

            var pso2versions = this._pso2controller.CheckForPSO2Updates();
            bool pso2update = false;
            if (pso2versions.IsNewVersionFound)
            {
                string pso2updater_FoundNewLatestVersion = string.Format(Classes.LanguageManager.GetMessageText("PSO2Updater_FoundNewLatestVersion", "Found new PSO2 client version: {0}.\nYour current version: {1}"), pso2versions.LatestVersion, pso2versions.CurrentVersion);
                this.PrintText(pso2updater_FoundNewLatestVersion);
                DialogResult pso2updateAnswer = DialogResult.No;
                this.SyncContext?.Send(new SendOrPostCallback(delegate { pso2updateAnswer = MetroMessageBox.Show(this, pso2updater_FoundNewLatestVersion + "\n" + Classes.LanguageManager.GetMessageText("PSO2Updater_ConfirmToUpdate", "Do you want to perform update now?"), "Notice", MessageBoxButtons.YesNo, MessageBoxIcon.Question); }), null);
                if (pso2updateAnswer == DialogResult.Yes)
                    pso2update = true;
            }
            else
                this.PrintText(string.Format(Classes.LanguageManager.GetMessageText("PSO2Updater_AlreadyLatestVersion", "PSO2 Client is already latest version: {0}"), pso2versions.CurrentVersion), Classes.Controls.RtfColor.Green);

            if (pso2update)
                this._pso2controller.NotifyPatches();
            else
            {
                var patchesversions = this._pso2controller.CheckForPatchesVersionsAndWait();
                
            }

            e.Result = new BootResult(pso2update);
        }

        private void BWorker_Boot_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Classes.Log.LogManager.GeneralLog.Print(e.Error);
                this.PrintText(e.Error.Message, Classes.Controls.RtfColor.Red);
                MetroMessageBox.Show(this, e.Error.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
            else
            {
                this.ChangeProgressBarStatus(ProgressBarVisibleState.None);
                this.buttonCancel.Tag = "R";
                //this._pso2controller.UpdatePSO2Client();
                if (e.Result is BootResult)
                {
                    BootResult br = e.Result as BootResult;
                    if (br.UpdatePSO2)
                        this._pso2controller.UpdatePSO2Client();
                }
            }
        }
        #endregion

        #region "Private Classes"
        private class BootResult
        {
            public bool UpdatePSO2 { get; }
            public BootResult(bool _updatepso2)
            {
                this.UpdatePSO2 = _updatepso2;
            }
        }

        public class CircleProgressBarProperties
        {
            public bool ShowSmallText { get; }
            public CircleProgressBarProperties(bool _showsmalltext)
            {
                this.ShowSmallText = _showsmalltext;
            }
        }

        public enum ProgressBarVisibleState : short
        {
            None,
            Percent,
            Infinite
        }
        #endregion

        #region "Tweaker Browser Methods"
        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.bWorker_tweakerWebBrowser_load.RunWorkerAsync();
        }

        private void BWorker_tweakerWebBrowser_load_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            tweakerWebBrowser_IsLoading(false);
        }

        private void BWorker_tweakerWebBrowser_load_DoWork(object sender, DoWorkEventArgs e)
        {
            this.tweakerWebBrowser.LockNavigate = false;
            tweakerWebBrowser_IsLoading(true);
            /*string linefiletoskip = Classes.AIDA.TweakerWebPanel.CutString;
            linefiletoskip = WebClientPool.GetWebClient(DefaultValues.MyServer.GetWebLink).DownloadString(DefaultValues.MyServer.GetWebLink + DefaultValues.MyServer.Web.TweakerSidePanelLiner);
            if (string.IsNullOrWhiteSpace(linefiletoskip))
                linefiletoskip = Classes.AIDA.TweakerWebPanel.CutString;*/
            string resultofgettinghtmlfile = WebClientPool.GetWebClient_AIDA().DownloadString(Classes.AIDA.TweakerWebPanel.InfoPageLink);
            if (string.IsNullOrEmpty(resultofgettinghtmlfile))
                this.tweakerWebBrowser.Navigate(Classes.AIDA.TweakerWebPanel.InfoPageLink);
            else
            {
                this.tweakerWebBrowser.LoadHTML(resultofgettinghtmlfile);
                //this.tweakerWebBrowser.EnglishPatchStatus = result.EnglishPatch;
                //this.tweakerWebBrowser.ItemPatchStatus = result.ItemPatch;
            }
            this.tweakerWebBrowser.LockNavigate = true;
        }

        private void tweakerWebBrowser_LockedNavigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            Thread launchWebThread = new Thread(new ParameterizedThreadStart(this.launchWeb));
            launchWebThread.IsBackground = true;
            launchWebThread.Start(e.Url);
        }

        private void launchWeb(object url)
        {
            try
            {
                Uri _uri = url as Uri;
                System.Diagnostics.Process.Start(_uri.OriginalString);
            }
            catch (Exception ex)
            {
                Classes.Log.LogManager.GeneralLog.Print(ex);
            }
        }

        public void tweakerWebBrowser_IsLoading(bool theBool)
        {
            this.SyncContext.Post(new System.Threading.SendOrPostCallback(this._tweakerWebBrowser_IsLoading), theBool as object);
        }
        private void _tweakerWebBrowser_IsLoading(object theboolean)
        {
            bool bo = Convert.ToBoolean(theboolean);
            this.tweakerWebBrowserLoading.Visible = bo;
            foreach (ToolStripMenuItem item in this.tweakerWebBrowserContextMenu.Items)
                item.Enabled = !bo;
        }


        #endregion
    }
}
