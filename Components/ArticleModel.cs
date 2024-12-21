using DNNrocketAPI;
using Simplisity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using DNNrocketAPI.Components;
using System.Globalization;
using System.Text.RegularExpressions;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Xml.Linq;

namespace RocketDirectoryAPI.Components
{
    public class ArticleModel
    {
        public ArticleModel(PortalCatalogLimpet portalContent, SimplisityInfo info, string langRequired)
        {
            PortalContent = portalContent;
            CultureCode = langRequired;
            if (CultureCode == "") CultureCode = DNNrocketUtils.GetEditCulture();
            Info = info;
            if (Info == null) Info = new SimplisityInfo();
            if (Info.GetXmlProperty("genxml/hidden/modelkey") == "")
            {
                var modelkey = GeneralUtils.GetUniqueString();
                Info.SetXmlProperty("genxml/hidden/modelkey", modelkey);
            }
        }

        public SimplisityInfo Info { get; private set; }
        public string ModelKey
        {
            get
            {
                var rtn = Info.GetXmlProperty("genxml/hidden/modelkey");
                return rtn;
            }
        }
        public string Ref
        {
            get
            {
                return Info.GetXmlProperty("genxml/textbox/modelref");
            }
            set
            {
                Info.SetXmlProperty("genxml/textbox/modelref", value);
            }
        }
        public string Name
        {
            get
            {
                return Info.GetXmlProperty("genxml/lang/genxml/textbox/modelname");
            }
            set
            {
                Info.SetXmlProperty("genxml/lang/genxml/textbox/modelname", value);
            }
        }
        public string BarCode
        {
            get
            {
                return Info.GetXmlProperty("genxml/textbox/barcode");
            }
            set
            {
                Info.SetXmlProperty("genxml/textbox/barcode", value);
            }
        }
        public void PriceSetValue(string price)
        {
            PriceCents = PortalContent.CurrencyConvertCents(price);
        }
        public int PriceCents
        {
            get { return Info.GetXmlPropertyInt("genxml/textbox/modelprice"); }
            set { Info.SetXmlPropertyInt("genxml/textbox/modelprice", value.ToString()); }
        }
        public decimal Price
        {
            get { return (PortalContent.CurrencyCentsToDollars(PriceCents)); }
        }
        public string PriceDisplay(string cultureCode = "")
        {
            if (cultureCode == "") cultureCode = PortalContent.CurrencyCultureCode;
            return Price.ToString("C", CultureInfo.GetCultureInfo(cultureCode));
        }
        public string CultureCode { get; private set; }
        public PortalCatalogLimpet PortalContent { get; private set; }


    }
}
