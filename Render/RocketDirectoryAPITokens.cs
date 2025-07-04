﻿using DNNrocketAPI.Components;
using Newtonsoft.Json.Linq;
using RazorEngine.Text;
using Rocket.AppThemes.Components;
using RocketPortal.Components;
using Simplisity;
using Simplisity.TemplateEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Razor.Parser.SyntaxTree;
using static System.Net.Mime.MediaTypeNames;

namespace RocketDirectoryAPI.Components
{
    public class RocketDirectoryAPITokens<T> : DNNrocketAPI.render.DNNrocketTokens<T>
    {
        // Define data classes, so we can use intellisense in inject templates
        public ArticleLimpet articleData;
        public ArticleLimpetList articleDataList;
        public AppThemeLimpet appTheme;
        public AppThemeLimpet appThemeDefault;
        public AppThemeSystemLimpet appThemeSystem;
        public AppThemeSystemLimpet appThemeDirectory;
        public AppThemeLimpet appThemeDirectoryDefault;
        public ModuleContentLimpet moduleData;
        public SimplisityInfo moduleDataInfo;
        [Obsolete("Use moduleData instead")]
        public ModuleContentLimpet moduleSettings;
        public PortalLimpet portalData;
        public PortalCatalogLimpet portalContent;
        public List<string> enabledlanguages = DNNrocketUtils.GetCultureCodeList();
        public SessionParams sessionParams;
        public UserParams userParams;
        public SimplisityInfo info;
        public SimplisityInfo infoempty;        
        public SystemLimpet systemData;
        public SystemLimpet systemDirectoryData;
        public CatalogSettingsLimpet catalogSettings;
        public CategoryLimpetList categoryDataList;
        public CategoryLimpet categoryData;
        public PropertyLimpetList propertyDataList;
        public PropertyLimpet propertyData;
        public AppThemeProjectLimpet appThemeProjects;
        public SystemGlobalData globalSettings;
        public AppThemeDataList appThemeDataList;
        public DashboardLimpet dashBoard;
        public AppThemeRocketApiLimpet appThemeRocketApi;

        public string AssignDataModel(SimplisityRazor sModel)
        {
            appTheme = (AppThemeLimpet)sModel.GetDataObject("apptheme");
            appThemeDefault = (AppThemeLimpet)sModel.GetDataObject("appthemedefault");
            appThemeSystem = (AppThemeSystemLimpet)sModel.GetDataObject("appthemesystem");
            appThemeDirectory = (AppThemeSystemLimpet)sModel.GetDataObject("appthemedirectory");
            appThemeDirectoryDefault = (AppThemeLimpet)sModel.GetDataObject("appthemedirectorydefault");
            appThemeProjects = (AppThemeProjectLimpet)sModel.GetDataObject("appthemeprojects");
            appThemeDataList = (AppThemeDataList)sModel.GetDataObject("appthemedatalist");
            portalContent = (PortalCatalogLimpet)sModel.GetDataObject("portalcontent");
            systemData = (SystemLimpet)sModel.GetDataObject("systemdata");
            systemDirectoryData = (SystemLimpet)sModel.GetDataObject("systemdirectorydata");
            portalData = (PortalLimpet)sModel.GetDataObject("portaldata");
            catalogSettings = (CatalogSettingsLimpet)sModel.GetDataObject("catalogsettings");
            moduleData = (ModuleContentLimpet)sModel.GetDataObject("modulesettings");
            moduleDataInfo = new SimplisityInfo(moduleData.Record);
            categoryDataList = (CategoryLimpetList)sModel.GetDataObject("categorylist");
            categoryData = (CategoryLimpet)sModel.GetDataObject("categorydata");
            propertyDataList = (PropertyLimpetList)sModel.GetDataObject("propertylist");
            propertyData = (PropertyLimpet)sModel.GetDataObject("propertydata");
            globalSettings = (SystemGlobalData)sModel.GetDataObject("globalsettings");
            dashBoard = (DashboardLimpet)sModel.GetDataObject("dashboard");
            articleData = (ArticleLimpet)sModel.GetDataObject("articledata");
            articleDataList = (ArticleLimpetList)sModel.GetDataObject("articlelist");
            sessionParams = sModel.SessionParamsData;
            userParams = (UserParams)sModel.GetDataObject("userparams");
            appThemeRocketApi = (AppThemeRocketApiLimpet)sModel.GetDataObject("appthemerocketapi");
            
            if (sessionParams == null) sessionParams = new SessionParams(new SimplisityInfo());
            info = new SimplisityInfo();
            if (articleData != null) info = articleData.Info;
            infoempty = new SimplisityInfo();

            AddProcessDataResx(appTheme, true);
            AddProcessData("resourcepath", systemData.SystemRelPath + "/App_LocalResources/");
            AddProcessData("resourcepath", systemDirectoryData.SystemRelPath + "/App_LocalResources/");

            // legacy
            moduleSettings = moduleData;


            // use return of "string", so we don;t get error with converting void to object.
            return "";
        }
        public IEncodedString TextBoxMoney(int portalId, string systemKey, string cultureCode, SimplisityInfo info, String xpath, String attributes = "", String defaultValue = "", bool localized = false, int row = 0, string listname = "", string type = "text")
        {
            if (info == null) info = new SimplisityInfo();
            var value = info.GetXmlPropertyInt(xpath);
            if (localized && !xpath.StartsWith("genxml/lang/"))
            {
                value = info.GetXmlPropertyInt("genxml/lang/" + xpath);
            }
            var upd = getUpdateAttr(xpath, attributes, localized);
            var id = getIdFromXpath(xpath, row, listname);

            // [TODO: add encrypt option.]
            //var value = encrypted ? NBrightCore.common.Security.Decrypt(PortalController.Instance.GetCurrentPortalSettings().GUID.ToString(), info.GetXmlProperty(xpath)) : info.GetXmlProperty(xpath);
            if (value == 0 && GeneralUtils.IsNumeric(defaultValue)) value = Convert.ToInt32(defaultValue);

            var typeattr = "type='" + type + "'";
            if (attributes.ToLower().Contains(" type=")) typeattr = "";

            var portalShop = new PortalCatalogLimpet(portalId, cultureCode, systemKey);

            var strOut = "<input value='" + portalShop.CurrencyEdit(value) + "' id='" + id + "' s-xpath='" + xpath + "' " + attributes + " " + upd + " " + typeattr + " />";

            return new RawString(strOut);
        }

        /// <summary>
        /// Gets the interfaces name from the resource file.
        /// </summary>
        /// <param name="rocketInterface">The rocket interface.</param>
        /// <param name="systemData">The system data.</param>
        /// <param name="lang">The language.</param>
        /// <param name="resxFileName">Name of the RESX file.</param>
        /// <returns></returns>
        public IEncodedString InterfaceNameResourceKey(RocketInterface rocketInterface, SystemLimpet systemData, String lang = "", string resxFileName = "SideMenu")
        {
            if (lang == "") lang = DNNrocketUtils.GetCurrentCulture();
            var interfaceName = DNNrocketUtils.GetResourceString(systemData.SystemRelPath + "/App_LocalResources", resxFileName + "." + rocketInterface.InterfaceKey, "Text", lang);
            if (interfaceName == "")
            {
                interfaceName = DNNrocketUtils.GetResourceString(rocketInterface.TemplateRelPath + "/App_LocalResources", resxFileName + "." + rocketInterface.InterfaceKey, "Text", lang);
            }
            if (interfaceName == "")
            {
                interfaceName = rocketInterface.InterfaceKey;
            }
            return new RawString(interfaceName);
        }
        #region "FilterGroups"
        /// <summary>
        /// CheckBox for a group filter. (Used in the ThemeSettings for selecting which group filters to use.)
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="textName">Name of the text.</param>
        /// <returns></returns>
        public IEncodedString FilterGroupCheckBox(SimplisityInfo info, string groupId, string textName)
        {
            return CheckBox(info, "genxml/settings/propertygroup-" + groupId.ToLower(), textName, " class='w3-check' ");
        }
        public IEncodedString FilterGroupCheckBox(SimplisityInfo info, CatalogSettingsLimpet catalogSettings)
        {
            var groupDict = catalogSettings.GetPropertyGroups();
            var rtn = "";
            foreach (var p in groupDict)
            {
                rtn += "<span class=\"w3-padding\">";
                rtn += CheckBox(info, "genxml /settings/propertygroup-" + p.Key.ToLower(), p.Value, " class='w3-check' ").ToString();
                rtn += "</span>";
            }
            return new RawString(rtn);
        }
        #endregion

        #region "Filters"

        /// <summary>
        /// Filters the CheckBox on the "Filters" website view.
        /// </summary>
        /// <param name="checkboxId">The checkbox identifier.</param>
        /// <param name="textName">Name of the text.</param>
        /// <param name="sreturn">The sreturn.</param>
        /// <param name="value">if set to <c>true</c> [value].</param>
        /// <returns></returns>
        public IEncodedString FilterCheckBox(string checkboxId, string textName, string sreturn, bool value, string cssClass = "", string attributes = "")
        {
            return FilterCheckBoxRender(infoempty, "genxml/checkbox/" + checkboxId, textName, " " + attributes + " class='simplisity_sessionfield rocket-filtercheckbox " + cssClass + " '  onchange='simplisity_setSessionField(this.id, this.checked);callFilterArticleList(\"" + sreturn + "\");' ", value);
        }
        private IEncodedString FilterCheckBoxRender(SimplisityInfo info, String xpath, String text, String attributes = "", Boolean defaultValue = false, bool localized = false, int row = 0, string listname = "")
        {
            if (info == null) info = new SimplisityInfo();
            var value = getChecked(info, xpath, defaultValue);
            if (localized && !xpath.StartsWith("genxml/lang/"))
            {
                value = getChecked(info, "genxml/lang/" + xpath, defaultValue);
            }
            var upd = getUpdateAttr(xpath, attributes, localized);
            var id = getIdFromXpath(xpath, row, listname);
            var strOut = "    <input id=\"" + id + "\" s-xpath=\"" + xpath + "\" type=\"checkbox\" " + value + " " + attributes + " " + upd + " /><label for=\"" + id + "\"><span class=\"rocket-filtercheckbox-name\">" + text + "</span><span class=\"rocket-filtercheckbox-display\"></span></label>";
            return new RawString(strOut);
        }

        /// <summary>
        /// Adds the JS for calling the filter API. JS:callArticleList() cmd:remote_publiclist
        /// </summary>
        /// <param name="systemKey">The system key.</param>
        /// <param name="sessionParams">The session parameters.</param>
        /// <returns></returns>
        public IEncodedString FilterJsApiCall(ModuleContentLimpet moduleData, SessionParams sessionParams, string templateName = "articlelist.cshtml")
        {
            var queryCatKey = RocketDirectoryAPIUtils.UrlQueryCategoryKey(PortalUtils.GetCurrentPortalId(), moduleData.SystemKey);
            if (queryCatKey == "") queryCatKey = "nocatkey";
            var strOut = "<script type=\"text/javascript\">";
            strOut += "function callFilterArticleList(sreturn) {";
            strOut += " $('.simplisity_loader').show();";
            strOut += " simplisity_setCookieValue('simplisity_language', '" + sessionParams.CultureCode + "');";
            strOut += " $(sreturn).getSimplisity('/Desktopmodules/dnnrocket/api/rocket/action', 'remote_publiclist', '{\"moduleref\":\"" + moduleData.ApiModuleRef + "\",\"moduleid\":\"" + sessionParams.ModuleId + "\",\"tabid\":\"" + sessionParams.TabId + "\",\"" + queryCatKey + "\":\"" + sessionParams.Get(queryCatKey) + "\",\"systemkey\":\"" + moduleData.SystemKey + "\",\"basesystemkey\":\"rocketdirectoryapi\",\"template\":\"" + templateName + "\"}', '');";
            strOut += " } ";
            strOut += "</script>";
            return new RawString(strOut);
        }
        public IEncodedString FilterClearButton(string textName, string sreturn)
        {
            var js = "$('.rocket-filtercheckbox').each(function(i, obj) { simplisity_setSessionField(this.id, false); $(this).prop('checked', false); });";
            js += "callFilterArticleList('" + sreturn + "');";
            var strOut = "<span class=\"rocket-filterbutton rocket-filterbuttonclear\" onclick=\"" + js + "return false;\">" + textName + "</span>";
            return new RawString(strOut);
        }
        #endregion

        #region "Tags"

        /// <summary>
        /// Tags the button.
        /// </summary>
        /// <param name="propertyid">The propertyid.</param>
        /// <param name="textName">Name of the text.</param>
        /// <param name="cssClasses">The CSS classes.</param>
        /// <param name="activeCssClasses">The active CSS classes.</param>
        /// <param name="sessionParams">The session parameters.</param>
        /// <returns></returns>
        public IEncodedString TagButtonClear(string textName, SessionParams sessionParams)
        {
            var s = "style='display:none;'";
            if (sessionParams.GetInt("rocketpropertyidtag") > 0) s = "";
            var strOut = "<span class='rocket-tagbutton rocket-tagbuttonclear rocket-tagbutton0' propertyid='0' onclick=\"simplisity_setSessionField('disablecache', 'true');simplisity_setSessionField('rocketpropertyidtag', '0');callTagArticleList" + sessionParams.ModuleId + "('0');return false;\" " + s + ">" + textName + "</span>";
            return new RawString(strOut);
        }
        public IEncodedString TagButton(int propertyid, string textName, SessionParams sessionParams)
        {
            string cssClassOn = "rocket-tagbuttonOn";
            var css = "";
            if (propertyid == sessionParams.GetInt("rocketpropertyidtag")) css = cssClassOn;
            var strOut = "<span class='rocket-tagbutton rocket-tagbutton" + propertyid + " " + css + "' propertyid='" + propertyid + "' onclick=\"simplisity_setSessionField('rocketpropertyidtag', '" + propertyid + "');callTagArticleList" + sessionParams.ModuleId + "('" + propertyid + "');return false;\" >" + textName + "</span>";
            return new RawString(strOut);
        }
        /// <summary>
        /// Adds the JS for calling the filter API. JS:callTagArticleList() cmd:remote_publiclist
        /// </summary>
        /// <param name="moduleData"></param>
        /// <param name="sreturn"></param>
        /// <param name="sessionParams"></param>
        /// <param name="templateName"></param>
        /// <returns></returns>
        public IEncodedString TagJsApiCall(ModuleContentLimpet moduleData, string sreturn, SessionParams sessionParams, string templateName = "articlelist.cshtml")
        {
            var queryCatKey = RocketDirectoryAPIUtils.UrlQueryCategoryKey(PortalUtils.GetCurrentPortalId(), moduleData.SystemKey);
            if (queryCatKey == "") queryCatKey = "nocat";
            string cssClassOn = "rocket-tagbuttonOn";
            var strOut = "<script type='text/javascript'>";
            strOut += "    function callTagArticleList" + sessionParams.ModuleId + "(propertyid) {";
            strOut += "        $('.simplisity_loader').show();";
            strOut += "        $('.rocket-tagbutton').removeClass('" + cssClassOn + "');";
            strOut += "        simplisity_setCookieValue('simplisity_language', '" + sessionParams.CultureCode + "');";
            strOut += "        if (propertyid > 0) {";
            strOut += "        $('.rocket-tagbutton' + propertyid).addClass('" + cssClassOn + "');";
            strOut += "        $('.rocket-tagbuttonclear').show();";
            strOut += "        }";
            strOut += "        else";
            strOut += "        {";
            strOut += "        $('.rocket-tagbuttonclear').hide();";
            strOut += "        }";
            strOut += "        $('" + sreturn + "').getSimplisity('/Desktopmodules/dnnrocket/api/rocket/action', 'remote_publiclist', '{\"moduleref\":\"" + moduleData.ApiModuleRef + "\",\"moduleid\":\"" + sessionParams.ModuleId + "\",\"tabid\":\"" + sessionParams.TabId + "\",\"" + queryCatKey + "\":\"" + sessionParams.Get(queryCatKey) + "\",\"systemkey\":\"" + moduleData.SystemKey + "\",\"basesystemkey\":\"rocketdirectoryapi\",\"template\":\"" + templateName + "\"}', '');";
            strOut += "    }";
            strOut += "</script>";
            return new RawString(strOut);
        }

        #endregion

        #region "Date Selection"
        /// <summary>
        /// Dates the js API call. JS function: doDateSearchReload cmd:remote_publiclist
        /// </summary>
        /// <param name="systemKey">systemkey.</param>
        /// <param name="sreturn">sreturn.</param>
        /// <param name="sessionParams">The session parameters.</param>
        /// <returns></returns>
        public IEncodedString DateJsApiCall(ModuleContentLimpet moduleData, string sreturn, SessionParams sessionParams, string templateName = "articlelist.cshtml")
        {
            var catKey = RocketDirectoryAPIUtils.UrlQueryCategoryKey(PortalUtils.GetCurrentPortalId(), moduleData.SystemKey);
            if (catKey == "") catKey = "nocat";
            var strOut = "<script type='text/javascript'>";
            strOut += "    function doDateSearchReload(searchdate1, searchdate2) {";
            strOut += " simplisity_setCookieValue('simplisity_language', '" + sessionParams.CultureCode + "');";
            strOut += "        simplisity_setSessionField('searchdate1', searchdate1);";
            strOut += "        simplisity_setSessionField('searchdate2', searchdate2);";
            strOut += "        $('.simplisity_loader').show();";
            strOut += "        $('" + sreturn + "').getSimplisity('/Desktopmodules/dnnrocket/api/rocket/action', 'remote_publiclist', '{\"disablecache\":\"true\",\"moduleref\":\"" + moduleData.ApiModuleRef + "\",\"moduleid\":\"" + sessionParams.ModuleId + "\",\"tabid\":\"" + sessionParams.TabId + "\",\"" + catKey + "\":\"" + sessionParams.Get(catKey) + "\",\"systemkey\":\"" + moduleData.SystemKey + "\",\"basesystemkey\":\"rocketdirectoryapi\",\"template\":\"" + templateName + "\"}', '');";
            strOut += "    }";
            strOut += "</script>";
            return new RawString(strOut);
        }
        #endregion

        #region "Page Navigation"
        /// <summary>
        /// Builds the List URL.
        /// </summary>
        /// <param name="listpageid">The listpageid.</param>
        /// <param name="categoryId">The category identifier.</param>
        /// <param name="categoryName">Name of the category.</param>
        /// <returns></returns>
        public IEncodedString ListUrl(int listpageid, CategoryLimpet categoryData, string[] urlparams = null)
        {
            return new RawString(RocketDirectoryAPIUtils.ListUrl(listpageid, categoryData, urlparams));
        }
        /// <summary>
        /// Builds the List URL with paramaters.
        /// </summary>
        /// <param name="listpageid"></param>
        /// <param name="urlparams"></param>
        /// <returns></returns>
        public IEncodedString ListUrl(int listpageid, string[] urlparams = null)
        {
            return new RawString(RocketDirectoryAPIUtils.ListUrl(listpageid, urlparams));
        }
        /// <summary>
        /// Builds the Detail URL.
        /// </summary>
        /// <param name="detailpageid">The detailpageid.</param>
        /// <param name="title">The title.</param>
        /// <param name="eId">The row eId.</param>
        /// <returns></returns>
        public IEncodedString DetailUrl(int detailpageid, ArticleLimpet articleData, string[] urlparams = null)
        {
            return new RawString(RocketDirectoryAPIUtils.DetailUrl(detailpageid, articleData, urlparams));
        }
        /// <summary>
        /// Builds the Detail URL.
        /// </summary>
        /// <param name="detailpageid">The detailpageid.</param>
        /// <param name="title">The title.</param>
        /// <param name="eId">The row eId.</param>
        /// <returns></returns>
        public IEncodedString DetailUrl(int detailpageid, ArticleLimpet articleData, CategoryLimpet categoryData, string[] urlparams = null)
        {
            return new RawString(RocketDirectoryAPIUtils.DetailUrl(detailpageid, articleData, categoryData, urlparams));
        }

        #endregion

        public IEncodedString RssUrl(int portalId, string cmd, int yearDate, int monthDate, int numberOfMonths = 1, string sqlidx = "")
        {
            var portalData = new PortalLimpet(portalId);
            var rssurl = portalData.EngineUrlWithProtocol + "/Desktopmodules/dnnrocket/api/rocket/action?cmd=" + cmd + "&year=" + yearDate + "&month=" + monthDate  + "&months=" + numberOfMonths;
            var sqlidxparam = "";
            if (sqlidx != "") sqlidxparam = "&sqlidx=" + sqlidx;
            rssurl = rssurl + sqlidxparam;
            return new RawString(rssurl);
        }
        public IEncodedString ChatGPT(string textId, string sourceTextId = "")
        {
            var globalData = new SystemGlobalData();
            if (String.IsNullOrEmpty(globalData.ChatGptKey)) return new RawString("");
            var apiResx = "/DesktopModules/DNNrocket/api/App_LocalResources/";
            return new RawString("<span class=\"w3-button w3-text-theme\" style=\"width:40px;height:40px;padding:8px 0;\"><span class=\"material-icons\" title=\"" + DNNrocketUtils.GetResourceString(apiResx, "DNNrocket.chatgpt", "Text") + "\" style=\"cursor:pointer;\" onclick=\"$('#chatgptmodal').show();simplisity_setSessionField('chatgpttextid','" + textId + "');simplisity_setSessionField('chatgptcmd','rocketdirectoryapi_chatgpt');$('#chatgptquestion').val($('#" + sourceTextId + "').val());\">sms</span></span>");
        }
        public IEncodedString DeepL(string textId, string sourceTextId = "", string cultureCode = "")
        {
            if (DNNrocketUtils.GetPortalLanguageList().Count <= 1) return new RawString("");
            var globalData = new SystemGlobalData();
            if (String.IsNullOrEmpty(globalData.DeepLauthKey)) return new RawString("");
            var apiResx = "/DesktopModules/DNNrocket/api/App_LocalResources/";
            return new RawString("<span class=\"w3-button w3-text-theme\" style=\"width:40px;height:40px;padding:8px 0;\"><span class=\"material-icons\" title=\"" + DNNrocketUtils.GetResourceString(apiResx, "DNNrocket.translate", "Text", cultureCode) + "\" style=\"cursor:pointer;\" onclick=\"$('#deeplmodal').show();simplisity_setSessionField('deepltextid','" + textId + "');simplisity_setSessionField('deeplcmd','rocketdirectoryapi_deepl');$('#deeplquestion').val(stripHTML($('#" + sourceTextId + "').val()));\">translate</span></span>");
        }

        #region "Obsolete"
        [Obsolete("Use RssUrl(int portalId, string cmd, int yearDate, int monthDate, int numberOfMonths, string sqlidx)")]
        public IEncodedString RssUrl(int portalId, string cmd, int numberOfMonths = 1, string sqlidx = "", int catid = 0)
        {
            return RssUrl(portalId, cmd, DateTime.Now.Year, DateTime.Now.Month, numberOfMonths, sqlidx);
        }
        [Obsolete]
        public string AssigDataModel(SimplisityRazor sModel)
        {
            AssignDataModel(sModel);
            return "";
        }
        [Obsolete("Use FilterClearButton(string textName, string sreturn)")]
        public IEncodedString FilterActionButton(string textName, SessionParams sessionParams, bool action)
        {
            var js = "$('.rocket-filtercheckbox').each(function(i, obj) { simplisity_setSessionField(this.id, " + action.ToString().ToLower() + "); });";
            js += "location.reload();";
            var strOut = "<span class=\"rocket-filterbutton rocket-filterbuttonclear\" onclick=\"" + js + "return false;\">" + textName + "</span>";
            return new RawString(strOut);
        }
        [Obsolete("Use TagJsApiCall(ModuleContentLimpet moduleData, string sreturn, SessionParams sessionParams, string templateName)")]
        public IEncodedString TagJsApiCall(string systemKey, string sreturn, SessionParams sessionParams, string templateName = "articlelist.cshtml")
        {
            var queryCatKey = RocketDirectoryAPIUtils.UrlQueryCategoryKey(PortalUtils.GetCurrentPortalId(), systemKey);
            string cssClassOn = "rocket-tagbuttonOn";
            var strOut = "<script type='text/javascript'>";
            strOut += "    function callTagArticleList" + sessionParams.ModuleId + "(propertyid) {";
            strOut += "        $('.simplisity_loader').show();";
            strOut += "        $('.rocket-tagbutton').removeClass('" + cssClassOn + "');";
            strOut += " simplisity_setCookieValue('simplisity_language', '" + sessionParams.CultureCode + "');";
            strOut += "        simplisity_setSessionField('searchdate1', '');";
            strOut += "        simplisity_setSessionField('searchdate2', '');";
            strOut += "        simplisity_setSessionField('page', '1');";
            strOut += "        if (propertyid > 0) {";
            strOut += "        $('.rocket-tagbutton' + propertyid).addClass('" + cssClassOn + "');";
            strOut += "        $('.rocket-tagbuttonclear').show();";
            strOut += "        }";
            strOut += "        else";
            strOut += "        {";
            strOut += "        $('.rocket-tagbuttonclear').hide();";
            strOut += "        }";
            strOut += "        $('" + sreturn + "').getSimplisity('/Desktopmodules/dnnrocket/api/rocket/action', 'remote_publiclist', '{\"moduleref\":\"" + sessionParams.ModuleRef + "\",\"moduleid\":\"" + sessionParams.ModuleId + "\",\"tabid\":\"" + sessionParams.TabId + "\",\"" + queryCatKey + "\":\"" + sessionParams.Get(queryCatKey) + "\",\"systemkey\":\"" + systemKey + "\",\"basesystemkey\":\"rocketdirectoryapi\",\"template\":\"" + templateName + "\"}', '');";
            strOut += "    }";
            strOut += "</script>";
            return new RawString(strOut);
        }
        [Obsolete("Use FilterJsApiCall(ModuleContentLimpet moduleData, SessionParams sessionParams, string templateName)")]
        public IEncodedString FilterJsApiCall(string systemKey, SessionParams sessionParams, string templateName = "articlelist.cshtml")
        {
            var queryCatKey = RocketDirectoryAPIUtils.UrlQueryCategoryKey(PortalUtils.GetCurrentPortalId(), systemKey);
            if (queryCatKey == "") queryCatKey = "nocatkey";
            var strOut = "<script type=\"text/javascript\">";
            strOut += "function callFilterArticleList(sreturn) {";
            strOut += " $('.simplisity_loader').show();";
            strOut += " simplisity_setCookieValue('simplisity_language', '" + sessionParams.CultureCode + "');";
            strOut += " simplisity_setSessionField('searchdate1', '');";
            strOut += " simplisity_setSessionField('searchdate2', '');";
            strOut += " simplisity_setSessionField('page', '1');";
            strOut += " $(sreturn).getSimplisity('/Desktopmodules/dnnrocket/api/rocket/action', 'remote_publiclist', '{\"moduleref\":\"" + sessionParams.ModuleRef + "\",\"moduleid\":\"" + sessionParams.ModuleId + "\",\"tabid\":\"" + sessionParams.TabId + "\",\"" + queryCatKey + "\":\"" + sessionParams.Get(queryCatKey) + "\",\"systemkey\":\"" + systemKey + "\",\"basesystemkey\":\"rocketdirectoryapi\",\"template\":\"" + templateName + "\"}', '');";
            strOut += " } ";
            strOut += "</script>";
            return new RawString(strOut);
        }
        [Obsolete("Use DateJsApiCall(ModuleContentLimpet moduleData, string sreturn, SessionParams sessionParams, string templateName)")]
        public IEncodedString DateJsApiCall(string systemKey, string sreturn, SessionParams sessionParams, string templateName = "articlelist.cshtml")
        {
            var strOut = "<script type='text/javascript'>";
            strOut += "    function doDateSearchReload(searchdate1, searchdate2) {";
            strOut += " simplisity_setCookieValue('simplisity_language', '" + sessionParams.CultureCode + "');";
            strOut += "        simplisity_setSessionField('searchdate1', searchdate1);";
            strOut += "        simplisity_setSessionField('searchdate2', searchdate2);";
            strOut += "        simplisity_setSessionField('page', '1');";
            strOut += "        $('.simplisity_loader').show();";
            strOut += "        $('" + sreturn + "').getSimplisity('/Desktopmodules/dnnrocket/api/rocket/action', 'remote_publiclist', '{\"disablecache\":\"true\",\"moduleref\":\"" + sessionParams.ModuleRef + "\",\"moduleid\":\"" + sessionParams.ModuleId + "\",\"tabid\":\"" + sessionParams.TabId + "\",\"catid\":\"" + sessionParams.Get(RocketDirectoryAPIUtils.UrlQueryCategoryKey(PortalUtils.GetCurrentPortalId(), systemKey)) + "\",\"systemkey\":\"" + systemKey + "\",\"basesystemkey\":\"rocketdirectoryapi\",\"template\":\"" + templateName + "\"}', '');";
            strOut += "    }";
            strOut += "</script>";
            return new RawString(strOut);
        }

        #endregion
    }
}

