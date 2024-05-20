using System;
using System.ComponentModel;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Windows.Forms;
using AntdUI;
using OQC_Check_App.Business;

namespace OQC_Check_App
{
    public partial class FormLogin : Window
    {
        BackgroundWorker worker;
        public FormLogin()
        {
            InitializeComponent();
            worker = new BackgroundWorker();
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if((bool)e.Result)
            {
                this.Hide();
                new FormMain().ShowDialog();
                this.Close();
            }
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BeginInvoke(new Action(() =>
            {
                btnLogin.Loading = true;
                btnLogin.Text = "Login...";
            }));

            if (!Ultils.IsNullOrEmpty(txtCode, errorProvider1))
            {
                BeginInvoke(new Action(() =>
                {
                    btnLogin.Loading = false;
                    btnLogin.Text = "Login";
                }));
                return;
            }
            if (!Ultils.IsNullOrEmpty(txtPassword, errorProvider1))
            {
                BeginInvoke(new Action(() =>
                {
                    btnLogin.Loading = false;
                    btnLogin.Text = "Login";
                }));
                
                return;
            }
            string username = txtCode.Text.Trim();
            string password = txtPassword.Text.Trim();
            var loginSucess = UmesHelper.Login(username, password);
            if (loginSucess)
            {
                Thread.Sleep(1000);
                e.Result = true;
                
            }
            else
            {
                BeginInvoke(new Action(() =>
                {
                    btnLogin.Loading = false;
                    btnLogin.Text = "Login";
                    lblMessage.Text = "Code không tồn tại. Vui lòng kiểm tra lại;";
                    txtCode.SelectAll();
                    txtCode.Focus();
                    txtPassword.ResetText();
                }));
            }
            //var user = Services.HelperSingleton.GetUser(username, password);
            //if (user != null)
            //{
            //    Runtime.SetUserLogin(user);
            //    this.Hide();
            //    new FormMain().ShowDialog();
            //}
            //else
            //{
            //    lblMessage.Text = "Code không tồn tại. Vui lòng kiểm tra lại;";
            //    txtCode.SelectAll();
            //    txtCode.Focus();
            //    txtPassword.ResetText();
            //}
        }

        private void txtCode_TextChanged(object sender, EventArgs e)
        {
            Ultils.IsNullOrEmpty(txtCode, errorProvider1);
        }

        private void txtPassword_TextChanged(object sender, EventArgs e)
        {
            Ultils.IsNullOrEmpty(txtPassword, errorProvider1);
        }

        private void txtPassword_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (Ultils.IsNullOrEmpty(txtCode) && Ultils.IsNullOrEmpty(txtPassword))
                {
                    btnLogin.PerformClick();
                }
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            worker.RunWorkerAsync();
        }

        private void txtCode_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (!string.IsNullOrEmpty(txtCode.Text))
                {
                    txtPassword.Focus();
                }
            }
        }

        private void FormLogin_Load(object sender, EventArgs e)
        {

        }
    }
}
