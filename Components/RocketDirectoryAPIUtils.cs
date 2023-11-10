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

namespace RocketDirectoryAPI.Components
{
    public static class RocketDirectoryAPIUtils
    {
        public const string ControlPath = "/DesktopModules/DNNrocketModules/RocketDirectoryAPI";
        public const string ResourcePath = "/DesktopModules/DNNrocketModules/RocketDirectoryAPI/App_LocalResources";
        public static Dictionary<string, string> ModuleTemples(AppThemeLimpet appThemeView, string moduleRef)
        {
            var rtn = new Dictionary<string,string>();
            if (appThemeView != null)
            {
                foreach (var tfile in appThemeView.GetModuleTemples())
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
                foreach (var tfile in appThemeView.GetModuleTemples())
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
            var rtn = (string)CacheUtils.GetCache(cacheKey, "portal" + portalId);
            if (rtn != null && !moduleSettings.DisableCache) return rtn;

            var dataObject = new DataObjectLimpet(portalId, moduleRef, sessionParam, systemKey, false);
            if (articleId > 0)
            {
                var articleData = new ArticleLimpet(dataObject.PortalContent.PortalId, articleId, sessionParam.CultureCode, systemKey);
                dataObject.SetDataObject("articledata", articleData);
            }
            var razorTempl = dataObject.AppTheme.GetTemplate(template, moduleRef);

            var pr = RenderRazorUtils.RazorProcessData(razorTempl, dataObject.DataObjects, null, sessionParam, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            CacheUtils.SetCache(cacheKey, pr.RenderedText, "portal" + portalId);
            return pr.RenderedText;
        }
        public static string DisplayView(DataObjectLimpet dataObject, string template = "")
        {
            var sessionParam = dataObject.SessionParamsData;
            if (sessionParam.PageSize == 0) sessionParam.PageSize = dataObject.ModuleSettings.GetSettingInt("pagesize");

            var cacheKey = dataObject.ModuleSettings.ModuleRef + "*" + sessionParam.UrlFriendly + "-" + sessionParam.OrderByRef + "-" + sessionParam.Page + "-" + sessionParam.PageSize;
            if (sessionParam.SearchText == "" && !sessionParam.GetBool("disablecache"))
            {
                var rtn = (string)CacheUtils.GetCache(cacheKey, "portal" + dataObject.PortalId);
                if (rtn != null && !dataObject.ModuleSettings.DisableCache) return rtn;
            }
            var aticleId = sessionParam.GetInt("articleid");
            if (template == "") template = dataObject.ModuleSettings.GetSetting("displaytemplate");
            if (template == "") template = "view.cshtml";
            var paramCmd = "list";
            var modt = RocketDirectoryAPIUtils.GetSelectedModuleTemple(dataObject.AppTheme, dataObject.ModuleSettings.ModuleRef, template);
            if (modt != null && modt.GetXmlProperty("genxml/cmd") != "") paramCmd = modt.GetXmlProperty("genxml/cmd");

            if (paramCmd == "list")
            {
                if (aticleId > 0)
                {
                    var articleData = new ArticleLimpet(dataObject.PortalContent.PortalId, aticleId, sessionParam.CultureCode, dataObject.SystemKey);
                    dataObject.SetDataObject("articledata", articleData);
                    var categoryData = new CategoryLimpet(dataObject.PortalContent.PortalId, articleData.DefaultCategory(), sessionParam.CultureCode, dataObject.SystemKey);
                    dataObject.SetDataObject("categorydata", categoryData);
                }
                else
                {
                    var defaultCat = sessionParam.GetInt("catid");
                    if (defaultCat == 0) defaultCat = dataObject.ModuleSettings.DefaultCategoryId;
                    if (defaultCat == 0) defaultCat = dataObject.CatalogSettings.DefaultCategoryId;
                    var articleDataList = new ArticleLimpetList(sessionParam, dataObject.PortalContent, sessionParam.CultureCode, true, false, defaultCat);
                    dataObject.SetDataObject("articlelist", articleDataList);
                    var categoryData = new CategoryLimpet(dataObject.PortalContent.PortalId, articleDataList.CategoryId, sessionParam.CultureCode, dataObject.SystemKey);
                    dataObject.SetDataObject("categorydata", categoryData);
                }
            }
            if (paramCmd == "catmenu")
            {

            }

            var razorTempl = dataObject.AppTheme.GetTemplate(template, dataObject.ModuleSettings.ModuleRef);
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, dataObject.DataObjects, null, sessionParam, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            if (sessionParam.SearchText == "") CacheUtils.SetCache(cacheKey, pr.RenderedText, "portal" + dataObject.PortalId);
            return pr.RenderedText;

        }
        public static string DisplayView(int portalId, string systemKey, string moduleRef, SessionParams sessionParam)
        {
            var dataObject = new DataObjectLimpet(portalId, moduleRef, sessionParam, systemKey, false);
            return DisplayView(dataObject);
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


    }

}
