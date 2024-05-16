using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using OQC_Check_App;
using OQC_Check_App.Business;

namespace OQC_Check_App
{
    public partial class frmLockSystem : Form
    {
        string boardNo = null;
        public frmLockSystem()
        {
            InitializeComponent();
            Ultils.WriteRegistry("lock", "1");
        }
        public frmLockSystem(string message, string ope, string board)
        {
            InitializeComponent();
            boardNo = board;
        }

        private void FormError_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string id = txtId.Text;
            string password = txtPassword.Text;
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(password))
            {
                return;
            }
            if (UmesHelper.Unlock(id, password))
            {
                this.Dispose();
                this.Close();
                Ultils.WriteRegistry("lock", "0");
            }
            else
            {
                this.lblErr.Text = "Tài khoản không hợp lệ!";
                // MessageBox.Show("Tài khoản không hợp lệ!", "THÔNG BÁO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            //if (!string.IsNullOrEmpty(leader) &&
            //    !string.IsNullOrEmpty(password))
            //{
            //    var checkLeader = leaderConfirmsBLL.CheckLeaderConfirm(leader, password);
            //    if (checkLeader != null)
            //    {
            //        var log = new ErrorLogs()
            //        {
            //            LeaderConfirm = checkLeader.FullName,
            //            Date = USERS_BLL.GetDateTime(),
            //            BoardNo = boardNo,
            //            Customer = "Murata"
            //        };

            //        try
            //        {
            //            leaderConfirmsBLL.Insert(log);
            //            this.Dispose();
            //            this.Close();
            //        }
            //        catch (Exception)
            //        {

            //        }
            //    }
            //    else
            //    {
            //        MessageBox.Show("Tên đăng nhập hoặc Mật khẩu không chính xác. Vui lòng nhập kiểm tra lại!", "THÔNG BÁO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //        txtLeader.Focus();
            //        return;
            //    }
            //}
            //else
            //{
            //    MessageBox.Show("Vui lòng nhập đủ thông tin lỗi. Sau đó lưu lại!", "THÔNG BÁO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    txtLeader.Focus();
            //    return;
            //}
        }

        private void FormError_Load(object sender, EventArgs e)
        {
            txtId.Focus();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //if (txtMessage.ForeColor == Color.Maroon)
            //{
            //    txtMessage.BackColor = Color.DarkOrange;
            //    txtMessage.ForeColor = Color.White;
            //}
            //else
            //{
            //    txtMessage.BackColor = SystemColors.Window;
            //    txtMessage.ForeColor = Color.Maroon;
            //}
        }
    }
}
