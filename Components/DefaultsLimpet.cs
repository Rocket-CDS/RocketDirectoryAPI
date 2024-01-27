using DNNrocketAPI.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace RocketDirectoryAPI.Components
{
    public class DefaultsLimpet
    {
        private string _defaultFileMapPath;
        private string _systemKey;
        public DefaultsLimpet(string systemKey)
        {
            _systemKey = systemKey;
            var cahceKey = systemKey + "*RocketDirectoryAPI*SystemDefaults.rules";
            try
            {
                Info = (SimplisityInfo)CacheUtils.GetCache(cahceKey);
                if (Info == null)
                {
                    _defaultFileMapPath = "/DesktopModules/DNNrocketModules/" + systemKey + "/Installation/SystemDefaults.rules";
                    if (!File.Exists(DNNrocketUtils.MapPath(_defaultFileMapPath)))
                    {
                        _defaultFileMapPath = "/DesktopModules/DNNrocketModules/RocketDirectoryAPI/Installation/SystemDefaults.rules";
                    }
                    var filenamepath = DNNrocketUtils.MapPath(_defaultFileMapPath);
                    var xmlString = FileUtils.ReadFile(filenamepath);
                    Info = new SimplisityInfo();
                    Info.XMLData = xmlString;
                    CacheUtils.SetCache(cahceKey, Info);
                }
            }
            catch (Exception)
            {
                CacheUtils.SetCache(cahceKey, null);
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
        public string SystemKey { get { return _systemKey; } }

    }
}
