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
using System.Web;

namespace RocketDirectoryAPI.Components
{
    public class PropertyLimpet
    {
        public const string _tableName = "RocketDirectoryAPI";
        private string _systemKey;
        private string _cacheKey;

        private DNNrocketController _objCtrl;
        /// <summary>
        /// Should be used to create an article, the portalId is required on creation
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="articleId"></param>
        /// <param name="langRequired"></param>
        public PropertyLimpet(int portalId, int propertyId, string langRequired, string systemKey)
        {
            _cacheKey = "PropertyLimpet*" + portalId + "*" + propertyId + "*" + langRequired + "*" + systemKey;
            PortalId = portalId;
            EntityTypeCode = systemKey + "PROP";
            TableName = _tableName;
            CultureCode = langRequired;
            _systemKey = systemKey;
            Populate(propertyId);
        }
        private void Populate(int propertyId)
        {
            _objCtrl = new DNNrocketController();

            Info = (SimplisityInfo)CacheUtilsDNN.GetCache(_cacheKey);
            if (Info == null)
            {
                Info = _objCtrl.GetInfo(propertyId, CultureCode, TableName); // get existing record.
                if (Info == null) // check if we have a real record.
                {
                    Info = new SimplisityInfo();
                    Info.ItemID = -1;
                    Info.TypeCode = EntityTypeCode;
                    Info.ModuleId = -1;
                    Info.UserId = -1;
                    Info.PortalId = PortalId;
                    Info.Lang = CultureCode;
                }
                else
                {
                    CacheUtilsDNN.SetCache(_cacheKey, Info);
                }
                Info.Lang = CultureCode; // reset langauge, for legacy record, without lang.
                PortalId = Info.PortalId;
            }
            PortalCatalog = new PortalCatalogLimpet(PortalId, CultureCode, SystemKey);
        }
        public void Delete()
        {
            if (Info.ItemID > 0)
            {
                // remove property xref
                var catxrefList = _objCtrl.GetList(PortalId, -1, "CATXREF", " and [XrefItemId] = " + PropertyId + " ", "", "", 0, 0, 0, 0, TableName);
                foreach (var catxrefRecord in catxrefList)
                {
                    _objCtrl.Delete(catxrefRecord.ItemID, TableName);
                }
                _objCtrl.Delete(Info.ItemID, TableName);
                CacheUtilsDNN.RemoveCache(_cacheKey);
            }
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
        public int Save(SimplisityInfo postInfo)
        {
            ReplaceInfoFields(postInfo, "genxml/textbox/*");
            ReplaceInfoFields(postInfo, "genxml/lang/genxml/textbox/*");
            ReplaceInfoFields(postInfo, "genxml/checkbox/*");
            ReplaceInfoFields(postInfo, "genxml/lang/genxml/checkbox/*");
            ReplaceInfoFields(postInfo, "genxml/select/*");
            ReplaceInfoFields(postInfo, "genxml/radio/*");

            Info.RemoveXmlNode("genxml/checkboxlist");
            Info.AddXmlNode(postInfo.GetXmlNode("genxml/checkboxlist"), "group", "genxml/checkboxlist");
            Info.GUIDKey = Ref;
            Info.SortOrder = Info.GetXmlPropertyInt("genxml/textbox/sortorder");
            var testInfo = _objCtrl.GetByGuidKey(PortalId, -1, EntityTypeCode, Info.GUIDKey, "", _tableName, CultureCode);
            if (testInfo != null && testInfo.ItemID != Info.ItemID)
                return -1;
            else
                return ValidateAndUpdate();
        }
        public List<SimplisityInfo> GetArticlesInfo()
        {
            var searchFilter = " and [PROPXREF].[XrefItemId] = " + PropertyId  + " ";
            return _objCtrl.GetList(PortalCatalog.PortalId, -1, SystemKey + "ART", searchFilter, CultureCode, "", 100, 0, 0, 0, _tableName);
        }
        public int Update()
        {
            Info = _objCtrl.SaveData(Info, TableName);
            CacheUtilsDNN.RemoveCache(_cacheKey);
            // clear portal cache, so list so change.
            CacheUtils.ClearAllCache("portal" + PortalId);
            return Info.ItemID;
        }
        public int ValidateAndUpdate()
        {
            Validate();
            return Update();
        }
        public void Validate()
        {
        }
        public List<string> PropertyGroups()
        {
            var rtn = new List<string>();
            var nodList = Info.XMLDoc.SelectNodes("genxml/checkboxlist/group/chk");
            if (nodList != null)
            {
                foreach (XmlNode nod in nodList)
                {
                    if (nod.Attributes["value"].InnerText.ToLower() == "true")
                    {
                        rtn.Add(nod.Attributes["data"].InnerText);
                    }
                }
            }
            return rtn;
        }
        public bool IsInGroup(string groupref)
        {
            return Info.GetXmlPropertyBool("genxml/checkboxlist/group/chk[@data='" + groupref + "']/@value");
        }

        #region "properties"

        public string CultureCode { get; set; }
        public string EntityTypeCode { get; set; }
        public SimplisityInfo Info { get; set; }
        public int ModuleId { get { return Info.ModuleId; } set { Info.ModuleId = value; } }
        public int XrefItemId { get { return Info.XrefItemId; } set { Info.XrefItemId = value; } }
        public int PropertyId { get { return Info.ItemID; } set { Info.ItemID = value; } }
        public string GUIDKey { get { return Info.GUIDKey; } set { Info.GUIDKey = value; } }
        public int SortOrder { get { return Info.SortOrder; } set { Info.SortOrder = value; } }
        public string ImageFolder { get; set; }
        public int PortalId { get; set; }
        public bool Exists { get { if (Info.ItemID <= 0) { return false; } else { return true; }; } }
        public string TableName { get; set; }
        public int Level { get { return Info.GetXmlPropertyInt("genxml/textbox/level"); } set { Info.SetXmlProperty("genxml/textbox/level", value.ToString()); } }
        public string ArticleListName { get { return "articlelist"; } }
        public PortalCatalogLimpet PortalCatalog { get; set; }
        public string Ref { get { return Info.GetXmlProperty(RefXPath); } }
        public string RefXPath { get { return "genxml/textbox/ref"; } }
        public string RichText { get { return Info.GetXmlProperty(RichTextXPath); } }
        public string RichTextXPath { get { return "genxml/lang/genxml/textbox/propertyrichtext"; } }
        public string Name { get { return Info.GetXmlProperty(NameXPath); } }
        public string NameXPath { get { return "genxml/lang/genxml/textbox/name"; } }
        public string Summary { get { return Info.GetXmlProperty(SummaryXPath); } }
        public string SummaryXPath { get { return "genxml/lang/genxml/textbox/summary"; } }
        public string Keywords { get { return Info.GetXmlProperty(KeywordsXPath); } }
        public string KeywordsXPath { get { return "genxml/lang/genxml/textbox/propertykeywords"; } }
        public bool Hidden { get { return Info.GetXmlPropertyBool("genxml/checkbox/hidden"); } }
        public bool HiddenByCulture { get { return Info.GetXmlPropertyBool("genxml/lang/genxml/checkbox/hidden"); } }
        public bool IsHidden { get { if (Hidden || HiddenByCulture) return true; else return false; } }
        public string SystemKey { get { return _systemKey; } }
        #endregion

    }

}
