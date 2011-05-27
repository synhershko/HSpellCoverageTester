using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using HebMorph.CorpusReaders;
using HebMorph.CorpusReaders.Common;
using HebMorph.CorpusReaders.Wikipedia;

namespace HSpellCoverageTester
{
    public partial class MainForm : Form
    {
        private Thread workerThread;
        /// <summary>
        /// Handles the ProgressChanged event from indexers
        /// </summary>
        /// <param name="sender">Indexer</param>
        /// <param name="e">Progress event</param>
        private delegate void ProgressChangedDelegate(object sender, ProgressChangedEventArgs e);

        private CoverageTester coverageTester;

        public MainForm()
        {
            InitializeComponent();
        }

        private static string SelectHSpellFolderPath()
        {
            var fbd = new FolderBrowserDialog();

            // Help locating the hspell-data-files folder
            string exeFile = (new System.Uri(System.Reflection.Assembly.GetEntryAssembly().CodeBase)).AbsolutePath;
            fbd.SelectedPath = System.IO.Path.GetDirectoryName(exeFile);
            fbd.ShowNewFolderButton = false;
            DialogResult dr = fbd.ShowDialog();
            if (dr != DialogResult.OK)
                return null;

            return fbd.SelectedPath;
        }

        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            string path = SelectHSpellFolderPath();
            if (!string.IsNullOrEmpty(path))
                txbHSpellPath.Text = path;
        }

        private void btnSelectCorpusWikiDump_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            
            DialogResult dr = ofd.ShowDialog();
            if (dr == DialogResult.OK && !string.IsNullOrEmpty(ofd.FileName))
            {
                txbCorpusPath.Text = ofd.FileName;
            }
        }

        private void btnSelectReportPath_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog();
            var dr = sfd.ShowDialog();
            if (dr == DialogResult.OK && !string.IsNullOrEmpty(sfd.FileName))
            {
                txbReportPath.Text = sfd.FileName;
            }
        }

        private void btnExecute_Click(object sender, EventArgs e)
        {
            if (workerThread != null)
            {
                btnExecute.Enabled = false;
                coverageTester.Abort();
            }
            else
            {
                if (string.IsNullOrEmpty(txbHSpellPath.Text) || string.IsNullOrEmpty(txbCorpusPath.Text))
                {
                    MessageBox.Show("Valid paths are required");
                    return;
                }

                if (!Directory.Exists(txbHSpellPath.Text) || !(Directory.Exists(txbCorpusPath.Text) || File.Exists(txbCorpusPath.Text)))
                {
                    MessageBox.Show("Valid paths are required");
                    return;
                }

                panel1.Enabled = false;
                lblStatus.Show();
                progressBar1.Show();
                btnExecute.Text = "Stop";
                workerThread = new Thread(delegate()
                {
                    ICorpusReader cr = new WikiDumpReader(txbCorpusPath.Text);
                    coverageTester = new CoverageTester(cr, txbHSpellPath.Text);
                    coverageTester.ComputeCoverage = chbComputeCoverage.Checked;
                    coverageTester.ProgressChanged += OnProgressChanged;
                    coverageTester.Run(txbReportPath.Text);
                });
                workerThread.Start();
            }
        }

        private void OnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Invoke(new ProgressChangedDelegate(UpdateProgress), sender, e);
        }

        private void UpdateProgress(object sender, ProgressChangedEventArgs e)
        {
            var pi = (ProgressInfo)e.UserState;
            if (!pi.IsStillRunning)
            {
                workerThread.Abort();
                workerThread = null;
                coverageTester = null;

                panel1.Enabled = true;
                progressBar1.Hide();
                lblStatus.Hide();
                btnExecute.Text = "Start";
                btnExecute.Enabled = true;

                return;
            }

            progressBar1.Value = e.ProgressPercentage;
            if (!string.IsNullOrEmpty(pi.Status))
                lblStatus.Text = pi.Status;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (coverageTester!=null)
                coverageTester.Abort();
            if (workerThread != null)
            {
                workerThread.Abort();
                workerThread = null;
            }
            coverageTester = null;

            base.OnClosing(e);
        }
    }
}