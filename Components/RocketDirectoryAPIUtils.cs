using DNNrocketAPI;
using DNNrocketAPI.Components;
using RocketPortal.Components;
using Simplisity;
using Simplisity.TemplateEngine;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace RocketDirectoryAPI.Components
{
    public static class RocketDirectoryAPIUtils
    {
        public const string ControlPath = "/DesktopModules/DNNrocketModules/RocketDirectoryAPI";
        public const string ResourcePath = "/DesktopModules/DNNrocketModules/RocketDirectoryAPI/App_LocalResources";

        /// <summary>
        /// Get ArticleData and implement cache, if the article exists.
        /// </summary>
        /// <param name="portalId">PortalId</param>
        /// <param name="articleId">-1 creates new record, no cache implemented.</param>
        /// <param name="cultureCode"></param>
        /// <param name="systemKey"></param>
        /// <param name="useCache">use Cache, default true</param>
        /// <returns></returns>
        public static ArticleLimpet GetArticleData(int portalId, int articleId, string cultureCode, string systemKey, bool useCache = true)
        {
            var cacheKey = "ArticleLimpet*" + portalId + "*" + articleId + "*" + cultureCode + "*" + systemKey;
            var groupId = systemKey + portalId;
            var articleData = (ArticleLimpet)CacheUtils.GetCache(cacheKey, groupId);
            if (articleData == null)
            {
                articleData = new ArticleLimpet(portalId, articleId, cultureCode, systemKey);
                if (articleId > 0) CacheUtils.SetCache(cacheKey, articleData, groupId);
                //LogUtils.LogSystem("RocketDirectoryAPIUtils.GetArticleData: " + cacheKey);
            }
            return articleData;
        }

        public static Dictionary<string, QueryParamsData> UrlQueryParams(AppThemeLimpet appThemeView)
        {
            var rtn = new Dictionary<string, QueryParamsData>();
            if (appThemeView != null)
            {
                foreach (var tfile in appThemeView.GetTemplatesDep())
                {
                    var t = appThemeView.GetModT(tfile.Key, "");
                    foreach (var r in t.GetRecordList("queryparams"))
                    {
                        var queryParamsData = new QueryParamsData();
                        queryParamsData.queryparam = r.GetXmlProperty("genxml/queryparam");
                        queryParamsData.tablename = r.GetXmlProperty("genxml/tablename");
                        queryParamsData.systemkey = r.GetXmlProperty("genxml/systemkey");
                        queryParamsData.datatype = r.GetXmlProperty("genxml/datatype");
                        queryParamsData.queryparamvalue = "";
                        if (!rtn.ContainsKey(queryParamsData.queryparam)) rtn.Add(queryParamsData.queryparam, queryParamsData);
                    }
                }
            }
            return rtn;
        }
        public static Dictionary<string, MenuProviderData> MenuProvider(AppThemeLimpet appThemeView)
        {
            var rtn = new Dictionary<string, MenuProviderData>();
            if (appThemeView != null)
            {
                foreach (var tfile in appThemeView.GetTemplatesDep())
                {
                    var t = appThemeView.GetModT(tfile.Key, "");
                    foreach (var r in t.GetRecordList("menuprovider"))
                    {
                        var menoproviderData = new MenuProviderData();
                        menoproviderData.assembly = r.GetXmlProperty("genxml/assembly");
                        menoproviderData.namespaceclass = r.GetXmlProperty("genxml/namespaceclass");
                        menoproviderData.systemkey = r.GetXmlProperty("genxml/systemkey");
                        if (!rtn.ContainsKey(menoproviderData.systemkey)) rtn.Add(menoproviderData.systemkey, menoproviderData);
                    }
                }
            }
            return rtn;
        }
        public static Dictionary<string, string> ModuleTemples(AppThemeLimpet appThemeView, string moduleRef)
        {
            var rtn = new Dictionary<string,string>();
            if (appThemeView != null)
            {
                foreach (var tfile in appThemeView.GetTemplatesDep())
                {
                    var t = appThemeView.GetModT(tfile.Key, moduleRef);
                    foreach (var r in t.GetRecordList("moduletemplates"))
                    {
                        if (!rtn.ContainsKey(r.GetXmlProperty("genxml/file"))) rtn.Add(r.GetXmlProperty("genxml/file"), r.GetXmlProperty("genxml/name"));
                    }
                }
            }
            return rtn;
        }
        public static SimplisityRecord GetSelectedModuleTemple(AppThemeLimpet appThemeView, string moduleRef, string templateFileName)
        {
            var rtn = new SimplisityRecord();
            if (appThemeView != null)
            {
                foreach (var tfile in appThemeView.GetTemplatesDep())
                {
                    var t = appThemeView.GetModT(tfile.Key, moduleRef);
                    foreach (var r in t.GetRecordList("moduletemplates"))
                    {
                        if (r.GetXmlProperty("genxml/file") == templateFileName) return r;
                    }
                }
            }
            return rtn;
        }
        public static List<SimplisityRecord> DependanciesList(int portalId, string systemKey, string moduleRef, SessionParams sessionParam)
        {
            var rtn = new List<SimplisityRecord>();
            var dataObject = new DataObjectLimpet(portalId, moduleRef, sessionParam, systemKey, false);
            if (dataObject.AppTheme != null)
            {
                foreach (var depfile in dataObject.AppTheme.GetTemplatesDep())
                {
                    var dep = dataObject.AppTheme.GetDep(depfile.Key, moduleRef);
                    foreach (var r in dep.GetRecordList("deps"))
                    {
                        var urlstr = r.GetXmlProperty("genxml/url");
                        if (urlstr.Contains("{"))
                        {
                            if (dataObject.PortalData != null) urlstr = urlstr.Replace("{domainurl}", dataObject.PortalData.EngineUrlWithProtocol);
                            if (dataObject.AppTheme != null) urlstr = urlstr.Replace("{appthemefolder}", dataObject.AppTheme.AppThemeVersionFolderRel);
                            if (dataObject.AppThemeSystem != null) urlstr = urlstr.Replace("{appthemesystemfolder}", dataObject.AppThemeSystem.AppThemeVersionFolderRel);
                        }
                        r.SetXmlProperty("genxml/id", CacheUtils.Md5HashCalc(urlstr));
                        r.SetXmlProperty("genxml/url", urlstr);
                        rtn.Add(r);
                    }
                }
            }
            return rtn;
        }
        public static List<RocketInterface> AdminInterfaceShow(PortalCatalogLimpet portalContent, List<RocketInterface> interfacelist, Dictionary<string, bool> interfacekeylist)
        {
            var rtn = new List<RocketInterface>();
            foreach (var r in interfacelist)
            {
                if (r.IsOnMenu && r.SecurityCheckUser(portalContent.PortalId, UserUtils.GetCurrentUserId()))
                {
                    if (!portalContent.IsPlugin(r.InterfaceKey) || (portalContent.IsPlugin(r.InterfaceKey) && portalContent.IsPluginActive(r.InterfaceKey)))
                    {
                        var show = true;
                        if (!portalContent.IsPlugin(r.InterfaceKey))
                        {
                            if (interfacekeylist.ContainsKey(r.InterfaceKey)) show = interfacekeylist[r.InterfaceKey];
                        }
                        if (UserUtils.IsSuperUser()) show = true;
                        if (show) rtn.Add(r);
                    }
                }
            }
            return rtn;
         }
        public static Dictionary<string, bool> AdminInterfaceKeyList(int portalId, string systemKey, string moduleRef, SessionParams sessionParam)
        {
            var rtn = new Dictionary<string, bool>();
            var dataObject = new DataObjectLimpet(portalId, moduleRef, sessionParam, systemKey, false);
            if (dataObject.AppTheme != null)
            {
                foreach (var depfile in dataObject.AppTheme.GetTemplatesDep())
                {
                    var dep = dataObject.AppTheme.GetDep(depfile.Key, moduleRef);
                    foreach (var r in dep.GetRecordList("adminpanelinterfacekeys"))
                    {
                        var interfacekey = r.GetXmlProperty("genxml/interfacekey");
                        var show = r.GetXmlPropertyBool("genxml/show");
                        rtn.Add(interfacekey,show);
                    }
                }
            }
            return rtn;
        }
        public static string AdminHeader(int portalId, string systemKey, string moduleRef, SessionParams sessionParam, string template)
        {
            return ViewHeader(portalId, systemKey, moduleRef, sessionParam, template);
        }
        public static string ViewHeader(int portalId, string systemKey, string moduleRef, SessionParams sessionParam, string template)
        {
            var moduleSettings = new ModuleContentLimpet(portalId, moduleRef, systemKey, sessionParam.ModuleId, sessionParam.TabId);
            if (moduleSettings.DisableHeader) return "";

            var articleId = sessionParam.GetInt("articleid");
            var cacheKey = moduleRef + "*" + articleId + "*" + template;
            var rtn = (string)CacheUtils.GetCache(cacheKey, systemKey + portalId);
            if (rtn != null && !moduleSettings.DisableCache) return rtn;

            var dataObject = new DataObjectLimpet(portalId, moduleRef, sessionParam, systemKey, false);
            if (articleId > 0)
            {
                var articleData = RocketDirectoryAPIUtils.GetArticleData(dataObject.PortalContent.PortalId, articleId, sessionParam.CultureCode, systemKey);
                dataObject.SetDataObject("articledata", articleData);
            }
            var razorTempl = dataObject.AppTheme.GetTemplate(template, moduleRef);

            var pr = RenderRazorUtils.RazorProcessData(razorTempl, dataObject.DataObjects, null, sessionParam, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            CacheUtils.SetCache(cacheKey, pr.RenderedText, systemKey + portalId);
            return pr.RenderedText;
        }
        public static string DisplayView(DataObjectLimpet dataObject, string template = "")
        {
            var sessionParam = dataObject.SessionParamsData;
            if (sessionParam.PageSize == 0) sessionParam.PageSize = dataObject.ModuleSettings.GetSettingInt("pagesize");

            DNNrocketUtils.SetCurrentCulture(sessionParam.CultureCode);

            // ------------------------------
            // CacheKey, with properties
            if (template == "") template = dataObject.ModuleSettings.GetSetting("displaytemplate");
            if (template == "") template = "view.cshtml";
            var cacheKey = template + "*" + dataObject.ModuleSettings.ModuleRef + "*" + sessionParam.UrlFriendly + "-" + sessionParam.OrderByRef + "-" + sessionParam.Page + "-" + sessionParam.PageSize;
            cacheKey += "-" + sessionParam.Get("rocketpropertyidtag");
            var nodList = sessionParam.Info.XMLDoc.SelectNodes("r/*[starts-with(name(), 'checkboxfilter')]");
            if (nodList != null)
            {
                foreach (XmlNode nod in nodList)
                {
                    cacheKey += "-" + nod.Name;
                    cacheKey += "-" + nod.InnerText;                        
                }
            }
            // ------------------------------

            if (!sessionParam.IsSearchMode() && !sessionParam.GetBool("disablecache"))
            {
                var rtn = (string)CacheUtils.GetCache(cacheKey, dataObject.SystemKey + dataObject.PortalId);
                if (!String.IsNullOrEmpty(rtn) && !dataObject.ModuleSettings.DisableCache) return rtn;
            }
            var aticleId = GetArticleId(dataObject.PortalId, dataObject.SystemKey, dataObject.SessionParamsData);
            var cmdType = dataObject.SessionParamsData.Get("cmdtype");
            if (cmdType == "") cmdType = "listdetail";
            var modt = RocketDirectoryAPIUtils.GetSelectedModuleTemple(dataObject.AppTheme, dataObject.ModuleSettings.ModuleRef, template);
            if (modt != null && modt.GetXmlProperty("genxml/cmd") != "") cmdType = modt.GetXmlProperty("genxml/cmd");

            if (cmdType == "list" || cmdType == "listdetail")
            {
                var articleData = RocketDirectoryAPIUtils.GetArticleData(dataObject.PortalContent.PortalId, aticleId, dataObject.SessionParamsData.CultureCode, dataObject.SystemKey);
                if (articleData.Exists)
                    dataObject = DetailData(articleData, dataObject);
                else
                    dataObject = ListData(dataObject);
            }
            if (cmdType == "listonly")
            {
                dataObject = ListData(dataObject);
            }
            if (cmdType == "detailonly")
            {
                var articleData = RocketDirectoryAPIUtils.GetArticleData(dataObject.PortalContent.PortalId, aticleId, dataObject.SessionParamsData.CultureCode, dataObject.SystemKey);
                dataObject = DetailData(articleData, dataObject);
            }
            if (cmdType == "satellite")
            {
                // load datalist, without populating it, for satelite modules.  use GetArticleList(ModuleContentLimpet moduleData, int maxReturn = 20) method.
                var articleDataList = new ArticleLimpetList(sessionParam, dataObject.PortalContent, sessionParam.CultureCode, false, false);
                dataObject.SetDataObject("articlelist", articleDataList);
            }
            if (cmdType == "catmenu")
            {
                var defaultCat = dataObject.SessionCatId();
                if (defaultCat == 0) defaultCat = dataObject.ModuleSettings.DefaultCategoryId;
                if (defaultCat == 0) defaultCat = dataObject.CatalogSettings.DefaultCategoryId;
                var categoryData = new CategoryLimpet(dataObject.PortalContent.PortalId, defaultCat, sessionParam.CultureCode, dataObject.SystemKey);
                dataObject.SetDataObject("categorydata", categoryData);
            }

            var razorTempl = dataObject.AppTheme.GetTemplate(template, dataObject.ModuleSettings.ModuleRef);
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, dataObject.DataObjects, null, sessionParam, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            if (sessionParam.SearchText == "")
            {
                CacheUtils.SetCache(cacheKey, pr.RenderedText, dataObject.SystemKey + dataObject.PortalId);
            }

            return pr.RenderedText;
        }
        private static int GetArticleId(int portalId, string systemKey, SessionParams sessionParams)
        {
            var rtn = 0;
            var paramidList = DNNrocketUtils.GetQueryKeys(portalId);
            foreach (var p in paramidList)
            {
                if (p.Value.systemkey == systemKey && p.Value.datatype.ToLower() == "article")
                {
                    var keyArray = p.Key.Split('_');
                    rtn = sessionParams.GetInt(keyArray[0]);
                    break;
                }
            }
            return rtn;
        }
        private static DataObjectLimpet ListData(DataObjectLimpet dataObject)
        {
            var sortorderkey = dataObject.ModuleSettings.GetSetting("sortorderkey");
            if (sortorderkey != "") dataObject.SessionParamsData.OrderByRef = sortorderkey; // use module setting if set.
            var defaultCat = dataObject.SessionCatId();
            if (defaultCat == 0) defaultCat = dataObject.ModuleSettings.DefaultCategoryId;
            if (defaultCat == 0) defaultCat = dataObject.CatalogSettings.DefaultCategoryId;
            var articleDataList = new ArticleLimpetList(dataObject.SessionParamsData, dataObject.PortalContent, dataObject.SessionParamsData.CultureCode, true, false, defaultCat);
            dataObject.SetDataObject("articlelist", articleDataList);
            var categoryData = new CategoryLimpet(dataObject.PortalContent.PortalId, articleDataList.CategoryId, dataObject.SessionParamsData.CultureCode, dataObject.SystemKey);
            dataObject.SetDataObject("categorydata", categoryData);
            return dataObject;
        }
        private static DataObjectLimpet DetailData(ArticleLimpet articleData, DataObjectLimpet dataObject)
        {
            if (articleData.Exists)
            {
                dataObject.SetDataObject("articledata", articleData);
                var articleCategoryData = new CategoryLimpet(dataObject.PortalContent.PortalId, articleData.DefaultCategory(), dataObject.SessionParamsData.CultureCode, dataObject.SystemKey);
                dataObject.SetDataObject("articlecategorydata", articleCategoryData);
            }
            var catid = dataObject.SessionCatId();
            var categoryData = new CategoryLimpet(dataObject.PortalContent.PortalId, catid, dataObject.SessionParamsData.CultureCode, dataObject.SystemKey);
            dataObject.SetDataObject("categorydata", categoryData);
            return dataObject;
        }
        public static string DisplayView(int portalId, string systemKey, string moduleRef, SessionParams sessionParam, string template = "", string noAppThemeReturn = "")
        {
            var dataObject = new DataObjectLimpet(portalId, moduleRef, sessionParam, systemKey, false);
            if (!dataObject.ModuleSettings.Exists || dataObject.AppTheme == null || dataObject.AppTheme.AppThemeFolder == "") return noAppThemeReturn;
            return DisplayView(dataObject, template);
        }
        public static string DisplaySystemView(int portalId, string systemKey, string moduleRef, SessionParams sessionParam, string template, bool editMode = true)
        {
            var dataObject = new DataObjectLimpet(portalId, moduleRef, sessionParam, systemKey, false);

            if (dataObject.AppThemeSystem == null) return "No System View";
            dataObject.ModuleSettings.AppThemeAdminFolder = dataObject.PortalContent.AppThemeFolder;
            dataObject.ModuleSettings.AppThemeAdminVersion = dataObject.PortalContent.AppThemeVersion;
            dataObject.ModuleSettings.ProjectName = dataObject.PortalContent.ProjectName;

            var razorTempl = dataObject.AppThemeSystem.GetTemplate(template, moduleRef);
            if (razorTempl == "") razorTempl = dataObject.AppThemeDirectory.GetTemplate(template, moduleRef);
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, dataObject.DataObjects, null, sessionParam, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public static string ResourceKey(string resourceKey, string resourceExt = "Text", string cultureCode = "")
        {
            return DNNrocketUtils.GetResourceString(ResourcePath, resourceKey, resourceExt, cultureCode);
        }        
        public static string TokenReplacementCultureCode(string str, string CultureCode)
        {
            if (CultureCode == "") return str;
            str = str.Replace("{culturecode}", CultureCode);
            var s = CultureCode.Split('-');
            if (s.Count() == 2)
            {
                str = str.Replace("{language}", s[0]);
                str = str.Replace("{country}", s[1]);
            }
            return str;
        }
        public static Dictionary<DateTime, List<SimplisityInfo>> GetArticlesByMonth(SessionParams sessionParams, string systemKey, DateTime startMonthDate, int numberOfMonths, string sqlindexDateRef = "", int catid = 0, bool useCache = true)
        {
            var cacheKey = systemKey + "*" + startMonthDate.ToString() + "*" + numberOfMonths + "*" + sqlindexDateRef + "*" + catid;
            var rtn = (Dictionary<DateTime, List<SimplisityInfo>>)CacheUtils.GetCache(cacheKey, "portalid" + PortalUtils.GetCurrentPortalId());
            if (rtn != null && useCache) return rtn;
            var adl = new ArticleLimpetList(sessionParams, new PortalCatalogLimpet(PortalUtils.GetCurrentPortalId(), DNNrocketUtils.GetCurrentCulture(), systemKey), DNNrocketUtils.GetCurrentCulture(), false);
            rtn = adl.GetArticlesByMonth(startMonthDate, numberOfMonths, sqlindexDateRef, catid);
            if (useCache) CacheUtils.SetCache(cacheKey, rtn, "portalid" + PortalUtils.GetCurrentPortalId());
            return rtn;
        }
        [Obsolete("Use GetArticlesByMonth(SessionParams sessionParams, string systemKey, DateTime startMonthDate, int numberOfMonths, string sqlindexDateRef = \"\", int catid = 0, bool useCache = true)")]
        public static Dictionary<DateTime, List<SimplisityInfo>> GetArticlesByMonth(string systemKey,DateTime startMonthDate, int numberOfMonths, string sqlindexDateRef = "", int catid = 0, bool useCache = true)
        {
            var cacheKey = systemKey + "*" + startMonthDate.ToString() + "*" + numberOfMonths + "*" + sqlindexDateRef + "*" + catid;
            var rtn = (Dictionary<DateTime, List<SimplisityInfo>>)CacheUtils.GetCache(cacheKey, "portalid" + PortalUtils.GetCurrentPortalId());
            if (rtn != null && useCache) return rtn;
            var adl = new ArticleLimpetList(new SessionParams(new SimplisityInfo()), new PortalCatalogLimpet(PortalUtils.GetCurrentPortalId(), DNNrocketUtils.GetCurrentCulture(), systemKey), DNNrocketUtils.GetCurrentCulture(), false);
            rtn = adl.GetArticlesByMonth(startMonthDate, numberOfMonths, sqlindexDateRef, catid);
            if (useCache) CacheUtils.SetCache(cacheKey, rtn, "portalid" + PortalUtils.GetCurrentPortalId());
            return rtn;
        }

        public static string UrlQueryKey(int portalId, string systemKey, string dataType)
        {
            var cacheKey = "UrlQueryKey*" + portalId + "*" + systemKey + "*" + dataType;
            var paramKey = (string)CacheUtils.GetCache(cacheKey);
            if (paramKey == null)
            {
                paramKey = "";
                var paramidList = DNNrocketUtils.GetQueryKeys(portalId);
                foreach (var paramDict in paramidList)
                {
                    if (systemKey == paramDict.Value.systemkey && paramDict.Value.datatype == dataType)
                    {
                        paramKey = paramDict.Value.queryparam;
                    }
                }
                CacheUtils.SetCache(cacheKey, paramKey, "portalid" + portalId);
            }
            return paramKey;
        }
        public static string UrlQueryCategoryKey(int portalId, string systemKey)
        {
            return UrlQueryKey(portalId, systemKey, "category");
        }
        public static string UrlQueryArticleKey(int portalId, string systemKey)
        {
            return UrlQueryKey(portalId, systemKey, "article");
        }

    }

}
