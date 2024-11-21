using DNNrocketAPI;
using DNNrocketAPI.Components;
using Simplisity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;

namespace RocketDirectoryAPI.Components
{

    public class ArticleLimpetList
    {
        private string _langRequired;
        private List<ArticleLimpet> _articleList;
        public const string _tableName = "RocketDirectoryAPI";
        private string _entityTypeCode;
        private DNNrocketController _objCtrl;
        private string _searchFilter;
        private int _searchcategoryid;
        private int _catidurl;
        private int _catid;
        private string _systemKey;
        private string _orderby;

        public ArticleLimpetList(int categoryId, PortalCatalogLimpet portalCatalog, string langRequired, bool populate)
        {
            PortalCatalog = portalCatalog;
            _systemKey = PortalCatalog.SystemKey;
            _entityTypeCode = _systemKey + "ART";
            _langRequired = langRequired;
            if (_langRequired == "") _langRequired = DNNrocketUtils.GetCurrentCulture();
            _objCtrl = new DNNrocketController();
            CatalogSettings = new CatalogSettingsLimpet(portalCatalog.PortalId, _langRequired, _systemKey);

            var paramInfo = new SimplisityInfo();
            SessionParamData = new SessionParams(paramInfo);
            SessionParamData.PageSize = 0;

            _searchcategoryid = categoryId;
            _catid = categoryId;

            if (populate) Populate();
        }
        public ArticleLimpetList(SessionParams sessionParams, PortalCatalogLimpet portalCatalog, string langRequired, bool populate, bool showHidden = true, int defaultCategoryId = 0)
        {
            InitArticleList(sessionParams, portalCatalog, langRequired, populate, showHidden, defaultCategoryId);
        }
        private void InitArticleList(SessionParams sessionParams, PortalCatalogLimpet portalCatalog, string langRequired, bool populate, bool showHidden = true, int defaultCategoryId = 0)
        {
            SessionParamData = sessionParams;
            PortalCatalog = portalCatalog;
            _systemKey = PortalCatalog.SystemKey;
            _entityTypeCode = _systemKey + "ART";
            _langRequired = langRequired;
            if (_langRequired == "") _langRequired = DNNrocketUtils.GetCurrentCulture();
            _objCtrl = new DNNrocketController();
            CatalogSettings = new CatalogSettingsLimpet(portalCatalog.PortalId, sessionParams.CultureCode, _systemKey);

            if (sessionParams.PageSize == 0) sessionParams.PageSize = 24;
            if (sessionParams.Page <= 0) sessionParams.Page = 1;

            _catid = sessionParams.GetInt(RocketDirectoryAPIUtils.UrlQueryCategoryKey(portalCatalog.PortalId, _systemKey));
            _catidurl = _catid;
            if (_catid == 0) _catid = defaultCategoryId;
            _searchcategoryid = _catid;

            if (sessionParams.OrderByRef == "" && _catid == 0) sessionParams.OrderByRef = "sqlorderby-article-name";

            if (populate) Populate(showHidden, sessionParams.Get("sqladminfilter"));
        }
        public void Populate(bool showHidden = true, string sqladminfilter = "")
        {
            _searchFilter = "";
            ClearFilter = false;
            ClearCategory = false;

            var searchText = PortalCatalog.GetFilterProductSQL(SessionParamData.Info, _systemKey, showHidden);
            var propertyFilter = "";
            if (showHidden)
                _catid = 0;  //assume showHidden is admin.
            else
                propertyFilter = GetPropertyFilterSQL();


            _searchFilter += searchText;
            _searchFilter += propertyFilter;
            if (_searchcategoryid > 0 && searchText == "") _searchFilter += " and [CATXREF].[XrefItemId] = " + _searchcategoryid + " ";

            // Filter hidden
            if (!showHidden)
            {
                _searchFilter += " and NOT(isnull([XMLData].value('(genxml/checkbox/hidden)[1]','nvarchar(4)'),'false') = 'true') and NOT(isnull([XMLData].value('(genxml/lang/genxml/checkbox/hidden)[1]','nvarchar(4)'),'false') = 'true') ";
            }
            else
            {
                _searchFilter += sqladminfilter;
            }

            _orderby = "";
            if (_searchcategoryid > 0 && CatalogSettings.ManualCategoryOrderby)
                _orderby = " order by [CATXREF].[SortOrder] "; // use manual sort for articles by category;
            else
                _orderby = PortalCatalog.OrderByProductSQL(SessionParamData.OrderByRef);

            if (showHidden && _searchcategoryid <= 0)
            {
                // Assume admin if showhidden.
                _orderby = PortalCatalog.OrderByProductSQL(PortalCatalog.Info.GetXmlProperty("genxml/hidden/adminorderbyref"));
            }

            if (_orderby == "") _orderby = " order by articlename.GUIDKey ";

            if (searchText.ToLower().Contains("in")) // check the searchtext filter only, so property filters work.
            {
                //DNN search used, the SPROC is not compatible.
                var list = new List<SimplisityInfo>();
                try
                {
                    var sql = "select * from {databaseOwner}[{objectQualifier}" + _tableName + "] as R1 where [R1].PortalId = " + PortalCatalog.PortalId;
                    var sql2 = sql + " " + _searchFilter;
                    list = _objCtrl.ExecSqlList(sql2);
                }
                catch (Exception ex)
                {
                    LogUtils.LogException(ex);
                }

                SessionParamData.RowCount = list.Count();
                DataList = list.Skip((SessionParamData.Page - 1) * SessionParamData.PageSize).Take(SessionParamData.PageSize).ToList();
            }
            else
            {
                // do not cache the result.  If a search is made then the cache will be a problem.
                SessionParamData.RowCount = _objCtrl.GetListCount(PortalCatalog.PortalId, -1, _entityTypeCode, _searchFilter, _langRequired, _tableName);
                RecordCount = SessionParamData.RowCount;
                DataList = _objCtrl.GetList(PortalCatalog.PortalId, -1, _entityTypeCode, _searchFilter, _langRequired, _orderby, 0, SessionParamData.Page, SessionParamData.PageSize, SessionParamData.RowCount, _tableName);
                //LogUtils.LogSystem("RocketDirectoryAPIUtils.GetArticleData: " + cacheKey);
            }
        }
        public Dictionary<DateTime, List<SimplisityInfo>> GetArticlesByMonth(DateTime startMonthDate, int numberOfMonths, string sqlindexDateRef = "", int catid = 0, int limit = 1000)
        {
            var rtn = new Dictionary<DateTime, List<SimplisityInfo>>();
            var startDate = new DateTime(startMonthDate.Year, startMonthDate.Month, 1).AddMonths(1).AddDays(-1);
            var systemData = new SystemLimpet(_systemKey);
            var sqlIndexRec = systemData.GetSqlIndex(sqlindexDateRef);
            var ed = startDate;
            var articleList = GetArticlesByDateDesc(startMonthDate, numberOfMonths, sqlindexDateRef, catid, limit);
            foreach (var a in articleList)
            {
                DateTime d = a.ModifiedDate;
                if (sqlindexDateRef != "" && sqlIndexRec != null)
                {
                    var xpath = sqlIndexRec.GetXmlProperty("genxml/xpath");
                    d = a.GetXmlPropertyDate(xpath);
                }
                var sd = new DateTime(d.Year, d.Month, 1);
                if (rtn.ContainsKey(sd))
                {
                    var l = rtn[sd];
                    l.Add(a);
                    rtn.Remove(sd);
                    rtn.Add(sd, l);
                }
                else
                {
                    var l = new List<SimplisityInfo>();
                    l.Add(a);
                    rtn.Add(sd, l);
                }
                ed = sd;
            }
            var endDate = ed;
            var rtn2 = new Dictionary<DateTime, List<SimplisityInfo>>();
            for (int i = 0; i < numberOfMonths; i++)
            {
                var d = endDate.AddMonths(i * -1);
                var sd = new DateTime(d.Year, d.Month, 1);
                if (rtn.ContainsKey(sd))
                    rtn2.Add(sd, rtn[sd]);
                else
                    rtn2.Add(sd, new List<SimplisityInfo>());
            }
            rtn2 = rtn2.OrderByDescending(obj => obj.Key).ToDictionary(obj => obj.Key, obj => obj.Value);
            return rtn2;
        }
        public List<ArticleLimpet> GetArticleRssList(DateTime startMonthDate, int numberOfMonths, string sqlindexDateRef = "", int catid = 0, int limit = 1000)
        {
            var rtn = new List<ArticleLimpet>();
            var articleList = GetArticlesByDateDesc(startMonthDate, numberOfMonths, sqlindexDateRef,catid, limit);
            foreach (var a in articleList)
            {
                rtn.Add(RocketDirectoryAPIUtils.GetArticleData(a.PortalId, a.ItemID, a.Lang, _systemKey));
            }
            return rtn;
        }
        public List<SimplisityInfo> GetArticlesByDateDesc(DateTime startMonthDate, int numberOfMonths, string sqlindexDateRef = "", int catid = 0, int limit = 1000)
        {
            var startDate = new DateTime(startMonthDate.Year, startMonthDate.Month, 1);
            var endDate = new DateTime(startMonthDate.AddMonths(numberOfMonths).Year, startMonthDate.AddMonths(numberOfMonths).Month, DateTime.DaysInMonth(startMonthDate.AddMonths(numberOfMonths).Year, startMonthDate.AddMonths(numberOfMonths).Month));
            var searchFilter = " and [XMLData].value('(genxml/checkbox/hidden)[1]','bit') = 0 ";
            var orderby = "order by modifieddate";
            var systemData = new SystemLimpet(_systemKey);
            var sqlIndexRec = systemData.GetSqlIndex(sqlindexDateRef);
            if (sqlindexDateRef != "" && sqlIndexRec != null)
            {
                var xpath = sqlIndexRec.GetXmlProperty("genxml/xpath");
                searchFilter += " and [XMLdata].value('(" + xpath + ")[1]','date') >= convert(date,'" + startDate.Date.ToString("O") + "') and [XMLdata].value('(" + xpath + ")[1]','date') <= convert(date,'" + endDate.Date.ToString("O") + "') ";
                orderby = "order by " + sqlindexDateRef + ".GUIDKey";
            }
            if (catid > 0) searchFilter += " and [CATXREF].[XrefItemId] = " + catid + " ";
            return _objCtrl.GetList(PortalCatalog.PortalId, -1, _entityTypeCode, searchFilter, _langRequired, orderby, limit, 0, 0, 0, _tableName);
        }
        public List<SimplisityInfo> GetArticlesByDateInfo(DateTime startMonthDate, DateTime endMonthDate, string sqlindexDateRef, int catid = 0, int limit = 1000)
        {
            var startDate = startMonthDate;
            var endDate = endMonthDate;
            var searchFilter = " and [XMLData].value('(genxml/checkbox/hidden)[1]','bit') = 0 ";
            var orderby = "order by modifieddate";
            var systemData = new SystemLimpet(_systemKey);
            var sqlIndexRec = systemData.GetSqlIndex(sqlindexDateRef);
            if (sqlindexDateRef != "" && sqlIndexRec != null)
            {
                var xpath = sqlIndexRec.GetXmlProperty("genxml/xpath");
                searchFilter += " and [XMLdata].value('(" + xpath + ")[1]','date') >= convert(date,'" + startDate.Date.ToString("O") + "') and [XMLdata].value('(" + xpath + ")[1]','date') <= convert(date,'" + endDate.Date.ToString("O") + "') ";
                orderby = "order by " + sqlindexDateRef + ".GUIDKey";
            }
            if (catid > 0) searchFilter += " and [CATXREF].[XrefItemId] = " + catid + " ";
            return _objCtrl.GetList(PortalCatalog.PortalId, -1, _entityTypeCode, searchFilter, _langRequired, orderby, limit, 0, 0, 0, _tableName);
        }
        public List<ArticleLimpet> GetArticlesByDate(DateTime startMonthDate, DateTime endMonthDate, string sqlindexDateRef, int catid = 0, int limit = 1000)
        {
            var rtn = new List<ArticleLimpet>();
            var articleList = GetArticlesByDateInfo(startMonthDate, endMonthDate, sqlindexDateRef, catid, limit);
            foreach (var a in articleList)
            {
                rtn.Add(RocketDirectoryAPIUtils.GetArticleData(a.PortalId, a.ItemID, a.Lang, _systemKey));
            }
            return rtn;
        }
        public List<ArticleLimpet> GetArticlesInDay(DateTime monthDate, string sqlindexDateRef, int catid = 0, int limit = 1000)
        {
            var rtn = new List<ArticleLimpet>();
            var articleList = GetArticlesByDateInfo(monthDate, monthDate, sqlindexDateRef, catid, limit);
            foreach (var a in articleList)
            {
                rtn.Add(RocketDirectoryAPIUtils.GetArticleData(a.PortalId, a.ItemID, a.Lang, _systemKey));
            }
            return rtn;
        }
        public List<SimplisityInfo> GetArticlesInMonthInfo(DateTime monthDate,string sqlindexDateRef, int catid = 0, int limit = 1000)
        {
            var startDate = new DateTime(monthDate.Year, monthDate.Month, 1);
            var endDate = new DateTime(monthDate.Year, monthDate.Month, 1).AddMonths(1).AddDays(-1); ;
            return GetArticlesByDateInfo(startDate, endDate, sqlindexDateRef, catid, limit);
        }
        public List<ArticleLimpet> GetArticlesInMonth(DateTime monthDate, string sqlindexDateRef, int catid = 0, int limit = 1000)
        {
            var rtn = new List<ArticleLimpet>();
            var articleList = GetArticlesInMonthInfo(monthDate, sqlindexDateRef, catid, limit);
            foreach (var a in articleList)
            {
                rtn.Add(RocketDirectoryAPIUtils.GetArticleData(a.PortalId, a.ItemID, a.Lang, _systemKey));
            }
            return rtn;            
        }
        /// <summary>
        /// Gets a listof articles that have changed, usually used for search index.  (ModuleId is used as a changed flag)
        /// </summary>
        /// <param name="moduleId">Search ModuleID</param>
        /// <param name="limit">limit of return records.</param>
        /// <returns></returns>
        public List<int> GetArticleChangedList(int moduleId, int limit = 1000)
        {
            var rtn = new List<int>();
            var searchFilter = " and R1.ModuleId = " + moduleId + " ";
            var articleList = _objCtrl.GetList(PortalCatalog.PortalId, moduleId, _entityTypeCode, searchFilter, _langRequired, "", limit, 0, 0, 0, _tableName);
            foreach (var a in articleList)
            {
                rtn.Add(a.ItemID);
            }
            return rtn;
        }
        public void DeleteAll()
        {
            var l = GetAllPortalArticles();
            foreach (var r in l)
            {
                _objCtrl.Delete(r.ItemID);
            }
        }
        public List<ArticleLimpet> GetArticlesListWithoutPaging(int limit = 1000)
        {
            var rtn = new List<ArticleLimpet>();
            var articleList = _objCtrl.GetList(PortalCatalog.PortalId, -1, _entityTypeCode, _searchFilter, _langRequired, _orderby, limit, 0, 0, 0, _tableName);
            foreach (var a in articleList)
            {
                rtn.Add(new ArticleLimpet(a));
            }
            return rtn;
        }

        private string GetPropertyFilterSQL()
        {
            //Filter Property
            var checkboxfilter = "";
            ModuleContentLimpet moduleSettings = null;

            var propidtag = SessionParamData.GetInt("rocketpropertyidtag");
            if (propidtag > 0)
            {
                // Tags on property (only 1 tag)
                checkboxfilter += " R1.ItemId IN (SELECT ParentItemId FROM {databaseOwner}[{objectQualifier}" + _tableName + "] as [PROPXREF] where [PROPXREF].TypeCode =  'PROPXREF' and [PROPXREF].XrefItemId = " + propidtag + ") ";
            }
            else
            {
                // Filter on property 
                var nodList = SessionParamData.Info.XMLDoc.SelectNodes("r/*[starts-with(name(), 'checkboxfilter')]");
                if (nodList != null && nodList.Count > 0) moduleSettings = new ModuleContentLimpet(PortalCatalog.PortalId, SessionParamData.ModuleRef, _systemKey, SessionParamData.ModuleId, SessionParamData.TabId);

                if (nodList != null)
                {
                    foreach (XmlNode nod in nodList)
                    {
                        if (nod.InnerText.ToLower() == "true")
                        {
                            var splitId = nod.Name.Split('-');
                            if (splitId.Count() == 2)
                            {
                                var propid = splitId[0].Replace("checkboxfilter", "");
                                // NOTE: checkbox for filter must be called "checkboxfilterand"
                                if (moduleSettings != null && PortalCatalog.FilterByInAll)
                                {
                                    if (checkboxfilter != "") checkboxfilter += " and ";
                                    checkboxfilter += " R1.ItemId IN (SELECT ParentItemId FROM {databaseOwner}[{objectQualifier}" + _tableName + "] as [PROPXREF] where [PROPXREF].TypeCode =  'PROPXREF' and [PROPXREF].XrefItemId = " + propid + ") ";
                                }
                                else
                                {
                                    if (checkboxfilter != "") checkboxfilter += " or ";
                                    checkboxfilter += " R1.ItemId IN (SELECT ParentItemId FROM {databaseOwner}[{objectQualifier}" + _tableName + "] as [PROPXREF] where [PROPXREF].TypeCode =  'PROPXREF' and [PROPXREF].XrefItemId = " + propid + ") ";
                                }
                            }
                        }
                    }
                }
            }
            if (checkboxfilter != "")
            {
                checkboxfilter = _objCtrl.ReplaceObjectQualifiers(checkboxfilter);
                return " and ( " + checkboxfilter + " ) ";
            }
            return "";
        }

        public bool ClearFilter { get; private set; }
        public bool ClearCategory { get; private set; }
        public SessionParams SessionParamData { get; set; }
        public List<SimplisityInfo> DataList { get; private set; }
        public PortalCatalogLimpet PortalCatalog { get; set; }
        public CatalogSettingsLimpet CatalogSettings { get; private set; }
        
        public int RecordCount { get; set; }
        public int CategoryId { get { return _searchcategoryid; } }
        public List<ArticleLimpet> GetArticleList()
        {
            _articleList = new List<ArticleLimpet>();
            foreach (var o in DataList)
            {
                var articleData = RocketDirectoryAPIUtils.GetArticleData(o.PortalId, o.ItemID, _langRequired, _systemKey);
                if (articleData.Exists) _articleList.Add(articleData);
            }
            return _articleList;
        }
        public List<ArticleLimpet> GetArticleList(ModuleContentLimpet moduleData, int maxReturn = 20)
        {
            var returnLimit = moduleData.GetSettingInt("itemlimit");
            if (returnLimit > maxReturn) returnLimit = maxReturn; // limit search for performace.
            var articleList  = new List<ArticleLimpet>();

            var searchFilter = "";
            if (moduleData.DefaultCategoryId > 0) searchFilter += " and [CATXREF].[XrefItemId] = " + moduleData.DefaultCategoryId + " ";
            searchFilter += " and NOT(isnull([XMLData].value('(genxml/checkbox/hidden)[1]','nvarchar(4)'),'false') = 'true') and NOT(isnull([XMLData].value('(genxml/lang/genxml/checkbox/hidden)[1]','nvarchar(4)'),'false') = 'true') ";

            var orderby = "";
            if (moduleData.DefaultCategoryId > 0 && CatalogSettings.ManualCategoryOrderby)
                orderby = " order by [CATXREF].[SortOrder] "; // use manual sort for articles by category;
            else
                orderby = PortalCatalog.OrderByProductSQL(moduleData.GetSetting("sortorderkey"));

            var sList = _objCtrl.GetList(PortalCatalog.PortalId, -1, _entityTypeCode, searchFilter, _langRequired, orderby, returnLimit, 0, 0, 0, _tableName);

            foreach (var o in sList)
            {
                var articleData = RocketDirectoryAPIUtils.GetArticleData(o.PortalId, o.ItemID, _langRequired, _systemKey);
                if (articleData.Exists) articleList.Add(articleData);
            }
            return articleList;
        }
        public List<List<ArticleLimpet>> GetArticleRows(int columns)
        {
            var rtnList = new List<List<ArticleLimpet>>();
            var rowList = new List<ArticleLimpet>();
            var lp = 0;
            foreach (var o in DataList)
            {
                var articleData = RocketDirectoryAPIUtils.GetArticleData(o.PortalId, o.ItemID, _langRequired, _systemKey);
                if (articleData.Exists) rowList.Add(articleData);

                if ((lp % columns) == (columns - 1))
                {
                    rtnList.Add(rowList);
                    rowList = new List<ArticleLimpet>();
                }
                lp += 1;
            }
            if (rowList.Count > 0) rtnList.Add(rowList);

            return rtnList;
        }
        public void SortOrderMove(int toItemId)
        {
            SortOrderMove(SessionParamData.SortActivate, toItemId);
        }
        public void SortOrderMove(int fromItemId, int toItemId)
        {
            if (fromItemId > 0 && toItemId > 0)
            {
                var moveData = RocketDirectoryAPIUtils.GetArticleData(PortalCatalog.PortalId, fromItemId, _langRequired, _systemKey);
                var toData = RocketDirectoryAPIUtils.GetArticleData(PortalCatalog.PortalId, toItemId, _langRequired, _systemKey);
                if (moveData.Exists && toData.Exists)
                {
                    var newSortOrder = toData.SortOrder - 1;
                    if (moveData.SortOrder < toData.SortOrder) newSortOrder = toData.SortOrder + 1;

                    moveData.SortOrder = newSortOrder;
                    moveData.Update();
                }
                SessionParamData.CancelItemSort();
            }
        }

        public List<SimplisityInfo> GetAllPortalArticles()
        {
            return _objCtrl.GetList(PortalCatalog.PortalId, -1, _entityTypeCode, "", _langRequired, "", 0, 0, 0, 0, _tableName);
        }
        public void ReIndex()
        {
            var list = GetAllPortalArticles();
            foreach (var pInfo in list)
            {
                var articleData = RocketDirectoryAPIUtils.GetArticleData(PortalCatalog.PortalId, pInfo.ItemID, _langRequired, _systemKey);
                articleData.ModuleId = PortalCatalog.SearchModuleId; // moduleid used as changed flag.
                articleData.Update();
                _objCtrl.RebuildIndex(PortalCatalog.PortalId, pInfo.ItemID, _systemKey, _tableName);
            }
            DNNrocketUtils.SynchronizeModule(PortalCatalog.SearchModuleId); // module search
        }
        public void Validate()
        {
            var list = GetAllPortalArticles();
            foreach (var pInfo in list)
            {
                var articleData = RocketDirectoryAPIUtils.GetArticleData(PortalCatalog.PortalId, pInfo.ItemID, _langRequired, _systemKey);
                articleData.ValidateAndUpdate();
            }
        }

        public string ListUrl()
        {
            return SessionParamData.PageUrl;
        }
    }

}
