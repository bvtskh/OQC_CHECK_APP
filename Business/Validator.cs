using OQC_Check_App.ERPService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OQC_Check_App.Business
{
    public class Validator
    {
        public static bool CheckVirtualBox(string box)
        {
            if(box == "F000000000")
            {
                return true;
            }
            return false;
        }

        public static bool CheckPacking(string boardNo)
        {
            if (UsapHelper.IsKyo)
            {
                var entity = SingletonHelper.PvsInstance.KyoGetBoard(boardNo);
                return entity == null;
            }
            return false;
        }
        private static ERPService.tbl_test_logEntity _entity;
        public static ERPService.tbl_test_logEntity entity
        {
            get
            {
                return _entity;
            }
        }
        public static IEnumerable<ResultEntity> GetErrors(string boardNo, string boxID, bool check = true)
        {
            ResultEntity result = new ResultEntity();
            _entity = SingletonHelper.ErpInstance.OQCTestLogFind(boardNo);

            if(_entity!=null && _entity.QA_Check && _entity.BoxID != boxID)
            {
                if (MessageBox.Show($"BoardNo: {boardNo} đã được bắn tại BoxId: {_entity.BoxID}\nBạn có muốn bắn vào BoxId mới?\nYES để tiếp tục!", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    string oldBoxId = _entity.BoxID;
                    string oldJudge = _entity.Judge;
                    string oldCheckDate = _entity.OQCCheckDate.ToString();
                    string oldCode = _entity.OperatorCode;
                    string oldName = _entity.OperatorNameCheck;
                    Validator.entity.OperatorCheck = UmesHelper.User.ID;
                    Validator.entity.Judge = check == true ? "OK" : "NG";
                    Validator.entity.QA_Check = true;
                    Validator.entity.BoxID = boxID;
                    Validator.entity.OldBoxID = oldBoxId;
                    Validator.entity.BoxIDCheck = boxID;
                    Validator.entity.OperatorNameCheck = UmesHelper.User.NAME;
                    Validator.entity.OQCCheckDate = SingletonHelper.PvsInstance.GetDateTime();

                    var res = SingletonHelper.ErpInstance.OQCTestLogSave(Validator.entity.ProductionID, Validator.entity);
                    if (res.status == "OK")
                    {
                        TestLogEntity testLogEntity = new TestLogEntity();
                        var id = Guid.NewGuid().ToString();
                        testLogEntity.ID = id;
                        testLogEntity.BOARD_NO = boardNo;
                        testLogEntity.SUB_BOARD = oldBoxId;
                        testLogEntity.PRODUCT_ID = oldJudge;
                        testLogEntity.ORDER_NO = oldCheckDate;
                        testLogEntity.HOST_NAME = oldCode;
                        testLogEntity.CLIENT_NAME = oldName;
                        testLogEntity.TEST_TIME = DateTime.Now;
                        SingletonHelper.ErpInstance.SaveTestLog(testLogEntity);
                    }
                    result.IsLock = false;
                    result.IsPass = true;
                    result.Status = $"Đổi BoxId cho BoardNo: {boardNo}";
                    yield return result;
                }
            }
            //if (_entity != null && _entity.QA_Check == true)
            //{
            //    result.IsLock = UsapHelper.IsKyo ? true : false;
            //    result.IsPass = false;
            //    result.Status = $"Barcode [{boardNo}] đã được kiểm tra.\nNgày kiểm tra [{entity.OQCCheckDate.Value.ToShortDateString()}]";
            //    yield return result;
            //}
            if (UsapHelper.IsKyo)
            {
                var entity = SingletonHelper.PvsInstance.KyoGetBoard(boardNo);
                if (entity == null)
                {
                    result.IsLock = true;
                    result.IsPass = false;
                    result.Status = $"Không có dữ liệu đóng gói [{UsapHelper.BoxID}]";
                    yield return result;
                }
                if (entity.BoxID != UsapHelper.BoxID)
                {
                    result.IsLock = false;
                    result.IsPass = false;
                    result.Status = $"Đóng sai thùng!\nVui lòng kiểm tra lại";
                    yield return result;
                }
            }
            if (UsapHelper.IsCanon)
            {
                TestLogEntity testLogEntity = SingletonHelper.ErpInstance.GetLastTestLog(boardNo, "FCT_CAN");
                if (testLogEntity != null && testLogEntity.BOARD_STATE == 2)
                {
                    result.IsLock = true;
                    result.Status = $"FCT NG!";
                    yield return result;
                }
            }
            if (UmesHelper.IsWip)
            {

                if (!UmesHelper.CompareModel())
                {
                    result.IsLock = true;
                    result.IsPass = false;
                    result.Status = $"Barcode thuộc Model [{UmesHelper.Items.PRODUCT_ID}]\nHệ thống bị khóa!";
                    yield return result;
                }
                if (!UmesHelper.MsgErr.IsNullOrBlank())
                {
                    result.IsLock = false;
                    result.IsPass = false;
                    result.Status = UmesHelper.MsgErr;
                    yield return result;
                }
            }
            else
            {
                if (UmesHelper.RuleItem == null)
                {
                    result.IsLock = false;
                    result.IsPass = false;
                    result.Status = $"Chưa thiết lập Rule [{UsapHelper.ProductID}]!";
                    yield return result;
                }
                if (!UmesHelper.MatchRule(boardNo))
                {
                    result.IsLock = true;
                    result.IsPass = false;
                    result.Status = "Sai Model(Rule).\nHệ thống bị khóa!";
                    yield return result;
                }
                if (Validator.CheckPacking(boardNo))
                {
                    result.IsLock = false;
                    result.IsPass = false;
                    result.Status = $"[{boardNo}] không có dữ liệu VI2!";
                    yield return result;
                }
            }
        }
    }
}
