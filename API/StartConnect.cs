using DNNrocketAPI.Components;
using DNNrocketAPI.Interfaces;
using RazorEngine.Templating;
using Rocket.AppThemes.Components;
using RocketDirectoryAPI.Components;
using RocketPortal.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;

namespace RocketDirectoryAPI.API
{
    public partial class StartConnect : IProcessCommand
    {
        private SimplisityInfo _postInfo;
        private SimplisityInfo _paramInfo;
        private RocketInterface _rocketInterface;
        private SessionParams _sessionParams;
        private DataObjectLimpet _dataObject;
        private string _moduleRef;
        private int _moduleId;
        private int _tabId;
        private UserParams _userParams;
        private const string _baseSystemKey = "rocketdirectoryapi";
        private string _storeParamCmd;

        public Dictionary<string, object> ProcessCommand(string paramCmd, SimplisityInfo systemInfo, SimplisityInfo interfaceInfo, SimplisityInfo postInfo, SimplisityInfo paramInfo, string langRequired = "")
        {
            var strOut = ""; // return nothing if not matching commands.
            _storeParamCmd = paramCmd;

            paramCmd = InitCmd(paramCmd, systemInfo, interfaceInfo, postInfo, paramInfo, langRequired);

            var rtnDic = new Dictionary<string, object>();

            switch (paramCmd)
            {
                case "rocketsystem_edit":
                    strOut = RocketSystem();
                    break;
                case "rocketsystem_init":
                    strOut = RocketSystemInit();
                    break;
                case "rocketsystem_delete":
                    strOut = RocketSystemDelete();
                    break;


                case "rocketdirectoryapi_activate":
                    strOut = RocketSystemSave();
                    break;


                case "rocketdirectoryapi_adminpanel":
                    strOut = AdminPanel();
                    break;
                case "rocketsystem_adminpanelheader":
                    strOut = AdminPanelHeader();
                    break;
                case "dashboard_get":
                    strOut = GetDashboard();
                    break;
                case "dashboard_save":
                    strOut = SaveDashboard();
                    break;


                case "portalcatalog_save":
                    SavePortalCatalog();
                    strOut = RocketSystem();
                    break;
                case "portalcatalog_delete":
                    DeletePortalCatalog();
                    strOut = RocketSystem();
                    break;
                case "portalcatalog_addsetting":
                    _dataObject.PortalContent.Record.AddListItem("settingsdata");
                    _dataObject.PortalContent.Update();
                    strOut = RocketSystem();
                    break;
                case "portalcatalog_reset":
                    ResetPortalCatalog(_paramInfo.GetXmlPropertyInt("genxml/hidden/portalid"));
                    strOut = RocketSystem();
                    break;
                case "portalcatalog_validatecatalog":
                    strOut = ValidateCatalog();
                    break;


                case "articleadmin_editlist":
                    strOut = GetArticleList();
                    break;
                case "articleadmin_articlesearch":
                    strOut = GetArticleList();
                    break;
                case "articleadmin_editarticle":
                    strOut = GetArticle(_paramInfo.GetXmlPropertyInt("genxml/hidden/articleid"));
                    break;
                case "articleadmin_copy":
                    CopyArticle();
                    strOut = GetArticleList();
                    break;
                case "articleadmin_addarticle":
                    strOut = AddArticle();
                    break;
                case "articleadmin_savedata":
                    strOut = GetArticle(SaveArticle());
                    break;
                case "articleadmin_delete":
                    DeleteArticle();
                    strOut = GetArticleList();
                    break;
                case "remote_addlistitem":
                    strOut = AddArticleListItem();
                    break;
                case "articleadmin_addimage":
                    strOut = AddArticleImage();
                    break;
                case "articleadmin_adddoc":
                    strOut = AddArticleDoc();
                    break;
                case "articleadmin_addlink":
                    strOut = AddArticleLink();
                    break;
                case "articleadmin_docupload":
                    ArticleDocumentUploadToFolder();
                    strOut = GetArticle(_paramInfo.GetXmlPropertyInt("genxml/hidden/articleid"));
                    break;
                case "articleadmin_docdelete":
                    ArticleDeleteDocument();
                    strOut = GetArticle(_paramInfo.GetXmlPropertyInt("genxml/hidden/articleid"));
                    break;
                case "articleadmin_docselectlist":
                    strOut = ArticleDocumentList();
                    break;
                case "articleadmin_assigncategory":
                    strOut = AssignArticleCategory();
                    break;
                case "articleadmin_assigndefaultcategory":
                    strOut = AssignDefaultArticleCategory();
                    break;
                case "articleadmin_removecategory":
                    strOut = RemoveArticleCategory();
                    break;
                case "articleadmin_assignproperty":
                    strOut = AssignArticleProperty();
                    break;
                case "articleadmin_removeproperty":
                    strOut = RemoveArticleProperty();
                    break;



                case "categoryadmin_add":
                    strOut = AddCategory();
                    break;
                case "categoryadmin_editlist":
                    strOut = GetCategoryList();
                    break;
                case "categoryadmin_edit":
                    strOut = GetCategory(_paramInfo.GetXmlPropertyInt("genxml/hidden/categoryid"));
                    break;
                case "categoryadmin_delete":
                    strOut = DeleteCategory();
                    break;
                case "categoryadmin_save":
                    strOut = GetCategory(SaveCategory());
                    break;
                case "categoryadmin_addimage":
                    strOut = AddCategoryImage();
                    break;
                case "categoryadmin_move":
                    strOut = MoveCategory();
                    break;
                case "categoryadmin_assignparent":
                    strOut = AssignParentCategory();
                    break;
                case "categoryadmin_removearticle":
                    strOut = RemoveCategoryArticle();
                    break;
                case "categoryadmin_assigndefault":
                    strOut = AssignDefaultCategory();
                    break;


                case "propertyadmin_add":
                    strOut = AddProperty();
                    break;
                case "propertyadmin_editlist":
                    strOut = GetPropertyList();
                    break;
                case "propertyadmin_edit":
                    strOut = GetProperty(_paramInfo.GetXmlPropertyInt("genxml/hidden/propertyid"));
                    break;
                case "propertyadmin_delete":
                    strOut = DeleteProperty();
                    break;
                case "propertyadmin_save":
                    strOut = SaveProperty();
                    break;
                case "propertyadmin_removearticle":
                    strOut = RemovePropertyArticle();
                    break;
                    

                case "settingsadmin_edit":
                    strOut = GetCatalogSettings();
                    break;
                case "settingsadmin_delete":
                    strOut = DeleteCatalogSettings();
                    break;
                case "settingsadmin_save":
                    strOut = SaveCatalogSettings();
                    break;


                case "rocketdirectoryapi_login":
                    strOut = ReloadPage();
                    break;

                   

                case "remote_publiclist":
                    strOut = GetPublicArticleList();
                    break;
                case "remote_publiclistheader":
                    strOut = GetPublicArticleHeader();
                    break;
                case "remote_publicviewbeforeheader":
                    strOut = GetPublicArticleBeforeHeader();
                    break;
                case "remote_publiclistseo":
                    strOut = GetPublicArticleSEO();
                    break;

                case "remote_publicview":
                    strOut = GetPublicArticleList();
                    break;
                case "remote_publicbase":
                    strOut = GetPublicBase();
                    break;


                case "rocketdirectoryapi_settings":
                    strOut = DisplaySettings();
                    break;
                case "rocketdirectoryapi_savesettings":
                    strOut = SaveSettings();
                    break;


                case "invalidcommand":
                    strOut = "INVALID COMMAND: " + _storeParamCmd;
                    break;

            }

            if (paramCmd == "remote_publicview" || paramCmd == "remote_publicmenu")
            {
                rtnDic.Add("remote-seoheader", GetPublicArticleSEO());
                rtnDic.Add("remote-firstheader", GetPublicArticleBeforeHeader());
                rtnDic.Add("remote-lastheader", GetPublicArticleHeader());
            }

            rtnDic.Add("outputhtml", strOut);
            return rtnDic;
        }
        private String RocketSystem()
        {
            try
            {
                var razorTempl = GetSystemTemplate("RocketSystem.cshtml");
                var pr = RenderRazorUtils.RazorProcessData(razorTempl, _dataObject.PortalContent, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
                if (pr.StatusCode != "00") return pr.ErrorMsg;
                return pr.RenderedText;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        private String RocketSystemInit()
        {
            try
            {
                var newportalId = _paramInfo.GetXmlPropertyInt("genxml/hidden/newportalid");
                if (newportalId > 0)
                {
                    var portalCatalog = new PortalCatalogLimpet(newportalId, _sessionParams.CultureCodeEdit, _dataObject.SystemKey);
                    portalCatalog.Validate();
                    portalCatalog.Active = true;
                    portalCatalog.Update();
                    _dataObject.SetDataObject("portalcontent", portalCatalog);
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        private String RocketSystemDelete()
        {
            try
            {
                var portalId = _paramInfo.GetXmlPropertyInt("genxml/hidden/portalid");
                if (portalId > 0)
                {
                    _dataObject.PortalContent.Delete();
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        public string InitCmd(string paramCmd, SimplisityInfo systemInfo, SimplisityInfo interfaceInfo, SimplisityInfo postInfo, SimplisityInfo paramInfo, string langRequired = "")
        {
            _postInfo = postInfo;
            _paramInfo = paramInfo;

            var portalid = _paramInfo.GetXmlPropertyInt("genxml/hidden/portalid");
            if (portalid == 0) portalid = PortalUtils.GetCurrentPortalId();

            _rocketInterface = new RocketInterface(interfaceInfo);
            _sessionParams = new SessionParams(_paramInfo);
            _userParams = new UserParams(_sessionParams.BrowserSessionId);
            _moduleRef = _paramInfo.GetXmlProperty("genxml/hidden/moduleref");
            _tabId = _paramInfo.GetXmlPropertyInt("genxml/hidden/tabid");
            _moduleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/moduleid");

            var requesturl = _paramInfo.GetXmlProperty("genxml/hidden/requesturl");
            if (_paramInfo.GetXmlPropertyInt("genxml/hidden/articleid") == 0 && (requesturl != "" && requesturl.ToLower().Contains("articleid")))
            {
                _paramInfo.SetXmlProperty("genxml/hidden/articleid", DNNrocketUtils.ParseQueryString("articleid", requesturl));
            }

            // Assign Langauge
            if (_sessionParams.CultureCode == "") _sessionParams.CultureCode = DNNrocketUtils.GetCurrentCulture();
            if (_sessionParams.CultureCodeEdit == "") _sessionParams.CultureCodeEdit = DNNrocketUtils.GetEditCulture();
            DNNrocketUtils.SetCurrentCulture(_sessionParams.CultureCode);
            DNNrocketUtils.SetEditCulture(_sessionParams.CultureCodeEdit);

            var systemkey = systemInfo.GetXmlProperty("genxml/systemkey");
            _dataObject = new DataObjectLimpet(portalid, _sessionParams.ModuleRef, _sessionParams, systemkey);

            if (paramCmd == "rocketdirectoryapi_activate") SavePortalCatalog();
            if (paramCmd.StartsWith("rocketsystem_") && UserUtils.IsSuperUser()) return paramCmd;
            if (!_dataObject.PortalContent.Active) return "";
            if (paramCmd.StartsWith("remote_public")) return paramCmd;

            var securityData = new SecurityLimpet(portalid, _baseSystemKey, _rocketInterface, -1, -1, _dataObject.SystemKey);
            paramCmd = securityData.HasSecurityAccess(paramCmd, "rocketdirectoryapi_login");
            return paramCmd;
        }
        /// <summary>
        /// We may have a wrapper system, so check both systems for the template 
        /// </summary>
        /// <param name="templateName"></param>
        /// <returns></returns>
        private string GetSystemTemplate(string templateName)
        {
            var razorTempl = _dataObject.AppThemeAdmin.GetTemplate(templateName);
            if (razorTempl == "") razorTempl = _dataObject.AppThemeSystem.GetTemplate(templateName);
            if (razorTempl == "") razorTempl = _dataObject.AppThemeDirectory.GetTemplate(templateName);
            return razorTempl;
        }
        private string GetDashboard()
        {
            try
            {
                var razorTempl = GetSystemTemplate("Dashboard.cshtml");
                var pr = RenderRazorUtils.RazorProcessData(razorTempl, _dataObject.PortalContent, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
                if (pr.StatusCode != "00") return pr.ErrorMsg;
                return pr.RenderedText;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        private string AdminPanel()
        {
            try
            {
                var razorTempl = GetSystemTemplate("AdminPanel.cshtml");
                var pr = RenderRazorUtils.RazorProcessData(razorTempl, _dataObject.PortalContent, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
                if (pr.StatusCode != "00") return pr.ErrorMsg;
                return pr.RenderedText;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        private string AdminPanelHeader()
        {
            try
            {
                var razorTempl = GetSystemTemplate("AdminPanelHeader.cshtml");
                var pr = RenderRazorUtils.RazorProcessData(razorTempl, _dataObject.PortalContent, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
                if (pr.StatusCode != "00") return pr.ErrorMsg;
                return pr.RenderedText;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        private string ReloadPage()
        {
            try
            {
                // user does not have access, logoff
                UserUtils.SignOut();

                var razorTempl = GetSystemTemplate("Reload.cshtml");
                var rtn = RenderRazorUtils.RazorProcessData(razorTempl, _dataObject.PortalData, null, _dataObject.Settings, _sessionParams, true);
                return rtn.RenderedText;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        private string RenderSystemTemplate(string templateName)
        {
            var razorTempl = GetSystemTemplate(templateName);
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        private string RocketSystemSave()
        {
            var portalId = _paramInfo.GetXmlPropertyInt("genxml/hidden/portalid"); // we may have passed selection
            if (portalId >= 0)
            {
                _dataObject.PortalContent.Save(_postInfo);
                _dataObject.PortalData.Record.SetXmlProperty("genxml/systems/" + _dataObject.SystemKey + "setup", "True");
                _dataObject.PortalData.Record.SetXmlProperty("genxml/systems/" + _dataObject.SystemKey, "True");
                _dataObject.PortalData.Update();
                return RocketSystem();
            }
            return "Invalid PortalId";
        }

    }
}
