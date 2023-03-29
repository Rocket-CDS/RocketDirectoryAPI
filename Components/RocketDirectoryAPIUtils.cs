using DNNrocketAPI;
using DNNrocketAPI.Components;
using RocketPortal.Components;
using Simplisity;
using Simplisity.TemplateEngine;
using System;
using System.Collections.Generic;
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
                foreach (var depfile in appThemeView.GetTemplatesDep())
                {
                    var dep = appThemeView.GetDep(depfile.Key, moduleRef);
                    foreach (var r in dep.GetRecordList("moduletemplates"))
                    {
                        rtn.Add(r.GetXmlProperty("genxml/file"),r.GetXmlProperty("genxml/name"));
                    }
                }
            }
            return rtn;
        }
        public static List<SimplisityRecord> DependanciesList(int portalId, string systemKey, string moduleRef, SessionParams sessionParam)
        {
            var rtn = new List<SimplisityRecord>();
            var dataObject = new DataObjectLimpet(portalId, moduleRef, sessionParam, systemKey, false);
            if (dataObject.AppThemeView != null)
            {
                foreach (var depfile in dataObject.AppThemeView.GetTemplatesDep())
                {
                    var dep = dataObject.AppThemeView.GetDep(depfile.Key, moduleRef);
                    foreach (var r in dep.GetRecordList("deps"))
                    {
                        var urlstr = r.GetXmlProperty("genxml/url");
                        if (urlstr.Contains("{"))
                        {
                            if (dataObject.PortalData != null) urlstr = urlstr.Replace("{domainurl}", dataObject.PortalData.EngineUrlWithProtocol);
                            if (dataObject.AppThemeView != null) urlstr = urlstr.Replace("{appthemefolder}", dataObject.AppThemeView.AppThemeVersionFolderRel);
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
        public static string ViewHeader(int portalId, string systemKey, string moduleRef, SessionParams sessionParam, string template)
        {
            var moduleSettings = new ModuleContentLimpet(portalId, moduleRef, sessionParam.ModuleId, sessionParam.TabId);
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
            var razorTempl = dataObject.AppThemeView.GetTemplate(template, moduleRef);
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, dataObject.DataObjects, null, sessionParam, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            CacheUtils.SetCache(cacheKey, pr.RenderedText, "portal" + portalId);
            return pr.RenderedText;
        }
        public static string DisplayView(int portalId, string systemKey, string moduleRef, SessionParams sessionParam)
        {
            var moduleSettings = new ModuleContentLimpet(portalId, moduleRef, sessionParam.ModuleId, sessionParam.TabId);
            if (sessionParam.PageSize == 0) sessionParam.PageSize = moduleSettings.GetSettingInt("pagesize");

            var cacheKey = moduleRef + "*" + sessionParam.UrlFriendly + "-" + sessionParam.OrderByRef + "-" + sessionParam.Page + "-" + sessionParam.PageSize;
            if (sessionParam.SearchText == "")
            {
                var rtn = (string)CacheUtils.GetCache(cacheKey, "portal" + portalId);
                if (rtn != null && !moduleSettings.DisableCache) return rtn;
            }

            var dataObject = new DataObjectLimpet(portalId, moduleRef, sessionParam, systemKey, false);
            var aticleId = sessionParam.GetInt("articleid");
            var template = moduleSettings.GetSetting("displaytemplate");
            var paramCmd = moduleSettings.GetSetting("displaycmd");
            if (template == "") template = "view.cshtml";
            if (paramCmd == "") paramCmd = "list";

            if (paramCmd == "" || paramCmd == "list")
            {
                if (aticleId > 0)
                {
                    var articleData = new ArticleLimpet(dataObject.PortalContent.PortalId, aticleId, sessionParam.CultureCode, systemKey);
                    dataObject.SetDataObject("articledata", articleData);
                    var categoryData = new CategoryLimpet(dataObject.PortalContent.PortalId, articleData.DefaultCategory(), sessionParam.CultureCode, systemKey);
                    dataObject.SetDataObject("categorydata", categoryData);
                }
                else
                {
                    var articleDataList = new ArticleLimpetList(sessionParam, dataObject.PortalContent, sessionParam.CultureCode, true, false, dataObject.CatalogSettings.DefaultCategoryId);
                    dataObject.SetDataObject("articlelist", articleDataList);
                    var categoryData = new CategoryLimpet(dataObject.PortalContent.PortalId, articleDataList.CategoryId, sessionParam.CultureCode, dataObject.SystemKey);
                    dataObject.SetDataObject("categorydata", categoryData);
                }
            }
            if (paramCmd == "catmenu")
            {

            }

            var razorTempl = dataObject.AppThemeView.GetTemplate(template, moduleRef);
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, dataObject.DataObjects, null, sessionParam, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            if (sessionParam.SearchText == "") CacheUtils.SetCache(cacheKey, pr.RenderedText, "portal" + portalId);
            return pr.RenderedText;
        }
        public static string DisplaySystemView(int portalId, string systemKey, string moduleRef, SessionParams sessionParam, string template, bool editMode = true)
        {
            var dataObject = new DataObjectLimpet(portalId, moduleRef, sessionParam, systemKey, false);
            if (dataObject.AppThemeSystem == null) return "No System View";

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
