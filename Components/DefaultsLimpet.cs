using DNNrocketAPI.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace RocketDirectoryAPI.Components
{
    public class DefaultsLimpet
    {
        private const string _defaultFileMapPath = "/DesktopModules/DNNrocketModules/RocketDirectoryAPI/Installation/SystemDefaults.rules";
        public DefaultsLimpet()
        {
            try
            {
                Info = (SimplisityInfo)CacheUtils.GetCache(_defaultFileMapPath);
                if (Info == null)
                {
                    var filenamepath = DNNrocketUtils.MapPath(_defaultFileMapPath);
                    var xmlString = FileUtils.ReadFile(filenamepath);
                    Info = new SimplisityInfo();
                    Info.XMLData = xmlString;
                    CacheUtils.SetCache(_defaultFileMapPath, Info);
                }
            }
            catch (Exception)
            {
                CacheUtils.SetCache(_defaultFileMapPath, null);
            }
        }
        public SimplisityInfo Info { get; set; }

        public string Get(string xpath)
        {
            return Info.GetXmlProperty(xpath);
        }
        public Dictionary<string, string> ArticleOrderBy()
        {
            var rtn = new Dictionary<string, string>();
            var nodList = Info.XMLDoc.SelectNodes("root/sqlorderby/product/*");
            if (nodList != null)
            {
                foreach (XmlNode nod in nodList)
                {
                    rtn.Add("sqlorderby-product-" + nod.Name, nod.InnerText);
                }
            }
            return rtn;
        }
        public Dictionary<string, string> PortalShopLinks()
        {
            var rtn = new Dictionary<string, string>();
            var nodList = Info.XMLDoc.SelectNodes("root/pageslinks/*");
            if (nodList != null)
            {
                foreach (XmlNode nod in nodList)
                {
                    rtn.Add(nod.Name, nod.InnerText);
                }
            }
            return rtn;
        }


    }
}
