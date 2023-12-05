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
                DocFolderRel = PortalUtils.HomeDNNrocketDirectoryRel(PortalId).TrimEnd('/') + "/" + systemKey + "/docs";
                DocFolderMapPath = DNNrocketUtils.MapPath(DocFolderRel);
                ImageFolderRel = PortalUtils.HomeDNNrocketDirectoryRel(PortalId).TrimEnd('/') + "/" + systemKey + "/images";
                ImageFolderMapPath = DNNrocketUtils.MapPath(ImageFolderRel);
                CatalogFolderRel = PortalUtils.HomeDNNrocketDirectoryRel(PortalId).TrimEnd('/') + "/" + systemKey;
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
                    Record = _objCtrl.SaveRecord(Record, _tableName); 
                }                
            }
            if (sqlInject)
            {
                LogUtils.LogSystem("SQL INJECTION Attempt:" + Record.XMLData);
                ReadRecord(Record.PortalId, Record.Lang);
            }

            // update PL settings for QueryParams
            if (AppThemeFolder != "")
            {
                var info = _objCtrl.GetRecordByGuidKey(_portalId, -1, "PLSETTINGS", "PLSETTINGS");
                if (info != null)
                {
                    var upd = false;
                    var appTheme = new AppThemeLimpet(PortalId, AppThemeFolder, AppThemeVersion, ProjectName);
                    foreach (var qdata in RocketDirectoryAPIUtils.UrlQueryParams(appTheme))
                    {
                        if (info.GetRecordListItem("queryparams", "genxml/textbox/queryparam", qdata.Key) == null)
                        {
                            var qRec = new SimplisityRecord();
                            qRec.SetXmlProperty("genxml/select/tablename", qdata.Value);
                            qRec.SetXmlProperty("genxml/textbox/queryparam", qdata.Key);
                            info.AddRecordListItem("queryparams", qRec);
                            upd = true;
                        }
                    }
                    if (upd) _objCtrl.Update(info);
                }

            }

            SaveReferenceId(); 

            RemoveCache();
        }
        /// <summary>
        /// Save ID as parentitemid so we can get data from non-system methods, like canonicallink in meta.ascx.
        /// This is a reference to the portalContent settings.
        /// We do not know what the EntityTypeCode is in different systems, so this unifies the name so it can be read.
        /// </summary>
        private void SaveReferenceId()
        {
            var sRec = new SimplisityInfo();
            sRec.PortalId = _portalId;
            sRec.ModuleId = -1;
            sRec.TypeCode = "PortalSettingsRef_" + SystemKey + PortalId;
            sRec.Lang = "";
            sRec.ParentItemId = Record.ItemID;
            _objCtrl.Update(sRec, _tableName);
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
            else
            {
                CacheUtils.SetCache(_cacheKey, Record);
            }
        }
        public void Validate()
        {
            // check for existing page on portal for this system
            //var tabid = PagesUtils.CreatePage(PortalId, _systemKey);
            //PagesUtils.AddPagePermissions(PortalId, tabid, DNNrocketRoles.Manager);
            //PagesUtils.AddPagePermissions(PortalId, tabid, DNNrocketRoles.Editor);
            //PagesUtils.AddPagePermissions(PortalId, tabid, DNNrocketRoles.Collaborator);
            //PagesUtils.AddPageSkin(PortalId, tabid, "rocketportal", "rocketadmin.ascx");
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
            RemoveCache();
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
            var tokenText = "";
            var nosearchText = true;
            foreach (var token in tokenList)
            {
                if (token.ToLower().StartsWith("isinrole:"))
                {
                    var tsplit = token.Split(':');
                    if (tsplit.Count() == 2)
                    {
                        tokenText = UserUtils.IsInRole(tsplit[1]).ToString();
                        nosearchText = false;
                    }
                }
                else
                {
                    var tok = "r/" + token;
                    tokenText = paramInfo.GetXmlProperty(tok).Replace("'", "''");
                    if (tokenText != "") nosearchText = false;
                }
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
        public bool IsPlugin(string interfaceKey)
        {
            var i = Info.GetListItem("plugins", "genxml/hidden/pluginkey", interfaceKey);
            if (i == null) return false;
            return true;
        }
        public bool IsPluginActive(string interfaceKey)
        {
            //if (interfaceKey == "dashboard") return true;
            var i = Info.GetListItem("plugins", "genxml/hidden/pluginkey", interfaceKey);
            if (i == null) return false;
            return i.GetXmlPropertyBool("genxml/checkbox/active");
        }
        public List<SimplisityInfo> PluginActveList(bool useCache = true)
        {
            var cacheKey = PortalId + "*" + SystemKey + "*PluginActveList";
            var rtn = (List<SimplisityInfo>)CacheUtils.GetCache(cacheKey);
            if (rtn == null || !useCache)
            {
                var pList = Info.GetList("plugins").OrderBy(o => o.GetXmlPropertyInt("genxml/config/sortorder")).ToList();
                rtn = new List<SimplisityInfo>();
                foreach (var sInfo in pList)
                {
                    if (sInfo.GetXmlPropertyBool("genxml/checkbox/active")) rtn.Add(sInfo);
                }
                CacheUtils.SetCache(cacheKey, rtn);
            }
            return rtn;
        }
        public List<RocketInterface> PluginActveInterface(SystemLimpet systemData, bool useCache = true)
        {
            var cacheKey = PortalId + "*" + SystemKey + "*PluginActveInterface";
            var rtn = (List<RocketInterface>)CacheUtils.GetCache(cacheKey);
            if (rtn == null || !useCache)
            {
                rtn = new List<RocketInterface>();
                // add default interfaces (not plugin)
                var iList = systemData.GetInterfaceList();
                foreach (var i in iList)
                {
                    rtn.Add(i);
                }

                // ------------------------------------------
                // this is now injected by the basesystem in the SystemSingleton class.
                // ------------------------------------------
                // Add plugins
                //var pList = PluginActveList();
                //foreach (var sInfo in pList)
                //{
                //    var i = systemData.GetInterface(sInfo.GetXmlProperty("genxml/hidden/pluginkey"));
                //    if (i == null)
                //    {
                //        // plugins only exist in rocketdirectoryapi
                //        var systemDirectoryData = SystemSingleton.Instance("rocketdirectoryapi");
                //        i = systemDirectoryData.GetInterface(sInfo.GetXmlProperty("genxml/hidden/pluginkey"));
                //    }
                //    if (rtn != null && i != null && !rtn.Contains(i)) rtn.Add(i);
                //}
                CacheUtils.SetCache(cacheKey, rtn);
            }
            return rtn;
        }
        public string DefaultCmd()
        {
            return "articleadmin_editlist";
        }
        public void ResetSetting()
        {
            var configFileName = DNNrocketUtils.MapPath("/DesktopModules/DNNRocketModules/" + SystemKey + "/Installation/SystemInit.rules");
            if (File.Exists(configFileName))
            {
                var xmlData = FileUtils.ReadFile(configFileName);
                Record.XMLData = xmlData;
            }
        }

        #region "Info - PortalCatalog Data"
        public SimplisityInfo Info { get { return new SimplisityInfo(Record); } }
        public SimplisityRecord Record { get; set; }
        public double MaxArticles { get { return Record.GetXmlPropertyDouble("genxml/maxarticles"); } set { Record.SetXmlProperty("genxml/maxarticles", value.ToString()); } }
        public double ArticleLimit { get { return MaxArticles; } }
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
        public bool FilterByInAll { get { return Record.GetXmlPropertyBool("genxml/checkboxfilterand"); } }
        public bool ArticleOrderBy { get { return Record.GetXmlPropertyBool("genxml/articleorderby"); } }
        public bool Active { get { return true; } }
        public bool EmailActive { get { return Record.GetXmlPropertyBool("genxml/emailon"); } }
        public bool DebugMode { get { if (Record == null) return false; else return Record.GetXmlPropertyBool("genxml/debugmode"); } }
        public bool SecureUpload { get { if (Record == null) return true; else return Record.GetXmlPropertyBool("genxml/secureupload"); } }        
        public int ArticleImageLimit { get { return Record.GetXmlPropertyInt("genxml/articlesimagelimit"); } }
        public int ArticleDocumentLimit { get { return Record.GetXmlPropertyInt("genxml/articlesdocumentlimit"); } }
        public int ListPageTabId { get { return Record.GetXmlPropertyInt("genxml/listpage"); } }
        public int DetailPageTabId { get { return Record.GetXmlPropertyInt("genxml/detailpage"); } }
        public int ImageResize { get { if (Record.GetXmlPropertyInt("genxml/imageresize") > 0) return Record.GetXmlPropertyInt("genxml/imageresize"); else return 640; } }        
        public string ProjectName { get { return Record.GetXmlProperty("genxml/select/selectedprojectnameadmin"); } set { Record.SetXmlProperty("genxml/select/selectedprojectnameadmin", value); } }
        public string AppThemeFolder { get { return Record.GetXmlProperty("genxml/select/appthemeadmin"); } set { Record.SetXmlProperty("genxml/select/appthemeadmin", value); } }
        public string AppThemeVersion { get { return Record.GetXmlProperty("genxml/select/appthemeadminversion"); } set { Record.SetXmlProperty("genxml/select/appthemeadminversion", value); } }
        #endregion

    }
}
