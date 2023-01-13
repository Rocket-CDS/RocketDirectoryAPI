using DNNrocketAPI;
using DNNrocketAPI.Components;
using Rocket.AppThemes.Components;
using RocketPortal.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RocketDirectoryAPI.Components
{
    public class PortalCatalogLimpet
    {
        private const string _tableName = "rocketdirectoryapi";
        private DNNrocketController _objCtrl;
        private int _portalId;
        private string _cacheKey;
        private string _systemKey;
        private string _entityTypeCode;
        public PortalCatalogLimpet(int portalId, string cultureCode, string systemKey)
        {
            Record = new SimplisityRecord();
            if (cultureCode == "") cultureCode = DNNrocketUtils.GetEditCulture();
            _portalId = portalId;
            _systemKey = systemKey;
            _entityTypeCode = systemKey + "PortalCatalog";
            _objCtrl = new DNNrocketController();

            _cacheKey = systemKey + "PortalCatalog" + portalId + "*" + cultureCode;

            Record = (SimplisityRecord)CacheUtils.GetCache(_cacheKey);
            if (Record == null)
            {
                ReadRecord(portalId, cultureCode);

                if (PortalUtils.PortalExists(portalId)) // check we have a portal, could be deleted
                {
                    // create folder on first load.
                    PortalUtils.CreateRocketDirectories(PortalId);
                    if (!Directory.Exists(PortalUtils.HomeDNNrocketDirectoryMapPath(PortalId))) Directory.CreateDirectory(PortalUtils.HomeDNNrocketDirectoryMapPath(PortalId));
                }
            }

            if (PortalUtils.PortalExists(portalId)) 
            {
                // Need to populate, not in cache.
                DocFolderRel = PortalUtils.HomeDNNrocketDirectoryRel(PortalId).TrimEnd('/') + "/rocketdirectoryapi/docs";
                DocFolderMapPath = DNNrocketUtils.MapPath(DocFolderRel);
                ImageFolderRel = PortalUtils.HomeDNNrocketDirectoryRel(PortalId).TrimEnd('/') + "/rocketdirectoryapi/images";
                ImageFolderMapPath = DNNrocketUtils.MapPath(ImageFolderRel);
                CatalogFolderRel = PortalUtils.HomeDNNrocketDirectoryRel(PortalId).TrimEnd('/') + "/rocketdirectoryapi";
                CatalogFolderMapPath = DNNrocketUtils.MapPath(CatalogFolderRel);
                if (!Directory.Exists(CatalogFolderMapPath)) Directory.CreateDirectory(CatalogFolderMapPath);
                if (!Directory.Exists(ImageFolderMapPath)) Directory.CreateDirectory(ImageFolderMapPath);
                if (!Directory.Exists(DocFolderMapPath)) Directory.CreateDirectory(DocFolderMapPath);
            }

        }

        public void Save(SimplisityInfo info)
        {
            Record.XMLData = info.XMLData;
            Update();
        }
        public void Update()
        {
            // check for SQL injection
            var sqlInject = false;
            foreach (var s in GetOrderByList())
            {
                if (SecurityInput.CheckForSQLInjection(s.GetXmlProperty("genxml/value")))
                {
                    sqlInject = true;
                    break;
                }
            }
            if (!sqlInject)
            {
                sqlInject = SecurityInput.CheckForSQLInjection(Record.GetXmlProperty("genxml/sqlfilterarticle"));
                if (!sqlInject)
                {
                    Record = _objCtrl.SaveRecord(Record, _tableName); // you must cache what comes back.  that is the copy of the DB.
                    CacheUtils.SetCache(_cacheKey, Record);
                }                
            }
            if (sqlInject)
            {
                LogUtils.LogSystem("SQL INJECTION Attempt:" + Record.XMLData);
                ReadRecord(Record.PortalId, Record.Lang);
            }
        }
        private void ReadRecord(int portalId, string cultureCode)
        {
            if (cultureCode == "") cultureCode = DNNrocketUtils.GetEditCulture();
            Record = _objCtrl.GetRecordByType(portalId, -1, _entityTypeCode, "", "", _tableName);
            if (Record == null || Record.ItemID <= 0)
            {
                Record = new SimplisityInfo();
                Record.PortalId = _portalId;
                Record.ModuleId = -1;
                Record.TypeCode = _entityTypeCode;
                Record.Lang = cultureCode;
            }
        }
        public void Validate()
        {
            // check for existing page on portal for this system
            var tabid = PagesUtils.CreatePage(PortalId, _systemKey);
            PagesUtils.AddPagePermissions(PortalId, tabid, DNNrocketRoles.Manager);
            PagesUtils.AddPagePermissions(PortalId, tabid, DNNrocketRoles.Editor);
            PagesUtils.AddPagePermissions(PortalId, tabid, DNNrocketRoles.ClientEditor);
            PagesUtils.AddPageSkin(PortalId, tabid, "rocketportal", "rocketadmin.ascx");
        }
        public void Delete()
        {
            _objCtrl.Delete(Record.ItemID, _tableName);

            // remove all portal records.
            var l = _objCtrl.GetList(_portalId, -1, "","","","",0,0,0,0, _tableName);
            foreach (var r in l)
            {
                _objCtrl.Delete(r.ItemID, _tableName);
            }
            CacheUtils.RemoveCache(_cacheKey);
        }
        public void RemoveCache()
        {
            CacheUtils.RemoveCache(_cacheKey);
        }
        private string SqlFilterProduct { get { return Record.GetXmlProperty("genxml/sqlfilterarticle"); } }
        private string GetFilterSQL(string SqlFilterTemplate, SimplisityInfo paramInfo)
        {
            FastReplacer fr = new FastReplacer("{", "}", false);
            fr.Append(SqlFilterTemplate);
            var tokenList = fr.GetTokenStrings();
            var nosearchText = true;
            foreach (var token in tokenList)
            {
                var tok = "r/" + token;
                var tokenText = paramInfo.GetXmlProperty(tok).Replace("'","''");
                if (tokenText != "") nosearchText = false;
                fr.Replace("{" + token + "}", tokenText);
            }
            if (nosearchText) return "";
            var filtersql = " " + fr.ToString() + " ";
            return filtersql;
        }
        public string GetFilterProductSQL(SimplisityInfo paramInfo)
        {
            return GetFilterSQL(SqlFilterProduct, paramInfo);
        }
        public string GetFilterPaymentSQL(SimplisityInfo paramInfo)
        {
            return GetFilterSQL(SqlFilterPayment, paramInfo);
        }
        public string GetFilterOrderSQL(SimplisityInfo paramInfo)
        {
            return GetFilterSQL(SqlFilterOrder, paramInfo);
        }

        public bool IsValidRemote(string securityKey)
        {
            if (Record.GetXmlProperty("genxml/securitykey") == securityKey) return true;
            return false;
        }
        private string SqlFilterOrder { get { return Record.GetXmlProperty("genxml/sqlfilterorder"); } }
        private string SqlFilterPayment { get { return Record.GetXmlProperty("genxml/sqlfilterpayment"); } }
        public string EntityTypeCode { get { return _entityTypeCode; } }

        #region "orderby"
        public List<SimplisityRecord> GetProductOrderByList()
        {
            return Record.GetRecordList("sqlorderbyproduct");
        }
        public string OrderByProductSQL(string orderbyRef = "")
        {
            var rtnsql = GetProductOrderBy(orderbyRef);
            if (rtnsql == "") rtnsql = GetProductOrderBy(0);
            return rtnsql;
        }
        public List<SimplisityRecord> GetOrderByList()
        {
            return Record.GetRecordList("sqlorderbyproduct");
        }
        public void AddProductOrderBy()
        {
            if (Record.ItemID < 0) Update(); // blank record, not on DB.  Create now.
            Record.AddListItem("sqlorderbyproduct");
            Update();
        }
        public string GetProductOrderBy(int idx)
        {
            var info = Record.GetRecordListItem("sqlorderbyproduct", idx);
            if (info == null) return "";
            return info.GetXmlProperty("genxml/value");
        }
        public string GetProductOrderBy(string orderbyref)
        {
            var info = Record.GetRecordListItem("sqlorderbyproduct", "genxml/key", orderbyref);
            if (info == null) return "";
            return info.GetXmlProperty("genxml/value");
        }
        #endregion

        #region "links"
        public List<SimplisityRecord> GetLinkList()
        {
            return Record.GetRecordList("linklist");
        }
        public void AddLink()
        {
            if (Record.ItemID < 0) Update(); // blank record, not on DB.  Create now.
            Record.AddListItem("linklist");
            Update();
        }
        public ArticleLink GetLink(int idx)
        {
            return new ArticleLink(Info.GetListItem("linklist", idx));
        }
        public ArticleLink GetLinkByRef(string linkref)
        {
            return new ArticleLink(Info.GetListItem("linklist","genxml/textbox/ref", linkref));
        }
        public List<ArticleLink> Getlinks()
        {
            var rtn = new List<ArticleLink>();
            foreach (var i in Info.GetList("linklist"))
            {
                rtn.Add(new ArticleLink(i));
            }
            return rtn;
        }
        #endregion

        #region "setting"
        public String GetSetting(String key, String defaultValue = "")
        {
            if (Settings != null && Settings.ContainsKey(key)) return Settings[key];
            return defaultValue;
        }
        public Boolean GetSettingBool(String key, Boolean defaultValue = false)
        {
            try
            {
                if (Settings == null) Settings = new Dictionary<String, String>();
                if (Settings.ContainsKey(key))
                {
                    var x = Settings[key];
                    // bool usually stored as "True" "False"
                    if (x.ToLower() == "true") return true;
                    // Test for 1 as true also.
                    if (GeneralUtils.IsNumeric(x))
                    {
                        if (Convert.ToInt32(x) > 0) return true;
                    }
                    return false;
                }
                return defaultValue;
            }
            catch (Exception ex)
            {
                var ms = ex.ToString();
                return defaultValue;
            }
        }
        private Dictionary<string,string> GetSettings()
        {
            var rtn = new Dictionary<string, string>();
            foreach (var s in Record.GetRecordList("settingsdata"))
            {
                var key = s.GetXmlProperty("genxml/textbox/key");
                if (key != "" && !rtn.ContainsKey(key))
                {
                    rtn.Add(key, s.GetXmlProperty("genxml/textbox/value"));
                }
            }
            return rtn;
        }

        #endregion

        public int ArticleCount
        {
            get
            {
                var countInt = 0;
                var cacheCount = CacheUtils.GetCache("ArticleCount" + PortalId);
                if (cacheCount == null)
                {
                    var l = _objCtrl.GetList(PortalId, -1, "ART", "", CultureCode, "", 0, 0, 0, 0, _tableName);
                    countInt = l.Count;
                    CacheUtils.GetCache("ArticleCountCount" + PortalId, countInt.ToString());
                }
                else
                {
                    countInt = (int)cacheCount;
                }
                return countInt;
            }
        }


        #region "Info - PortalCatalog Data"
        public SimplisityInfo Info { get { return new SimplisityInfo(Record); } }
        public SimplisityRecord Record { get; set; }
        public double MaxArticles { get { return Record.GetXmlPropertyDouble("genxml/maxarticles"); } set { Record.SetXmlProperty("genxml/maxarticles", value.ToString()); } }
        public double ArticleLimit { get { return MaxArticles; } }
        public string WebsiteUrl { get { return Record.GetXmlProperty("genxml/merchant/websiteurl"); } set { Record.SetXmlProperty("genxml/merchant/websiteurl", value.ToString()); } }
        public int PortalId { get { return Record.PortalId; } }
        public bool Exists { get { if (Record.ItemID > 0) return true; else return false; } }
        public string CultureCode { get { return Record.Lang; } }
        public bool ValidCatalog { get { if (Record.GetXmlProperty("genxml/maxarticles") != "") return true; else return false; } }
        public DateTime LastSchedulerTime
        {
            get
            {
                if (GeneralUtils.IsDateInvariantCulture(Record.GetXmlPropertyDate("genxml/lastschedulertime")))
                    return Record.GetXmlPropertyDate("genxml/lastschedulertime");
                else
                    return DateTime.Now.AddDays(-10);
            }
            set { Record.SetXmlProperty("genxml/lastschedulertime", value.ToString(), TypeCode.DateTime); }
        }
        public int SchedulerRunHours
        {
            get
            {
                var rtn = Record.GetXmlPropertyInt("genxml/chedulerrunhours");
                if (Record.GetXmlProperty("genxml/schedulerrunhours") == "") rtn = 24;
                return rtn;
            }
        }
        public string SiteKey { get { return Record.GUIDKey; } set { Record.GUIDKey = value; } }
        public Dictionary<string, string> Settings { get; private set; }
        public string CatalogFolderRel { get; set; }
        public string CatalogFolderMapPath { get; set; }
        public string ImageFolderRel { get; set; }
        public string ImageFolderMapPath { get; set; }
        public string DocFolderRel { get; set; }
        public string DocFolderMapPath { get; set; }
        public string SystemKey { get { return _systemKey; } }
        public string SecurityKey { get { return Record.GetXmlProperty("genxml/securitykey"); } }
        public bool ArticleOrderBy { get { return Record.GetXmlPropertyBool("genxml/articleorderby"); } }
        public bool Active { get { return Record.GetXmlPropertyBool("genxml/active"); } set { Record.SetXmlProperty("genxml/active", value.ToString()); } }
        public bool EmailActive { get { return Record.GetXmlPropertyBool("genxml/emailon"); } }
        public bool DebugMode { get { if (Record == null) return false; else return Record.GetXmlPropertyBool("genxml/debugmode"); } }
        public int ArticleImageLimit { get { return Record.GetXmlPropertyInt("genxml/articlesimagelimit"); } }
        public int ArticleDocumentLimit { get { return Record.GetXmlPropertyInt("genxml/articlesdocumentlimit"); } }
        public string ArticleListPageUrl { get { return Record.GetXmlProperty("genxml/textbox/articlelisturl"); } }
        public string ArticlePagingUrl { get { return Record.GetXmlProperty("genxml/textbox/articlepagingurl"); } }
        public string ArticleDetailPageUrl { get { return Record.GetXmlProperty("genxml/textbox/articledetailurl"); } }
        public int ImageResize { get { if (Record.GetXmlPropertyInt("genxml/imageresize") > 0) return Record.GetXmlPropertyInt("genxml/imageresize"); else return 640; } }
        public string ProjectName { get { return Record.GetXmlProperty("genxml/select/selectedprojectname"); } set { Record.SetXmlProperty("genxml/select/selectedprojectname", value); } }
        public string AppThemeAdminFolder { get { return Record.GetXmlProperty("genxml/select/appthemeadmin"); } }
        public string AppThemeAdminVersion { get { return Record.GetXmlProperty("genxml/select/appthemeversion"); } }


        #endregion

    }
}
