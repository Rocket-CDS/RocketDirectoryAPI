using DNNrocketAPI.Components;
using Rocket.AppThemes.Components;
using RocketDirectoryAPI.Components;
using RocketPortal.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.IO;

namespace RocketDirectoryAPI.API
{
    public partial class StartConnect : DNNrocketAPI.APInterface
    {
        private SimplisityInfo _postInfo;
        private SimplisityInfo _paramInfo;
        private RocketInterface _rocketInterface;
        private SystemLimpet _systemData;
        private PortalCatalogLimpet _portalCatalog;
        private Dictionary<string, string> _passSettings;
        private SessionParams _sessionParams;
        private UserParams _userParams;
        private AppThemeSystemLimpet _appThemeSystem;
        private string _moduleRef;
        private AppThemeLimpet _appThemeAdmin;
        private AppThemeLimpet _appThemeView;
        private AppThemeLimpet _appThemeDefault;
        private AppThemeSystemLimpet _appThemeCatalog;        
        private RemoteModule _remoteModule;
        private Dictionary<string, object> _dataObjects;
        private PortalLimpet _portalData;
        private string _org;
        private AppThemeProjectLimpet _orgData;
        private CatalogSettingsLimpet _catalogSettings;
        private const string _baseSystemKey = "rocketdirectoryapi";
        private int _defaultCategoryId;
        private string _storeParamCmd;
        public override Dictionary<string, object> ProcessCommand(string paramCmd, SimplisityInfo systemInfo, SimplisityInfo interfaceInfo, SimplisityInfo postInfo, SimplisityInfo paramInfo, string langRequired = "")
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
                case "rocketsystem_getappthemeversions":
                    strOut = AppThemeAdminVersions();
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
                    _portalCatalog.Record.AddListItem("settingsdata");
                    _portalCatalog.Update();
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


                    
                case "remote_editoption":
                    strOut = "false";
                    break;
                case "remote_settings":
                    strOut = RemoteSettings();
                    break;
                case "remote_settingssave":
                    strOut = SaveSettings();
                    break;
                case "remote_clearsettings":
                    strOut = ClearSettings();
                    break;
                case "remote_getappthemeversions":
                    strOut = AppThemeVersions();
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

                case "invalidcommand":
                    strOut = "INVALID COMMAND: " + _storeParamCmd;
                    break;

            }

            if (paramCmd == "remote_publicview" || paramCmd == "remote_publicmenu")
            {
                rtnDic.Add("remote-seoheader", GetPublicArticleSEO());
                rtnDic.Add("remote-firstheader", GetPublicArticleBeforeHeader());
                rtnDic.Add("remote-lastheader", GetPublicArticleHeader());
                rtnDic.Add("remote-cache", "True");
            }
            _remoteModule.AppThemeFolder = _remoteModule.AppThemeViewFolder; // We do not have a edit AppTheme in the module. (Part of hte catalog edit)
            _remoteModule.AppThemeVersion = _remoteModule.AppThemeViewVersion;
            if (!rtnDic.ContainsKey("remote-settingsxml")) rtnDic.Add("remote-settingsxml", _remoteModule.Record.ToXmlItem());
            // if we have a searchtext we do not want to cache.
            if (_sessionParams.SearchText != "") rtnDic.Remove("remote-cache");

            rtnDic.Add("outputhtml", strOut);
            return rtnDic;
        }
        private String RocketSystem()
        {
            try
            {
                var razorTempl = GetSystemTemplate("RocketSystem.cshtml");
                var pr = RenderRazorUtils.RazorProcessData(razorTempl, _portalCatalog, _dataObjects, _passSettings, _sessionParams, true);
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
                    _portalCatalog = new PortalCatalogLimpet(newportalId, _sessionParams.CultureCodeEdit, _systemData.SystemKey);
                    _portalCatalog.Validate();
                    _portalCatalog.Update();
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
                    _portalCatalog.Delete();
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
            _systemData = new SystemLimpet(systemInfo.GetXmlProperty("genxml/systemkey"));
            _appThemeSystem = AppThemeUtils.AppThemeSystem(PortalUtils.GetPortalId(), _systemData.SystemKey);
            _appThemeCatalog = AppThemeUtils.AppThemeSystem(PortalUtils.GetPortalId(), "rocketdirectoryapi");            
            _rocketInterface = new RocketInterface(interfaceInfo);
            _sessionParams = new SessionParams(_paramInfo);
            _userParams = new UserParams(_sessionParams.BrowserSessionId);
            _passSettings = new Dictionary<string, string>();
            _orgData = new AppThemeProjectLimpet();
            _moduleRef = _paramInfo.GetXmlProperty("genxml/hidden/moduleref");
            if (_moduleRef == "") _moduleRef = _paramInfo.GetXmlProperty("genxml/remote/moduleref");

            // Assign Langauge
            DNNrocketUtils.SetCurrentCulture();
            if (_sessionParams.CultureCode == "") _sessionParams.CultureCode = DNNrocketUtils.GetCurrentCulture();
            if (_sessionParams.CultureCodeEdit == "") _sessionParams.CultureCodeEdit = DNNrocketUtils.GetEditCulture();
            DNNrocketUtils.SetCurrentCulture(_sessionParams.CultureCode);
            DNNrocketUtils.SetEditCulture(_sessionParams.CultureCodeEdit);

            var rtnCultureCode = _sessionParams.CultureCodeEdit;
            if (paramCmd.StartsWith("remote_")) rtnCultureCode = _sessionParams.CultureCode;

            var portalid = _paramInfo.GetXmlPropertyInt("genxml/hidden/portalid");
            if (portalid >= 0 && PortalUtils.GetPortalId() == 0)
            {
                _remoteModule = new RemoteModule(portalid, _moduleRef);
                _portalCatalog = new PortalCatalogLimpet(portalid, rtnCultureCode, _systemData.SystemKey); // Portal 0 is admin, editing portal setup
            }
            else
            {
                _remoteModule = new RemoteModule(PortalUtils.GetPortalId(), _moduleRef);
                _portalCatalog = new PortalCatalogLimpet(PortalUtils.GetPortalId(), rtnCultureCode, _systemData.SystemKey);
                if (!_portalCatalog.Active) return "";
            }

            _portalData = new PortalLimpet(_portalCatalog.PortalId);
            _appThemeDefault = AppThemeUtils.AppThemeDefault(_portalCatalog.PortalId, _systemData, "Default", "1.0");
            _catalogSettings = new CatalogSettingsLimpet(_portalCatalog.PortalId, rtnCultureCode, _systemData.SystemKey);

            // If the module is not on the display page, we need to create alink to the detail page.  This setting providers the new page url.
            var detailpageurl = _remoteModule.Record.GetXmlProperty("genxml/remote/detailpageurl" + _sessionParams.CultureCode);
            if (detailpageurl != "") _sessionParams.PageDetailUrl = detailpageurl;
            var listpageurl = _remoteModule.Record.GetXmlProperty("genxml/remote/listpageurl" + _sessionParams.CultureCode);
            if (listpageurl != "") _sessionParams.PageListUrl = listpageurl;

            //deaful pagesize
            if (_sessionParams.PageSize == 0) _paramInfo.SetXmlPropertyInt("genxml/hidden/pagesize", _remoteModule.Record.GetXmlPropertyInt("genxml/remote/pagesize"));

            // set default category
            _defaultCategoryId = _remoteModule.Record.GetXmlPropertyInt("genxml/remote/categoryid");
            if (_defaultCategoryId <= 0) _defaultCategoryId = _catalogSettings.DefaultCategoryId;

            _org = _remoteModule.ProjectName;
            if (_org == "") _org = _orgData.DefaultProjectName();
            if (_postInfo.GetXmlProperty("genxml/select/selectedprojectname") != "") _portalCatalog.ProjectName = _postInfo.GetXmlProperty("genxml/select/selectedprojectname");
            if (_portalCatalog.ProjectName == "") _portalCatalog.ProjectName = _orgData.DefaultProjectName();

            _appThemeView = AppThemeUtils.AppTheme(_portalCatalog.PortalId, _remoteModule.AppThemeViewFolder, _remoteModule.AppThemeViewVersion, _org);
            _appThemeAdmin = AppThemeUtils.AppTheme(_portalCatalog.PortalId, _portalCatalog.AppThemeAdminFolder, _portalCatalog.AppThemeAdminVersion, _portalCatalog.ProjectName);

            var securityData = new SecurityLimpet(_portalCatalog.PortalId, _baseSystemKey, _rocketInterface, -1, -1, _systemData.SystemKey);

            _dataObjects = new Dictionary<string, object>();
            _dataObjects.Add("remotemodule", _remoteModule);
            _dataObjects.Add("apptheme", _appThemeView);
            _dataObjects.Add("appthemeadmin", _appThemeAdmin);
            _dataObjects.Add("appthemedefault", _appThemeDefault);
            _dataObjects.Add("appthemecatalog", _appThemeCatalog);
            _dataObjects.Add("appthemesystem", _appThemeSystem);
            _dataObjects.Add("portalcatalog", _portalCatalog);
            _dataObjects.Add("portaldata", _portalData);
            _dataObjects.Add("catalogsettings", _catalogSettings);
            _dataObjects.Add("securitydata", securityData);
            _dataObjects.Add("systemdata", _systemData);
            var categoryDataList = new CategoryLimpetList(_portalCatalog.PortalId, rtnCultureCode, _systemData.SystemKey, true, _remoteModule);
            _dataObjects.Add("categorylist", categoryDataList);
            var propertyList = new PropertyLimpetList(_portalCatalog.PortalId, _sessionParams, rtnCultureCode, _systemData.SystemKey, _remoteModule);
            _dataObjects.Add("propertylist", propertyList);
            var portalDashboard = new DashboardLimpet(_portalCatalog.PortalId, rtnCultureCode);
            _dataObjects.Add("dashboard", portalDashboard);

            // if we have a remote_view, find the required cmd from module settings.
            if (paramCmd.StartsWith("remote_"))
            {
                var sk = _paramInfo.GetXmlProperty("genxml/remote/securitykey");
                if (!paramCmd.StartsWith("remote_public"))
                {
                    CacheUtils.ClearAllCache("hbs");  // clear hbs cache if not a public display - (Admin update)
                    if (_portalData.SecurityKey != sk) paramCmd = "";
                }
                else
                {
                    var convertCmd = _paramInfo.GetXmlProperty("genxml/remote/urlparams/cmd"); // use the remote URL param "cmd" if it exists.
                    if (convertCmd == "") convertCmd = _remoteModule.Record.GetXmlProperty("genxml/select/cmd"); // use remote selected cmd.
                    if (paramCmd == "remote_publicview") paramCmd = convertCmd;
                    if (paramCmd == "remote_publicviewheader") paramCmd = convertCmd + "header";
                    if (paramCmd == "remote_publicseo") paramCmd = convertCmd + "seo";
                }
            }
            else
            {
                paramCmd = securityData.HasSecurityAccess(paramCmd, "rocketdirectoryapi_login");
            }
            return paramCmd;
        }
        /// <summary>
        /// We may have a wrapper system, so check both systems for the template 
        /// </summary>
        /// <param name="templateName"></param>
        /// <returns></returns>
        private string GetSystemTemplate(string templateName)
        {
            var razorTempl = _appThemeAdmin.GetTemplate(templateName);
            if (razorTempl == "") razorTempl = _appThemeSystem.GetTemplate(templateName);
            if (razorTempl == "") razorTempl = _appThemeCatalog.GetTemplate(templateName);
            return razorTempl;
        }
        private string GetDashboard()
        {
            try
            {
                var razorTempl = GetSystemTemplate("Dashboard.cshtml");
                var pr = RenderRazorUtils.RazorProcessData(razorTempl, _portalCatalog, _dataObjects, _passSettings, _sessionParams, true);
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
                var pr = RenderRazorUtils.RazorProcessData(razorTempl, _portalCatalog, _dataObjects, _passSettings, _sessionParams, true);
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
                var pr = RenderRazorUtils.RazorProcessData(razorTempl, _portalCatalog, _dataObjects, _passSettings, _sessionParams, true);
                if (pr.StatusCode != "00") return pr.ErrorMsg;
                return pr.RenderedText;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        #region "Remote Helpers"
        private string AssignRemoteHeaderTemplate(string templateName = "")
        {
            return AssignRemoteTemplate("header", templateName);
        }
        private string AssignRemoteTemplateName(string templateName = "")
        {
            var template = templateName;
            if (template == "") template = _paramInfo.GetXmlProperty("genxml/hidden/template");
            if (template == "") template = _paramInfo.GetXmlProperty("genxml/hidden/remotetemplate");
            if (template == "") template = _remoteModule.Record.GetXmlProperty("genxml/remote/template");
            return template;
        }
        private string AssignRemoteTemplate(string appendix = "", string templateName = "")
        {
            var template = AssignRemoteTemplateName(templateName);
            if (template == "") template = "view.cshtml";
            var templateRtn = _appThemeView.GetTemplate(Path.GetFileNameWithoutExtension(template) + appendix + Path.GetExtension(template), _moduleRef);
            if (templateRtn == "") templateRtn = _appThemeDefault.GetTemplate(Path.GetFileNameWithoutExtension(template) + appendix + Path.GetExtension(template));
            if (templateRtn == "") templateRtn = GetSystemTemplate(Path.GetFileNameWithoutExtension(template) + appendix + Path.GetExtension(template));
            return templateRtn;
        }
        private string ReloadPage()
        {
            try
            {
                // user does not have access, logoff
                UserUtils.SignOut();

                var razorTempl = GetSystemTemplate("Reload.cshtml");
                var rtn = RenderRazorUtils.RazorProcessData(razorTempl, _portalData, null, _passSettings, _sessionParams, true);
                return rtn.RenderedText;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        #endregion

    }
}
