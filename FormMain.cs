using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OQC_Check_App.Business;

namespace OQC_Check_App
{
    public partial class FormMain : Form
    {
        BackgroundWorker worker = new BackgroundWorker();
        public FormMain()
        {
            InitializeComponent();
            dataGridView1.AutoGenerateColumns = false;
            lblUser.Text = Business.UmesHelper.User.NAME;
            lblRunVersion.Text = Ultils.GetRunningVersion();
            var value = Ultils.GetValueRegistryKey("lock");
            if (value != null)
            {
                var lockState = Convert.ToInt32(value);
                if (lockState == 1)
                {
                    new frmLockSystem().ShowDialog();
                }
            }
           
            worker.DoWork += new DoWorkEventHandler(CheckBarcode);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(CheckBarcodeFinish);
        }

        private void CheckBarcodeFinish(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Enabled = true;
            txtBarcode.ResetText();
            txtBarcode.Focus();
            ResultEntity entity = e.Result as ResultEntity;
            if (entity.IsLock)
            {
                new frmLockSystem().ShowDialog();
                DisplayMessage("NG", entity.Status);
            }
            else if (!entity.IsPass)
            {
                DisplayMessage("NG", entity.Status);
            }
            else
            {
                if (UmesHelper.IsWip)
                {
                    string status = chkOK.Checked ? "P" : "F";
                    Ultils.WriteLog(entity.Status, status, SingletonHelper.PvsInstance.GetDateTime());
                }
                DisplayMessage("OK", entity.Status);
                txtBarcode.ResetText();
                var source = SingletonHelper.ErpInstance.OQCTestLogGetByBoxID(UsapHelper.BoxID);
                var oqcCheckTotal = SingletonHelper.ErpInstance.OQCTestLogTotal(UsapHelper.WO);
                dataGridView1.DataSource = source;
                lblQuantity.Text = $"{source.Count}/{UsapHelper.Qty}";
                lblQuality.Text = $"{oqcCheckTotal}/{UsapHelper.Aql}/{UsapHelper.WO_QTY}";
                if (source.Count >= UsapHelper.Qty)
                {
                    DisplayMessage("OK", $"BoxID [{UsapHelper.BoxID}] đã đầy.\nVui lòng nhập BoxID khác!");
                    txtBoxID.Enabled = true;
                    txtBoxID.ResetText();
                    txtBoxID.Focus();
                    txtBarcode.Enabled = false;
                    chkOK.Checked = chkNG.Checked = false;
                    chkOK.Enabled = false;
                    chkNG.Enabled = false;
                }
                else
                {
                    var msg = string.Format("Successfully.\nCần kiểm tra {0} Pcs ({1})", Math.Max(UsapHelper.Aql - oqcCheckTotal, 0), UsapHelper.IsAql ? "AQL" : "100%");
                    DisplayMessage("OK", msg);
                    txtBarcode.Focus();
                }

            }
        }

        private void CheckBarcode(object sender, DoWorkEventArgs e)
        {
            var dateTime = SingletonHelper.PvsInstance.GetDateTime();
            var boardNo = e.Argument.ToString();
            UmesHelper.FindUmesInfo(boardNo);
            foreach (var msg in Validator.GetErrors(boardNo))
            {
                e.Result = msg;
                return;
            }
            if (Validator.entity == null)
            {
                var entity = new ERPService.tbl_test_logEntity()
                {
                    ProductionID = boardNo,
                    MacAddress = NetworkHelper.GetMacAddress("http://172.28.10.8:8084"),
                    LineID = 1,
                    BoxID = UsapHelper.BoxID,
                    OperatorCode = UmesHelper.User.ID,
                    DateCheck = dateTime,
                    Judge = chkOK.Checked == true ? "OK" : "NG",
                    ModelNO = UsapHelper.ProductID,
                    ModelName = UsapHelper.ProductID,
                    Shipping = false,
                    Date_Shipping = null,
                    OperatorCheck = UmesHelper.User.ID,
                    QA_Check = true,
                    BoxIDCheck = UsapHelper.BoxID,
                    OperatorNameCheck = UmesHelper.User.NAME,
                    OQCCheckDate = dateTime,
                    WorkingOrder = UsapHelper.TnNo.Right(10),
                    Soft_Ver = Ultils.GetRunningVersion()
                };
                var res = SingletonHelper.ErpInstance.OQCTestLogSave("", entity);
                if (res.status == "NG")
                {
                    e.Result = new ResultEntity()
                    {
                        IsLock = false,
                        IsPass = false,
                        Status = "Có lỗi trong quá trình lưu dữ liệu!\nVui lòng kiểm tra và bắn lại!"
                    };
                    return;
                }
            }
            else
            {
                Validator.entity.OperatorCheck = UmesHelper.User.ID;
                Validator.entity.QA_Check = true;
                Validator.entity.BoxIDCheck = UsapHelper.BoxID;
                Validator.entity.OperatorNameCheck = UmesHelper.User.NAME;
                Validator.entity.OQCCheckDate = dateTime;
                var res = SingletonHelper.ErpInstance.OQCTestLogSave(Validator.entity.ProductionID, Validator.entity);
                if (res.status == "NG")
                {
                    e.Result = new ResultEntity()
                    {
                        IsLock = false,
                        IsPass = false,
                        Status = "Có lỗi trong quá trình lưu dữ liệu!\nVui lòng kiểm tra và bắn lại!"
                    };
                    return;
                }
            }
            e.Result = new ResultEntity() { IsLock = false, IsPass = true, Status = boardNo };
        }

        /// <summary>
        /// Thông báo lỗi
        /// </summary>
        /// <param name="status"></param>
        /// <param name="message"></param>
        void DisplayMessage(string status, string message)
        {
            Color backColor = new Color();
            Color foreColor = new Color();

            switch (status)
            {
                case "OK":
                    backColor = Color.DarkGreen;
                    foreColor = Color.White;
                    break;
                case "NG":
                    backColor = Color.DarkRed;
                    foreColor = Color.White;
                    break;
                case "WARNING":
                    backColor = Color.DarkOrange;
                    foreColor = Color.White;
                    break;
                default:
                    backColor = Color.White;
                    foreColor = Color.FromArgb(192, 64, 0);
                    break;
            }
            this.BeginInvoke(new Action(() => { lblStatus.Text = status; }));
            this.BeginInvoke(new Action(() => { lblStatus.BackColor = backColor; }));
            this.BeginInvoke(new Action(() => { lblStatus.ForeColor = foreColor; }));

            this.BeginInvoke(new Action(() => { lblMessge.Text = message; }));
            this.BeginInvoke(new Action(() => { lblMessge.BackColor = backColor; }));
            this.BeginInvoke(new Action(() => { lblMessge.ForeColor = foreColor; }));
        }
        private void chkOK_CheckedChanged(object sender, EventArgs e)
        {
            if (chkOK.Checked)
            {
                chkNG.Checked = false;
                txtBarcode.Enabled = true;
                txtBarcode.Focus();
            }
        }

        private void chkNG_CheckedChanged(object sender, EventArgs e)
        {
            if (chkNG.Checked)
            {
                chkOK.Checked = false;
                txtBarcode.Enabled = true;
                txtBarcode.Focus();

            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtBoxID_TextChanged(object sender, EventArgs e)
        {
            if (Ultils.IsNullOrEmpty(txtBoxID))
            {
                DisplayMessage("N/A", "no results");
            }
        }
        private void txtBarcode_TextChanged(object sender, EventArgs e)
        {
            if (Ultils.IsNullOrEmpty(txtBarcode))
            {
                DisplayMessage("N/A", "no results");
            }
        }

        private void txtBoxID_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string boxId = txtBoxID.Text.Trim();
                if (boxId.IsNullOrBlank())
                {
                    return;
                }

                UsapHelper.FindBoxItem(boxId);

                if (UsapHelper.BoxID.IsNullOrBlank())
                {
                    DisplayMessage("NG", $"BoxID [{boxId}] không tồn tại.\nVui lòng nhập lại!");
                    lblWO.ResetText();
                    lblQuality.ResetText();
                    txtBoxID.SelectAll();
                    txtBoxID.Focus();
                    return;
                }
                var oqcCheckTotal = SingletonHelper.ErpInstance.OQCTestLogTotal(UsapHelper.WO);
                tProduct.Text = lblModel.Text = UsapHelper.ProductID;
                this.lblWO.Text = UsapHelper.WO;
                lblQuality.Text = $"{oqcCheckTotal}/{UsapHelper.Aql}/{UsapHelper.WO_QTY}";
                var lst = SingletonHelper.ErpInstance.OQCTestLogGetByBoxID(boxId);
                dataGridView1.DataSource = lst;
                lblQuantity.Text = $"{lst.Count}/{UsapHelper.Qty}";
                if (lst.Count >= UsapHelper.Qty)
                {
                    DisplayMessage("OK", $"BoxID [{boxId}] đã đầy.\nVui lòng nhập Box khác!");
                    txtBoxID.SelectAll();
                    txtBoxID.Focus();
                    return;
                }
                //if (UsapHelper.IsKyo)
                //{
                //    if (Validator.lstEngine.Contains(UsapHelper.ProductID.LeftOf('-')))
                //    {
                //        var vi2Packing = SingletonHelper.PvsInstance.KyoGetMac(UsapHelper.BoxID);
                //        if (vi2Packing.Any(r => r.DownloadStatus != "OK"))
                //        {
                //            DisplayMessage("NG", $"Mạch trong thùng chưa có dữ liệu FCT!");
                //            txtBoxID.SelectAll();
                //            txtBoxID.Focus();
                //            return;
                //        }
                //    }
                //}
                var msg = string.Format("{0}.\nCần kiểm tra {1} Pcs ({2})", UsapHelper.IsKyo ? "Mac thùng đã check log FCT-OK" : "Successfully", Math.Max(UsapHelper.Aql - oqcCheckTotal, 0), UsapHelper.IsAql ? "AQL" : "100%");
                DisplayMessage("OK", msg);
                chkOK.Enabled = chkNG.Enabled = true;
                chkOK.Checked = true;
                txtBoxID.Enabled = false;
            }
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                DialogResult dialogResult = MessageBox.Show("Bạn có thực sự muốn đóng hay không!",
                    @"THÔNG BÁO",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                if (dialogResult == DialogResult.Yes)
                {
                    Application.ExitThread();
                }
            }
        }

        private void txtBarcode_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                var boardNo = txtBarcode.Text;
                if (boardNo.IsNullOrBlank())
                {
                    return;
                }
                if (!worker.IsBusy)
                {
                    this.Enabled = false;
                    worker.RunWorkerAsync(argument: boardNo);
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReset_Click(object sender, EventArgs e)
        {
            DisplayMessage("N/A", "no results");
            txtBoxID.Enabled = true;
            txtBoxID.ResetText();

            txtBarcode.ResetText();
            txtBarcode.Enabled = false;
            lblModel.ResetText();
            lblQuantity.Text = string.Format("0/0");
            lblWO.ResetText();
            lblQuality.ResetText();
            dataGridView1.DataSource = null;

            txtBoxID.Focus();
            chkOK.Checked = chkNG.Checked = false;
            chkOK.Enabled = chkNG.Enabled = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (txtSearchKey.Visible == false)
            {
                txtSearchKey.Visible = true;
                cboValue.Visible = true;
                lblSearchKey.Visible = true;
                btnClear.Visible = true;
                lblNote.Visible = true;
                txtSearchKey.Focus();
            }
            else
            {
                Search();
            }
        }

        // Tìm kiếm
        void Search()
        {
            string value = cboValue.Text;
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            string key = txtSearchKey.Text;
            var data = new List<ERPService.tbl_test_logEntity>();
            if (value == "BoxID")
            {
                if (key != "")
                {
                    // data = testLog.GetOQCCheck(key, true);
                    data = SingletonHelper.ErpInstance.OQCTestLogGetByBoxID(key);
                    dataGridView1.AutoGenerateColumns = false;
                    dataGridView1.DataSource = data;
                    //for (int i = 0; i < dataGridView1.RowCount; i++)
                    //{
                    //    string judge = dataGridView1.Rows[i].Cells["Column4"].Value.ToString();
                    //    if (judge == "OK")
                    //    {
                    //        dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.DarkGreen;
                    //        dataGridView1.Rows[i].DefaultCellStyle.ForeColor = Color.White;
                    //    }
                    //    if (judge == "NG")
                    //    {
                    //        dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.DarkRed;
                    //        dataGridView1.Rows[i].DefaultCellStyle.ForeColor = Color.White;
                    //    }
                    //}
                    if (data.Count < 1)
                    {
                        DisplayMessage("NG", $"Không tìm thấy dữ liệu mác thùng '{txtSearchKey.Text}'!");
                    }
                    txtSearchKey.ResetText();
                    txtSearchKey.Focus();

                }
                else
                {
                    DisplayMessage("NG", "Vui lòng nhập vào mã thùng cần tìm kiếm!");
                    txtSearchKey.Focus();
                }
            }
            else if (value == "Board NO")
            {
                if (key != "")
                {
                    var entity = SingletonHelper.ErpInstance.OQCTestLogFind(key);
                    if (entity == null)
                    {
                        DisplayMessage("NG", $"Không tìm thấy dữ liệu của bản mạch '{txtSearchKey.Text}'!");
                    }
                    else
                    {

                        data.Add(entity);
                        dataGridView1.AutoGenerateColumns = false;
                        dataGridView1.DataSource = data;
                    }
                    txtSearchKey.ResetText();
                    txtSearchKey.Focus();

                }
                else
                {
                    DisplayMessage("NG", "Vui lòng bắn vào mã bản mạch cần tìm kiếm!");
                    txtSearchKey.Focus();
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboValue_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtSearchKey.SelectAll();
            txtSearchKey.Focus();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClear_Click(object sender, EventArgs e)
        {
            txtSearchKey.Visible = false;
            cboValue.Visible = false;
            lblSearchKey.Visible = false;
            btnClear.Visible = false;
            lblNote.Visible = false;
            dataGridView1.DataSource = null;
            dataGridView1.Refresh();

            if (txtBarcode.Enabled == true)
            {
                txtBarcode.ResetText();
                txtBarcode.Focus();
            }
            else
            {
                btnReset.PerformClick();
            }
        }

        private void txtSearchKey_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Search();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count > 0)
            {
                if (MessageBox.Show("Bạn có chắc muốn xóa bản mạch này không?", "Xóa bản ghi", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    int selectedrowindex = dataGridView1.SelectedCells[0].RowIndex;
                    DataGridViewRow selectedRow = dataGridView1.Rows[selectedrowindex];
                    //  List<tbl_test_log> data = new List<tbl_test_log>();

                    string boardNo = Convert.ToString(selectedRow.Cells["Column1"].Value);
                    string boxId = Convert.ToString(selectedRow.Cells["Column3"].Value);
                    //var checkExists = testLog.Get(boardNo);
                    var checkExists = SingletonHelper.ErpInstance.OQCTestLogFind(boardNo);
                    checkExists.OperatorCheck = null;
                    checkExists.QA_Check = false;
                    checkExists.BoxIDCheck = null;
                    checkExists.OperatorNameCheck = null;
                    checkExists.OQCCheckDate = null;

                    //  testLog.UpdateOQCCheck(checkExists);
                    // testLog.Delete(boardNo);
                    var res = SingletonHelper.ErpInstance.OQCTestLogRemove(boardNo);
                    if (res.status == "OK")
                    {

                        //var  data = testLog.GetOQCCheck(boxId, true);
                        var data = SingletonHelper.ErpInstance.OQCTestLogGetByBoxID(boxId);
                        dataGridView1.DataSource = data;
                        lblQuantity.Text = $"{data.Count}/{UsapHelper.Qty}";
                        var oqcCheckTotal = SingletonHelper.ErpInstance.OQCTestLogTotal(UsapHelper.WO);
                        lblQuality.Text = $"{oqcCheckTotal}/{UsapHelper.Aql}/{UsapHelper.WO_QTY}";
                        MessageBox.Show("Xóa thành công!", "THÔNG BÁO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Xóa thất bại!\nLiên hệ IT(3143).", "THÔNG BÁO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemExportExcel_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count > 0)
            {
                SaveToCSV(dataGridView1);
            }
            else
            {
                MessageBox.Show("Không tìm thấy dữ liệu để xuất!", "THÔNG BÁO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="DGV"></param>
        private void SaveToCSV(DataGridView DGV)
        {
            string filename = "";
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV (*.csv)|*.csv";
            sfd.FileName = $"{DateTime.Now.ToString("yyyyMMdd")}.csv";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                if (File.Exists(filename))
                {
                    try
                    {
                        File.Delete(filename);
                    }
                    catch (IOException ex)
                    {
                        MessageBox.Show("Lỗi trong quá trình lưu dữ liệu.\nChi tiết lỗi: " + ex.Message, "THÔNG BÁO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                int columnCount = DGV.ColumnCount;
                string columnNames = "";
                string[] output = new string[DGV.RowCount + 1];
                for (int i = 0; i < columnCount; i++)
                {
                    columnNames += DGV.Columns[i].HeaderText.ToString() + ",";
                }
                output[0] += columnNames;
                for (int i = 1; (i - 1) < DGV.RowCount; i++)
                {
                    for (int j = 0; j < columnCount; j++)
                    {
                        output[i] += DGV.Rows[i - 1].Cells[j].Value.ToString() + ",";
                    }
                }
                File.WriteAllLines(sfd.FileName, output, Encoding.UTF8);
                MessageBox.Show("Xuất và Lưu dữ liệu thành công.", "THÔNG BÁO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Process.Start(sfd.FileName);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemAllBox_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count > 0)
            {
                int selectedrowindex = dataGridView1.SelectedCells[0].RowIndex;
                DataGridViewRow selectedRow = dataGridView1.Rows[selectedrowindex];

                string boxId = Convert.ToString(selectedRow.Cells["Column3"].Value);

                if (MessageBox.Show($"Bạn có chắc muốn xóa tất bản mạch trong thùng [{boxId}] này không?", "THÔNG BÁO", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    try
                    {
                        foreach (var item in SingletonHelper.ErpInstance.OQCTestLogGetByBoxID(boxId))
                        {
                            item.Date_Shipping = null;
                            item.OperatorCheck = null;
                            item.QA_Check = false;
                            item.BoxIDCheck = null;
                            item.OperatorNameCheck = null;
                            item.OQCCheckDate = null;
                            SingletonHelper.ErpInstance.OQCTestLogSave(item.ProductionID, item);
                            // testLog.UpdateOQCCheck(item);

                            //testResult.Delete(item.ProductionID, 1);
                        }
                        dataGridView1.DataSource = null;
                        dataGridView1.Refresh();

                        MessageBox.Show($"Xóa tất cả bản mạch trong thùng [{boxId}] thành công!", "THÔNG BÁO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.btnReset.PerformClick();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi!\n{ex.Message}", "THÔNG BÁO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void MenuItemCopyCell_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count > 0)
            {
                int selectedrowindex = dataGridView1.SelectedCells[0].RowIndex;
                int selectedColumnIndex = dataGridView1.SelectedCells[0].ColumnIndex;

                DataGridViewRow selectedRow = dataGridView1.Rows[selectedrowindex];
                string value = dataGridView1[selectedColumnIndex, selectedrowindex].Value.ToString();

                string boardNo = Convert.ToString(value);
                Clipboard.SetText(boardNo);
            }
        }
    }
}
