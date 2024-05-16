using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OQC_Check_App.Business
{
    public class UmesHelper
    {
        #region Property
        private static bool _Is_Wip;
        public static bool IsWip { get { return _Is_Wip; } }
        private static PVSWebService.OrderItemEntity _Items;
        public static PVSWebService.OrderItemEntity Items { get { return _Items; } }
        // private static string _ProductID;
        // public static string ProductID { get { return _ProductID; } }
        private static PVSWebService.BARCODE_RULE_ITEMSEntity _RuleItem;
        public static PVSWebService.BARCODE_RULE_ITEMSEntity RuleItem { get { return _RuleItem; } }
        private static PVSWebService.USERSEntity _User;
        public static PVSWebService.USERSEntity User { get { return _User; } }
        #endregion
        #region Method
        public static void FindUmesInfo(string boardNo)
        {
            _Items = SingletonHelper.PvsInstance.GetOrderItem(boardNo);
            _Is_Wip = _Items != null;
            if (!_Is_Wip)
            {
                _RuleItem = SingletonHelper.PvsInstance.GetBarodeRuleItemsByRuleNo(UsapHelper.ProductID);
                if (_RuleItem == null)
                {
                    var tmp = UsapHelper.ProductID.LastIndexOf('-');
                    _RuleItem = SingletonHelper.PvsInstance.GetBarodeRuleItemsByRuleNo(UsapHelper.ProductID.Left(tmp));
                }
            }
            else
            {
                RemoveRepair();
            }
        }
        public static bool MatchRule(string boardNo)
        {
            if (!_Is_Wip)
            {
                if (boardNo.Length != _RuleItem.LENGTH)
                {
                    return false;
                }
                var content = boardNo.Substring((int)_RuleItem.LOCATION - 1, (int)_RuleItem.CONTENT_LENGTH);
                return string.Equals(content, _RuleItem.CONTENT, StringComparison.OrdinalIgnoreCase);
                //var tmp = content != _RuleItem.CONTENT;
                //return content != _RuleItem.CONTENT;
            }
            return false;
        }
        public static bool Login(string username, string password)
        {
            _User = SingletonHelper.PvsInstance.CheckUserLogin(username, password);
            return _User != null;
        }
        public static bool Unlock(string username, string password)
        {
            var user = SingletonHelper.PvsInstance.FindUser(username, password);
            if (user != null)
            {
                return user.Rules.Any(r => r.MODULE == "OQC_CRM" && r.RULE_ID == 1);
            }
            return false;
        }

        public static string MsgErr
        {
            get
            {
                var hostName = NetworkHelper.GetMacAddress("http://172.28.10.8:8084");
                var inspection = SingletonHelper.PvsInstance.GetStationByHostName(hostName);
                if (inspection == null)
                {
                    return "Thiết lập trạm trên WIP và thử lại";
                }
                if (inspection.STATION_NO.Contains("VI"))
                {
                    return $"Phát hiện trạm [{inspection.STATION_NO}] bất thường.\nChỉ sử dụng cho trạm OQC";
                }
                var procedure = SingletonHelper.PvsInstance.GetWoProcedure(_Items.ORDER_ID.ToString(), inspection.STATION_NO).FirstOrDefault();
                if (procedure == null)
                {
                    return $"[{_Items.ORDER_NO}] không có trạm [{inspection.STATION_NO}]";
                }
                if (_Items.PROCEDURE_INDEX < procedure.INDEX - 1)
                {
                    return $"[{_Items.BOARD_NO}] đang tại trạm [{_Items.STATION_NO}]";
                }
                return string.Empty;
            }
        }
        public static bool CompareModel()
        {
            if (UmesHelper.Items.PRODUCT_ID.Equals(UsapHelper.ProductID, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            string umesModel = UmesHelper.Items.PRODUCT_ID;
            if (umesModel.EndsWith("AI"))
            {
                umesModel = umesModel.Replace("AI", "").Trim();
            }
            if (umesModel.EndsWith("SMT"))
            {
                umesModel = umesModel.Replace("SMT", "").Trim();
            }

            umesModel = umesModel.Replace("000", "").Trim();
            umesModel = umesModel.Replace("00", "").Trim();


            var usapModel = UsapHelper.ProductID;
            var arrUmesModel = umesModel.Split('-');
            var arrUsapModel = UsapHelper.ProductID.Split('-');
            if (arrUsapModel.Length >= 3 && arrUsapModel[2].Contains("000SS"))
            {
                usapModel = string.Format("{0}-{1}-{2}", arrUsapModel[0], arrUsapModel[1], arrUsapModel[2].Substring(3, arrUsapModel[2].Length - 3));
                usapModel = usapModel.Replace("000", "").Trim();              
                //return umesModel.Equals(usapModel, StringComparison.OrdinalIgnoreCase);
            }
            arrUsapModel = usapModel.Split('-');
            if (arrUsapModel.Count() == arrUmesModel.Count())
            {
                for(int index = 0; index < arrUsapModel.Count(); index++) 
                {
                    if (arrUsapModel[index] != arrUmesModel[index] && index == arrUsapModel.Count()-1)
                    {
                        var endUsap = Regex.Replace(arrUsapModel[index], @"[^\d]", "");
                        var endUmes = Regex.Replace(arrUmesModel[index], @"[^\d]", "");
                        if (endUsap == endUmes) return true;
                    } 
                }
            }
            var min = Math.Min(usapModel.Length, umesModel.Length);
            return umesModel.Left(min).Equals(usapModel.Left(min), StringComparison.OrdinalIgnoreCase);
        }
        private static void RemoveRepair()
        {
            if (UsapHelper.IsKyo)
            {
                var packEntity = SingletonHelper.ErpInstance.OQCTestLogFind(_Items.BOARD_NO);
                if (packEntity != null)
                {
                    var repairEntity = SingletonHelper.PvsInstance.GetRepair(_Items.BOARD_NO);
                    if (repairEntity != null && DateTime.Compare(repairEntity.UPDATE_TIME, (DateTime)packEntity.DateCheck) > 0)
                    {
                        SingletonHelper.ErpInstance.OQCTestLogRemove(_Items.BOARD_NO);
                    }
                }
            }
        }
        #endregion
    }
}
