using System;
using System.Windows.Forms;
using OQC_Check_App.Business;

namespace OQC_Check_App
{
    public partial class FormLogin : Form
    {
        public FormLogin()
        {
            InitializeComponent();
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
            if (!Ultils.IsNullOrEmpty(txtCode, errorProvider1))
            {
                return;
            }
            if (!Ultils.IsNullOrEmpty(txtPassword, errorProvider1))
            {
                return;
            }
            string username = txtCode.Text.Trim();
            string password = txtPassword.Text.Trim();
            var loginSucess = UmesHelper.Login(username, password);
            if (loginSucess)
            {
                this.Hide();
                new FormMain().ShowDialog();
            }
            else
            {
                lblMessage.Text = "Code không tồn tại. Vui lòng kiểm tra lại;";
                txtCode.SelectAll();
                txtCode.Focus();
                txtPassword.ResetText();
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
    }
}
