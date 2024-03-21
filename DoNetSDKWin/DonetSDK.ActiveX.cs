using System;
using System.Runtime.InteropServices;

namespace Com.Boc.Icms.DoNetSDK
{
    /*****************************************************************************************
     *
     * 注： 此接口与类用于ACTIVEX控件的构建
     *      将类名修改为Actiex控件的类名
     *      将类Guid修改为AssemblyInfo类中的Guid
     *      
     *      设置AssemblyInfo类中程序集属性
     *      （[assembly: AllowPartiallyTrustedCallers()]、
     *        [assembly: ComVisible(true)]）
     * 
     * 温馨提醒：请勿修改IObjectSafety、InEvents的GUID，
     *           否则将导致无法标记控件为安全。
    /*****************************************************************************************/
    [ComImport, Guid("CB5BDC81-93C1-11CF-8F20-00805F2CD064")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IObjectSafety
    {
        [PreserveSig]
        int GetInterfaceSafetyOptions(ref Guid riid, [MarshalAs(UnmanagedType.U4)] ref int pdwSupportedOptions, [MarshalAs(UnmanagedType.U4)] ref int pdwEnabledOptions);

        [PreserveSig()]
        int SetInterfaceSafetyOptions(ref Guid riid, [MarshalAs(UnmanagedType.U4)] int dwOptionSetMask, [MarshalAs(UnmanagedType.U4)] int dwEnabledOptions);
    }


    [Guid("2560D44E-325C-4CA0-858F-DEA5F9B89980")]
    //[Guid("ACFC2BD9-BF97-4E08-B9BA-1E9B1D10B85F")]
    public partial class DonetSdk : IObjectSafety
    {
        private const string IidIDispatch = "{00020400-0000-0000-C000-000000000046}";
        private const string IidIDispatchEx = "{a6ef9860-c720-11d0-9337-00a0c90dcaa9}";
        private const string IidIPersistStorage = "{0000010A-0000-0000-C000-000000000046}";
        private const string IidIPersistStream = "{00000109-0000-0000-C000-000000000046}";
        private const string IidIPersistPropertyBag = "{37D84F60-42CB-11CE-8135-00AA004BB851}";

        private const int InterfacesafeForUntrustedCaller = 0x00000001;
        private const int InterfacesafeForUntrustedData = 0x00000002;
        private const int SOk = 0;
        private const int EFail = unchecked((int)0x80004005);
        private const int ENointerface = unchecked((int)0x80004002);

        private readonly bool _fSafeForScripting = true;
        private readonly bool _fSafeForInitializing = true;

        public int GetInterfaceSafetyOptions(ref Guid riid, ref int pdwSupportedOptions, ref int pdwEnabledOptions)
        {
            int rslt = EFail;

            string strGuid = riid.ToString("B");
            pdwSupportedOptions = InterfacesafeForUntrustedCaller | InterfacesafeForUntrustedData;
            switch (strGuid)
            {
                case IidIDispatch:
                case IidIDispatchEx:
                    rslt = SOk;
                    pdwEnabledOptions = 0;
                    if (this._fSafeForScripting)
                    {
                        pdwEnabledOptions = InterfacesafeForUntrustedCaller;
                    }

                    break;
                case IidIPersistStorage:
                case IidIPersistStream:
                case IidIPersistPropertyBag:
                    rslt = SOk;
                    pdwEnabledOptions = 0;
                    if (this._fSafeForInitializing)
                    {
                        pdwEnabledOptions = InterfacesafeForUntrustedData;
                    }

                    break;
                default:
                    rslt = ENointerface;
                    break;
            }

            return rslt;
        }

        public int SetInterfaceSafetyOptions(ref Guid riid, int dwOptionSetMask, int dwEnabledOptions)
        {
            int rslt = EFail;

            string strGuid = riid.ToString("B");
            switch (strGuid)
            {
                case IidIDispatch:
                case IidIDispatchEx:
                    if (((dwEnabledOptions & dwOptionSetMask) == InterfacesafeForUntrustedCaller) &&
                         this._fSafeForScripting)
                    {
                        rslt = SOk;
                    }

                    break;
                case IidIPersistStorage:
                case IidIPersistStream:
                case IidIPersistPropertyBag:
                    if (((dwEnabledOptions & dwOptionSetMask) == InterfacesafeForUntrustedData) &&
                         this._fSafeForInitializing)
                    {
                        rslt = SOk;
                    }

                    break;
                default:
                    rslt = ENointerface;
                    break;
            }

            return rslt;
        }
    }
}
