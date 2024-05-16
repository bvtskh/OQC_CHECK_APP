using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OQC_Check_App.Business
{
    public class SingletonHelper
    {
        private static volatile PVSWebService.PVSWebServiceSoapClient _pvs_service = null;
        public static volatile USAPService.USAPWebServiceSoapClient _usap_service = null;
        public static volatile ERPService.ERPWebServiceSoapClient _erp_service = null;
        private static readonly object sync = new object();
        public static PVSWebService.PVSWebServiceSoapClient PvsInstance
        {
            get
            {
                if (_pvs_service == null)
                {
                    lock (sync)
                    {
                        if (_pvs_service == null)
                        {
                            _pvs_service = new PVSWebService.PVSWebServiceSoapClient();
                        }
                    }
                }
                return _pvs_service;
            }

        }
 
        public static USAPService.USAPWebServiceSoapClient UsapInstance
        {
            get
            {
                if (_usap_service == null)
                {
                    lock (sync)
                    {
                        if (_usap_service == null)
                        {
                            _usap_service = new USAPService.USAPWebServiceSoapClient();
                        }
                    }
                }
                return _usap_service;
            }
        }
        public static ERPService.ERPWebServiceSoapClient ErpInstance
        {
            get
            {
                if (_erp_service == null)
                {
                    lock (sync)
                    {
                        if (_erp_service == null)
                        {
                            _erp_service = new ERPService.ERPWebServiceSoapClient();
                        }
                    }
                }
                return _erp_service;
            }
        }
        //public static DateTime GetDateTime()
        //{
        //    try
        //    {
        //        return SingletonHelper.PvsInstance.GetDateTime();
        //    }
        //    catch
        //    {
        //        return DateTime.Now;
        //    }
        //}
    }
}
