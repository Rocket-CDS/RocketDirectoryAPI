using DNNrocketAPI;
using DNNrocketAPI.Components;
using Rocket.AppThemes.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace RocketDirectoryAPI.Components
{
    public class CatalogSettingsLimpet
    {
        public const string _tableName = "RocketDirectoryAPI";

        private DNNrocketController _objCtrl;
        private int _portalId;
        private string _cacheKey;

        public CatalogSettingsLimpet(int portalId, string cultureCode, string systemKey)
        {
            Info = new SimplisityInfo();
            if (cultureCode == "") cultureCode = DNNrocketUtils.GetEditCulture();
            CultureCode = cultureCode;
            _portalId = portalId;
            SystemKey = systemKey;

            _objCtrl = new DNNrocketController();

            var entityTypeCode = SystemKey + "CATALOGSETTINGS";
            _cacheKey = entityTypeCode + portalId + "*" + cultureCode;

            Info = (SimplisityInfo)CacheUtils.GetCache(_cacheKey);
            if (Info == null)
            {
                Info = _objCtrl.GetByType(portalId, -1, entityTypeCode, "", CultureCode, _tableName);
                if (Info == null || Info.ItemID <= 0)
                {
                    Info = new SimplisityInfo();
                    Info.PortalId = _portalId;
                    Info.ModuleId = -1;
                    Info.TypeCode = entityTypeCode;
                    Info.Lang = cultureCode;
                }
            }
        }

        public void Save(SimplisityInfo postInfo)
        {
            ReplaceInfoFields(postInfo, "genxml/textbox/*");
            ReplaceInfoFields(postInfo, "genxml/lang/genxml/textbox/*");
            ReplaceInfoFields(postInfo, "genxml/checkbox/*");
            ReplaceInfoFields(postInfo, "genxml/lang/genxml/checkbox/*");
            ReplaceInfoFields(postInfo, "genxml/select/*");
            ReplaceInfoFields(postInfo, "genxml/lang/genxml/select/*");
            ReplaceInfoFields(postInfo, "genxml/radio/*");
            ReplaceInfoFields(postInfo, "genxml/lang/genxml/radio/*");

            Info.RemoveList("grouplist");
            foreach (var g in postInfo.GetList("grouplist"))
            {
                g.SetXmlProperty("genxml/textbox/ref", g.GetXmlProperty("genxml/textbox/ref").Replace(" ", "-").Trim());
                var groupRef = g.GetXmlProperty("genxml/textbox/ref");
                if (groupRef != "") AddGroup(g);
            }

            Update();
        }
        public List<SimplisityInfo> GroupList()
        {
            return Info.GetList("grouplist");
        }
        public void AddGroup(SimplisityInfo groupInfo)
        {
            Info.AddListItem("grouplist", groupInfo);
        }
        private void ReplaceInfoFields(SimplisityInfo postInfo, string xpathListSelect)
        {
            var textList = Info.XMLDoc.SelectNodes(xpathListSelect);
            if (textList != null)
            {
                foreach (XmlNode nod in textList)
                {
                    Info.RemoveXmlNode(xpathListSelect.Replace("*", "") + nod.Name);
                }
            }
            textList = postInfo.XMLDoc.SelectNodes(xpathListSelect);
            if (textList != null)
            {
                foreach (XmlNode nod in textList)
                {
                    Info.SetXmlProperty(xpathListSelect.Replace("*", "") + nod.Name, nod.InnerText);
                }
            }
        }

        public void Update()
        {
            Info = _objCtrl.SaveData(Info, _tableName);
            CacheUtils.SetCache(_cacheKey, Info);
        }
        public void Delete()
        {
            _objCtrl.Delete(Info.ItemID, _tableName);
            CacheUtils.RemoveCache(_cacheKey);
        }
        public Dictionary<string,string> GetPropertyGroups()
        {
            var rtn = new Dictionary<string, string>();
            foreach (var g in GroupList())
            {
                var r = g.GetXmlProperty("genxml/textbox/ref");
                var v = g.GetXmlProperty("genxml/lang/genxml/textbox/name");
                if (v == "") v = r;
                rtn.Add(r, v);
            }
            return rtn;
        }
        public string EntityTypeCode { get { return Info.GUIDKey; } }
        public SimplisityInfo Info { get; set; }
        public int PortalId { get { return Info.PortalId; } }
        public bool Exists { get { if (Info.ItemID > 0) return true; else return false; }  }
        public string CultureCode { get; set; }
        public string CatalogName { get { return Info.GetXmlProperty("genxml/textbox/catalogname"); } }
        public string FromEmail { get { return Info.GetXmlProperty("genxml/textbox/fromemail"); } }
        public string ContactEmail { get { return Info.GetXmlProperty("genxml/textbox/contactemail"); } }
        public string TechnicalEmail { get { return Info.GetXmlProperty("genxml/textbox/technicalemail"); } }
        public string PropertyGroups { get { return Info.GetXmlProperty("genxml/textbox/propertygroups"); } }
        public bool ManualCategoryOrderby { get { return Info.GetXmlPropertyBool("genxml/checkbox/manualcategoryorderby"); } }

        // use /config/ so it does not get overwritten on save.
        public int DefaultCategoryId { get { return Info.GetXmlPropertyInt("genxml/config/defaultcategoryid"); } set { Info.SetXmlPropertyInt("genxml/config/defaultcategoryid", value); } }
        public string SystemKey { get { return Info.GetXmlProperty("genxml/systemkey"); } set { Info.SetXmlProperty("genxml/systemkey", value); } }
    }
}
