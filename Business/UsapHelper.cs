using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OQC_Check_App.Business
{
    public class UsapHelper
    {
        #region Property
        private static string _BoxID;
        private static string _ProductID;
        private static int _Qty;
        private static int _WO_QTY;
        private static string _WO;
        private static string _TN_NO;
        private static int _Aql;
        private static bool _Is_Aql;
        private static bool _Is_Kyo;
        private static bool _Is_Canon;
        public static string BoxID { get { return _BoxID; } }
        public static string ProductID { get { return _ProductID; } }
        public static int Qty { get { return _Qty; } }
        public static int WO_QTY { get { return _WO_QTY; } }
        public static string WO { get { return _WO; } }
        public static string TnNo { get { return _TN_NO; } }
        public static bool IsAql
        {
            get
            {
                return _Is_Aql;
            }
        }
        public static bool IsKyo
        {
            get
            {
                return _Is_Kyo;
            }
        }
        public static bool IsCanon
        {
            get
            {
                return _Is_Canon;
            }
        }
        public static int Aql
        {
            get
            {
                return _Aql;
            }
        }
        #endregion

        #region Method
        public static void FindBoxItem(string boxID)
        {
            var bclcflm = SingletonHelper.UsapInstance.GetByBcNo(boxID);
            if (Validator.CheckVirtualBox(boxID))
            {

            }
            if (bclcflm != null)
            {
                _BoxID = bclcflm.BC_NO;
                _ProductID = bclcflm.PART_NO;
                _Qty = Convert.ToInt32(bclcflm.OS_QTY);
                _WO = bclcflm.TN_NO.Right(10);
                _WO_QTY = Convert.ToInt32(SingletonHelper.UsapInstance.FindWoQty(bclcflm.TN_NO));
                _TN_NO = bclcflm.TN_NO;
                _Is_Kyo = bclcflm.WH_CODE.StartsWith("KY");
                _Is_Canon = bclcflm.WH_CODE.StartsWith("CA");
                var model = SingletonHelper.PvsInstance.GetModelInfo(_ProductID);
                if (model != null)
                {
                    _Is_Aql = Convert.ToBoolean(model.Is_Aql);
                }
                else
                {
                    _Is_Aql = false;
                }
                if (_Is_Aql)
                {
                    _Aql = _WO_QTY <= 0 ? 0
                        : _WO_QTY <= 8 ? 8
                        : _WO_QTY <= 15 ? 15
                        : _WO_QTY <= 280 ? 20
                        : _WO_QTY <= 1200 ? 80
                        : _WO_QTY <= 3200 ? 125
                        : _WO_QTY <= 10000 ? 200
                        : _WO_QTY <= 35000 ? 315
                        : _WO_QTY <= 150000 ? 500
                        : _WO_QTY <= 50000 ? 800
                        : 1250;
                }
                else
                {
                    _Aql = _WO_QTY;
                }
            }
            else if (boxID == "2000231101A")
            {
                _Qty = _WO_QTY = 17;
                _BoxID = boxID.Right(7);
                _TN_NO = "2000231101A";
                _ProductID = "3V2M480036-05";
                _Is_Aql = false;
                _Aql = 17;

            }
            else
            {
                _BoxID = string.Empty;
                _ProductID = string.Empty;
                _Qty = 0;
                _WO_QTY = 0;
                _WO = string.Empty;
                _TN_NO = string.Empty;
            }
        }
        #endregion
    }
}
