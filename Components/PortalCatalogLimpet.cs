using DNNrocketAPI;
using DNNrocketAPI.Components;
using Rocket.AppThemes.Components;
using RocketPortal.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RocketDirectoryAPI.Components
{
    /// <summary>
    /// System data and settings.
    /// </summary>
    public class PortalCatalogLimpet
    {
        private string _tableName = "RocketDirectoryApi";
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
                CacheUtils.SetCache(_cacheKey, Record);

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
            UpdateCurrencyCode(CurrencyCultureCode);
            Update();
        }
        public void UpdateCurrencyCode(string currencyCultureCode)
        {
            CurrencyCultureCode = currencyCultureCode;
            //currency data
            var cultureInfo = new CultureInfo(CurrencyCultureCode, false);
            NumberFormatInfo nfi = cultureInfo.NumberFormat;
            CurrencyDecimalDigits = nfi.CurrencyDecimalDigits;
            CurrencyDecimalSeparator = nfi.CurrencyDecimalSeparator;
            CurrencyGroupSeparator = nfi.CurrencyGroupSeparator;
            CurrencySymbol = nfi.CurrencySymbol;
            var ri = new RegionInfo(cultureInfo.LCID);
            CurrencyCode = ri.ISOCurrencySymbol;
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
                if (info == null)
                {
                    info = new SimplisityRecord();
                    info.ItemID = -1;
                    info.PortalId = _portalId;
                    info.TypeCode = "PLSETTINGS";
                    info.GUIDKey = "PLSETTINGS";
                }
                if (info != null)
                {
                    var upd = false;
                    var appTheme = new AppThemeLimpet(PortalId, AppThemeFolder, AppThemeVersion, ProjectName);

                    // Add Query Params for Article and Categories
                    var newqueryParamList = new List<SimplisityRecord>();
                    foreach (var qdata in RocketDirectoryAPIUtils.UrlQueryParams(appTheme))
                    {
                        if (info.GetRecordListItem("queryparams", "genxml/textbox/queryparam", qdata.Key) == null)
                        {
                            var qRec = new SimplisityRecord();
                            qRec.SetXmlProperty("genxml/select/tablename", qdata.Value.tablename);
                            qRec.SetXmlProperty("genxml/select/datatype", qdata.Value.datatype);
                            qRec.SetXmlProperty("genxml/textbox/queryparam", qdata.Value.queryparam);
                            qRec.SetXmlProperty("genxml/textbox/systemkey", qdata.Value.systemkey);
                            newqueryParamList.Add(qRec);
                            upd = true;
                        }
                    }
                    if (upd)
                    {
                        // Remove duplicate Query Params
                        foreach (var newq in newqueryParamList)
                        {
                            var idx = 0;
                            foreach (var q in info.GetRecordList("queryparams"))
                            {
                                if (q.GetXmlProperty("genxml/select/datatype") == newq.GetXmlProperty("genxml/select/datatype") && q.GetXmlProperty("genxml/textbox/systemkey") == newq.GetXmlProperty("genxml/textbox/systemkey"))
                                {
                                    info.RemoveRecordListItem("queryparams", idx);
                                    break; // should only be 1.
                                }
                                idx += 1;
                            }
                        }
                        foreach (var newq in newqueryParamList)
                        {
                            info.AddRecordListItem("queryparams", newq);
                        }
                    }
                    // Add Menu Provider
                    foreach (var menuproviderData in RocketDirectoryAPIUtils.MenuProvider(appTheme))
                    {
                        if (info.GetRecordListItem("menuprovider", "genxml/textbox/systemkey", menuproviderData.Key) == null)
                        {
                            var mRec = new SimplisityRecord();
                            mRec.SetXmlProperty("genxml/textbox/assembly", menuproviderData.Value.assembly);
                            mRec.SetXmlProperty("genxml/textbox/namespaceclass", menuproviderData.Value.namespaceclass);
                            mRec.SetXmlProperty("genxml/textbox/systemkey", menuproviderData.Value.systemkey);
                            info.AddRecordListItem("menuprovider", mRec);
                            upd = true;
                        }
                    }

                    if (upd) _objCtrl.Update(info, _tableName);
                }
            }

            // Do not force search tab, it is not compatible with the way DNN9 works.
            //if (SearchModuleId > 0 && SearchPageTabId > 0) PortalUtils.SetSearchTabId(PortalId, SearchPageTabId);
            
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
            var sRec = DNNrocketUtils.GetPortalContentRecByRefId(PortalId, SystemKey, _tableName);
            if (sRec == null)
            {
                var entityType = "PortalSettingsRef_" + SystemKey + PortalId;
                sRec = new SimplisityInfo();
                sRec.PortalId = _portalId;
                sRec.ModuleId = -1;
                sRec.TypeCode = entityType;
                sRec.GUIDKey = entityType;
                sRec.Lang = "";
                sRec.ParentItemId = Record.ItemID;
                _objCtrl.Update(sRec, _tableName);
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
        public void Reset()
        {
            // delete legacy
            var sqlCmd = "delete from DNNrocket where typecode = '" + _entityTypeCode + "' and portalId = " + PortalId + " ";
            _objCtrl.ExecSql(sqlCmd);

            // delete existing
            var sqlCmd3 = "delete from " + _tableName + " where typecode = '" + _entityTypeCode + "' and portalId = " + PortalId + " ";
            _objCtrl.ExecSql(sqlCmd3);

            // Delete crossref
            var entityType = "PortalSettingsRef_" + SystemKey + PortalId;
            var sqlCmd2 = "delete from " + _tableName + " where typecode = '" + entityType + "' and portalId = " + PortalId + " ";
            _objCtrl.ExecSql(sqlCmd2);

            var configFileName = DNNrocketUtils.MapPath("/DesktopModules/DNNRocketModules/" + SystemKey + "/Installation/SystemInit.rules");
            if (File.Exists(configFileName))
            {
                var xmlData = FileUtils.ReadFile(configFileName);
                Record.XMLData = xmlData;
            }
            Record.ItemID = _objCtrl.Update(new SimplisityInfo(Record), _tableName);
            SaveReferenceId();
            RemoveCache();
        }
        public void RemoveCache()
        {
            CacheUtils.RemoveCache(_cacheKey);
        }
        private string SqlFilterProduct { get { return Record.GetXmlProperty("genxml/sqlfilterarticle"); } }
        private string SqlFilterProductAdmin { get { return Record.GetXmlProperty("genxml/sqlfilterarticleadmin"); } }
        private string GetFilterSQL(string SqlFilterTemplate, SimplisityInfo paramInfo, string systemKey)
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
                } else if (token.ToLower().StartsWith("contains:"))
                {
                    var tsplit = token.Split(':');
                    if (tsplit.Count() == 2)
                    {
                        tokenText = paramInfo.GetXmlProperty("r/" + tsplit[1]);
                        if (!String.IsNullOrEmpty(tokenText))
                        {
                            var searchmodel = new SearchPostModel();
                            searchmodel.SearchInput = tokenText;
                            searchmodel.PageIndex = 1;
                            searchmodel.PageSize = 200;
                            var searchResults = SearchUtils.DoSearch(PortalId, searchmodel);
                            string inClause = "";
                            foreach (string searchKey in searchResults)
                            {
                                var searchKeys = searchKey.Split('_');
                                if (searchKeys.Count() >= 3 && searchKeys[0] == systemKey)
                                {
                                    var itemid = searchKeys[2];
                                    if (GeneralUtils.IsNumeric(itemid))
                                    {
                                        if (!string.IsNullOrEmpty(inClause)) inClause += ", ";
                                        inClause += itemid;
                                    }
                                }
                            }
                            if (inClause == "") inClause = "0";
                            tokenText = " AND [R1].ItemId IN (" + inClause + ") ";
                            nosearchText = false;
                        }
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
        public string GetFilterProductSQL(SimplisityInfo paramInfo, string systemKey, bool isAdmin)
        {
            if (!isAdmin)
                return GetFilterSQL(SqlFilterProduct.Replace(":searchtext}", ":viewsearchtext}"), paramInfo, systemKey);
            else
                return GetFilterSQL(SqlFilterProductAdmin.Replace(":viewsearchtext}", ":searchtext}"), paramInfo, systemKey);
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
                // this is now injected by the basesystem (DNNrocket) in the SystemSingleton.cs class.
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

        #region "Currency"
        private bool IsNumeric(string value)
        {
            int i = 0;
            return int.TryParse(value, out i); //i now = value 
        }
        public int CurrencyConvertCents(string value)
        {
            var rtn = CurrencyConvertToCulture(value);
            var rtnStr = Regex.Replace(rtn.ToString(), "[^0-9]", ""); // remove ALL non numeric
            if (IsNumeric(rtnStr)) return Convert.ToInt32(rtnStr);
            return 0;
        }
        public string CurrencyEdit(int intValue)
        {
            return CurrencyCentsToDollars(intValue).ToString();
        }
        public string CurrencyDisplay(decimal value)
        {
            return value.ToString("C", CultureInfo.GetCultureInfo(CurrencyCultureCode));
        }
        public decimal CurrencyCentsToDollars(int cents)
        {
            // DCL: Unsure why we make everything positive, I think it was a mistake.  
            //var minus = false;
            //if (cents < 0) minus = true;

            var multiplyer = "1";
            var lp = 0;
            while (lp < CurrencyDecimalDigits)
            {
                multiplyer += "0";
                lp += 1;
            }
            var rtn = Convert.ToDecimal(cents) / Convert.ToDecimal(multiplyer);
            //if (minus) rtn = (rtn * -1);
            return CurrencyConvertToCulture(rtn.ToString());
        }
        public decimal CurrencyConvertToCulture(string value)
        {
            // Reformat the amount, to ensure it is a valid currency. 
            // very often the entered decimal seperator is the group seperator.
            // so we convert to try and help, but may still be wrong.  
            // We remove all non-numeric and then enter the decimal seperator at the correct place for the shop currencyculturecode.
            // !!! There is probably a better way to do this !!!
            var minus = false;
            if (value.TrimStart(' ').StartsWith("-")) minus = true;
            if (IsNumeric(value))
            {
                // FIX: 78  --> 78.00
                // no decimal seperator, pad the string to allow for that.
                string padzero = new String('0', CurrencyDecimalDigits);
                value = value + padzero;
            }
            else
            {
                // FIX: 78.3  --> 78.30
                // find out how many decimal numbers after seperator.
                // We do not know what the sepeartor is, so take the last non-numeric as the seperator. (Reverse loop, so first in code.)
                var seperatorCount = 0;
                for (int i = 0; i < value.Length; i++)
                {
                    if (!char.IsNumber(value[i])) seperatorCount = (value.Length - i) - 1;
                }
                if (seperatorCount < CurrencyDecimalDigits)
                {
                    value = value.PadRight(value.Length + (CurrencyDecimalDigits - seperatorCount), '0');
                }
            }
            value = Regex.Replace(value, "[^0-9]", ""); // remove ALL non numeric
            if (value.Length < (CurrencyDecimalDigits + 1)) value = value.PadRight((CurrencyDecimalDigits + 1), '0');
            var newamount = "";
            for (int i = 0; i < value.Length; i++)
            {
                if ((value.Length - i) == (CurrencyDecimalDigits))
                {
                    newamount += CurrencyDecimalSeparator;
                }
                newamount += value[i];
            }
            var rtn = Convert.ToDecimal(newamount, CultureInfo.GetCultureInfo(CurrencyCultureCode));
            if (minus) rtn = (rtn * -1);
            return rtn;
        }
        public string CurrencyCultureCode
        {
            get
            {
                var rtn = Record.GetXmlProperty("genxml/currencyculturecode");
                if (rtn == "") rtn = DNNrocketUtils.GetCurrentCulture();
                return rtn;
            }
            set { Record.SetXmlProperty("genxml/currencyculturecode", value.ToString()); }
        }
        public int CurrencyDecimalDigits { get { return Record.GetXmlPropertyInt("genxml/currencydecimaldigits"); } set { Record.SetXmlProperty("genxml/currencydecimaldigits", value.ToString()); } }
        public string CurrencyDecimalSeparator { get { return Record.GetXmlProperty("genxml/currencydecimalseparator"); } set { Record.SetXmlProperty("genxml/currencydecimalseparator", value.ToString()); } }
        public string CurrencyGroupSeparator { get { return Record.GetXmlProperty("genxml/currencygroupseparator"); } set { Record.SetXmlProperty("genxml/currencygroupseparator", value.ToString()); } }
        public string CurrencySymbol { get { return Record.GetXmlProperty("genxml/currencysymbol"); } set { Record.SetXmlProperty("genxml/currencysymbol", value.ToString()); } }
        public string CurrencyCode { get { return Record.GetXmlProperty("genxml/currencycode"); } set { Record.SetXmlProperty("genxml/currencycode", value.ToString()); } }


        #endregion

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
        public int SearchPageTabId { get { return Record.GetXmlPropertyInt("genxml/searchpage"); } }
        public int ImageResize { get { if (Record.GetXmlPropertyInt("genxml/imageresize") > 0) return Record.GetXmlPropertyInt("genxml/imageresize"); else return 640; } }        
        public string ProjectName { get { return Record.GetXmlProperty("genxml/select/selectedprojectnameadmin"); } set { Record.SetXmlProperty("genxml/select/selectedprojectnameadmin", value); } }
        public string AppThemeFolder { get { return Record.GetXmlProperty("genxml/select/appthemeadmin"); } set { Record.SetXmlProperty("genxml/select/appthemeadmin", value); } }
        public string AppThemeVersion { get { return Record.GetXmlProperty("genxml/select/appthemeadminversion"); } set { Record.SetXmlProperty("genxml/select/appthemeadminversion", value); } }
        public int SearchModuleId { get { return Record.GetXmlPropertyInt("genxml/searchmoduleid"); } set { Record.SetXmlProperty("genxml/searchmoduleid", value.ToString()); } }
        #endregion

    }
}
