using DNNrocketAPI;
using DNNrocketAPI.Components;
using Simplisity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

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

            _catid = sessionParams.GetInt("catid");
            _catidurl = _catid;
            if (_catid == 0) _catid = defaultCategoryId;
            _searchcategoryid = _catid;

            if (sessionParams.OrderByRef == "" && _catid == 0) sessionParams.OrderByRef = "sqlorderby-article-name";

            if (populate) Populate(showHidden);
        }
        public void Populate(bool showHidden = true)
        {
            _searchFilter = "";
            ClearFilter = false;
            ClearCategory = false;

            var searchText = PortalCatalog.GetFilterProductSQL(SessionParamData.Info);
            var propertyFilter = "";
            if (showHidden)
                _catid = 0;  //assume showHidden is admin.
            else
                propertyFilter = GetPropertyFilterSQL();


            _searchFilter += searchText;
            _searchFilter += propertyFilter;
            if (_searchcategoryid > 0) _searchFilter += " and [CATXREF].[XrefItemId] = " + _searchcategoryid + " ";

            // Filter hidden
            if (!showHidden)
            {
                _searchFilter += " and NOT(isnull([XMLData].value('(genxml/checkbox/hidden)[1]','nvarchar(4)'),'false') = 'true') and NOT(isnull([XMLData].value('(genxml/lang/genxml/checkbox/hidden)[1]','nvarchar(4)'),'false') = 'true') ";
            }

            var orderby = "";
            if (_searchcategoryid > 0 && CatalogSettings.ManualCategoryOrderby)
                orderby = " order by [CATXREF].[SortOrder] "; // use manual sort for articles by category;
            else
                orderby = PortalCatalog.OrderByProductSQL(SessionParamData.OrderByRef);

            if (showHidden)
            {
                // Assume admin if showhidden.
                orderby = PortalCatalog.OrderByProductSQL(PortalCatalog.Info.GetXmlProperty("genxml/hidden/adminorderbyref"));
            }

            if (orderby == "") orderby = " order by articlename.GUIDKey ";

            SessionParamData.RowCount = _objCtrl.GetListCount(PortalCatalog.PortalId, -1, _entityTypeCode, _searchFilter, _langRequired, _tableName);
            RecordCount = SessionParamData.RowCount;

            DataList = _objCtrl.GetList(PortalCatalog.PortalId, -1, _entityTypeCode, _searchFilter, _langRequired, orderby, 0, SessionParamData.Page, SessionParamData.PageSize, SessionParamData.RowCount, _tableName);
        }
        public void DeleteAll()
        {
            var l = GetAllPortalArticles();
            foreach (var r in l)
            {
                _objCtrl.Delete(r.ItemID);
            }
        }

        private string GetPropertyFilterSQL()
        {
            //Filter Property
            var checkboxfilter = "";
            ModuleContentLimpet moduleSettings = null;
            var nodList = SessionParamData.Info.XMLDoc.SelectNodes("r/*[starts-with(name(), 'checkboxfilter')]");
            if (nodList != null && nodList.Count > 0) moduleSettings = new ModuleContentLimpet(PortalCatalog.PortalId, SessionParamData.ModuleRef, _systemKey, SessionParamData.ModuleId, SessionParamData.TabId);

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
                var articleData = new ArticleLimpet(o.ItemID, _langRequired, _systemKey);
                if (articleData.Exists) _articleList.Add(articleData);
            }
            return _articleList;
        }
        public void SortOrderMove(int toItemId)
        {
            SortOrderMove(SessionParamData.SortActivate, toItemId);
        }
        public void SortOrderMove(int fromItemId, int toItemId)
        {
            if (fromItemId > 0 && toItemId > 0)
            {
                var moveData = new ArticleLimpet(fromItemId, _langRequired, _systemKey);
                var toData = new ArticleLimpet(toItemId, _langRequired, _systemKey);
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
                _objCtrl.RebuildIndex(PortalCatalog.PortalId, pInfo.ItemID, _systemKey, _tableName);
            }
        }
        public void Validate()
        {
            var list = GetAllPortalArticles();
            foreach (var pInfo in list)
            {
                var articleData = new ArticleLimpet(PortalCatalog.PortalId, pInfo.ItemID, _langRequired, _systemKey);
                articleData.ValidateAndUpdate();
            }
        }

        public string ListUrl()
        {
            return SessionParamData.PageUrl;
        }
    }

}
