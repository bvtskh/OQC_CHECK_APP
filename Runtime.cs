using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OQC_Check_App.Business
{
    public class Runtime
    {
        public class UmesInfo
        {
            public static string BoardNo { get; set; }
            public static string ProductId { get; set; }
        }
        public class BoxInfo
        {
            public static string BoxId { get; set; }
            public static string ProductId { get; set; }
            public static int Quantity { get; set; }
        }
        public class TestLogInfo
        {
            public static int Quantity { get; set; }
        }
        public class User
        {
            public static string ID { get; set; }
            public static string Name { get; set; }
        }
        public static bool IsWip()
        {
            return !UmesInfo.ProductId.IsNullOrBlank();
        }
        public static PVSWebService.BARCODE_RULE_ITEMSEntity GetRuleItem()
        {
            string extend = "000SS01";
            var ruleNo = BoxInfo.ProductId;

            var ruleItem = SingletonHelper.GetRule(ruleNo);
            if (ruleItem == null)
            {
                ruleNo = BoxInfo.ProductId.Substring(0, BoxInfo.ProductId.Length - extend.Length - 1);
            }
            ruleItem = SingletonHelper.GetRule(ruleNo);
            return ruleItem;
        }
        public static bool IsProductNotMap(PVSWebService.BARCODE_RULE_ITEMSEntity ruleItem)
        {
            var content = UmesInfo.BoardNo.Substring((int)ruleItem.INDEX - 1, (int)ruleItem.CONTENT_LENGTH);
            return content != ruleItem.CONTENT;
        }
        public static bool IsFullBox()
        {
            return BoxInfo.Quantity == TestLogInfo.Quantity;
        }
        public static void SetBoxInfo(USAPService.BCLBFLMEntity entity)
        {
            BoxInfo.ProductId = entity.PART_NO;
            BoxInfo.Quantity = Convert.ToInt32(entity.OS_QTY);
            BoxInfo.BoxId = entity.BC_NO;
        }
        public static void SetUserLogin(PVSWebService.USERSEntity user)
        {
            User.ID = user.ID;
            User.Name = user.NAME;
        }
    }
}
