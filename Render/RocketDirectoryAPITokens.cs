using DNNrocketAPI.Components;
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
        public DefaultsLimpet defaultData;
        public SystemGlobalData globalSettings;
        public AppThemeDataList appThemeDataList;
        public DashboardLimpet dashBoard;

        public string AssigDataModel(SimplisityRazor sModel)
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
            articleData = (ArticleLimpet)sModel.GetDataObject("articledata");
            moduleData = (ModuleContentLimpet)sModel.GetDataObject("modulesettings");
            moduleDataInfo = new SimplisityInfo(moduleData.Record);
            categoryDataList = (CategoryLimpetList)sModel.GetDataObject("categorylist");
            categoryData = (CategoryLimpet)sModel.GetDataObject("categorydata");
            propertyDataList = (PropertyLimpetList)sModel.GetDataObject("propertylist");
            propertyData = (PropertyLimpet)sModel.GetDataObject("propertydata");
            defaultData = (DefaultsLimpet)sModel.GetDataObject("defaultdata");
            globalSettings = (SystemGlobalData)sModel.GetDataObject("globalsettings");
            dashBoard = (DashboardLimpet)sModel.GetDataObject("dashboard");
            articleData = (ArticleLimpet)sModel.GetDataObject("articledata");
            articleDataList = (ArticleLimpetList)sModel.GetDataObject("articlelist");
            sessionParams = sModel.SessionParamsData;
            userParams = (UserParams)sModel.GetDataObject("userparams");

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
        /// <summary>
        /// Textbox for money input
        /// </summary>
        /// <param name="cultureCode">The culture code.</param>
        /// <param name="info">The information.</param>
        /// <param name="xpath">The xpath.</param>
        /// <param name="attributes">The attributes.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="localized">if set to <c>true</c> [localized].</param>
        /// <param name="row">The row.</param>
        /// <param name="listname">The listname.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public IEncodedString TextBoxMoney(string cultureCode, SimplisityInfo info, String xpath, String attributes = "", String defaultValue = "", bool localized = false, int row = 0, string listname = "", string type = "text")
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

            var strOut = "<input value='" + CurrencyUtils.CurrencyEdit(value, cultureCode) + "' id='" + id + "' s-datatype='int' s-xpath='" + xpath + "' " + attributes + " " + upd + " " + typeattr + " />";
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
        /// <summary>
        /// Filters the CheckBox on the "Filters" website view.
        /// </summary>
        /// <param name="checkboxId">The checkbox identifier.</param>
        /// <param name="textName">Name of the text.</param>
        /// <param name="sreturn">The sreturn.</param>
        /// <param name="value">if set to <c>true</c> [value].</param>
        /// <returns></returns>
        public IEncodedString FilterCheckBox(string checkboxId, string textName, string sreturn, bool value)
        {
            return CheckBox(infoempty, "genxml/checkbox/" + checkboxId, "&nbsp;" + textName, " class='simplisity_sessionfield rocket-filtercheckbox'  onchange='simplisity_setSessionField(this.id, this.checked);callArticleList(\"" + sreturn + "\");' ", value);
        }
        /// <summary>
        /// Adds the JS for calling the filter API.
        /// </summary>
        /// <param name="systemKey">The system key.</param>
        /// <param name="sessionParams">The session parameters.</param>
        /// <returns></returns>
        public IEncodedString FilterJsApiCall(string systemKey, SessionParams sessionParams)
        {
            var strOut = "<script type=\"text/javascript\"> function callArticleList(sreturn) {";
            strOut += " $('.simplisity_loader').show();";
            strOut += " $(sreturn).getSimplisity('/Desktopmodules/dnnrocket/api/rocket/action', 'remote_publiclist', '{\"moduleref\":\"" + sessionParams.ModuleRef + "\",\"moduleid\":\"" + sessionParams.ModuleId + "\",\"tabid\":\"" + sessionParams.TabId + "\",\"catid\":\"" + sessionParams.Get("catid") + "\",\"systemkey\":\"" + systemKey + "\",\"basesystemkey\":\"rocketdirectoryapi\",\"template\":\"articlelist.cshtml\"}', '');";
            strOut += " } </script>";
            return new RawString(strOut);
        }
        /// <summary>
        /// Tags the button.
        /// </summary>
        /// <param name="propertyid">The propertyid.</param>
        /// <param name="textName">Name of the text.</param>
        /// <param name="cssClasses">The CSS classes.</param>
        /// <param name="activeCssClasses">The active CSS classes.</param>
        /// <param name="sessionParams">The session parameters.</param>
        /// <returns></returns>
        public IEncodedString TagButton(int propertyid, string textName, string cssClassOff, string cssClassOn, SessionParams sessionParams)
        {
            var css = cssClassOff;
            if (propertyid == sessionParams.GetInt("rocketpropertyidtag")) css = cssClassOn;
            var strOut = "<button type='button' class='rocket-tagbutton rocket-tagbutton" + propertyid + " " + css + "' propertyid='" + propertyid + "' onclick=\"simplisity_setSessionField('rocketpropertyidtag', '" + propertyid + "');callTagArticleList('" + propertyid + "');return false;\" >" + textName + "</button>";
            return new RawString(strOut);
        }
        /// <summary>
        /// Adds the JS for calling the filter API.
        /// </summary>
        /// <param name="systemKey">The system key.</param>
        /// <param name="sreturn">The sreturn.</param>
        /// <param name="sessionParams">The session parameters.</param>
        /// <returns></returns>
        public IEncodedString TagJsApiCall(string systemKey, string sreturn, string cssClassOff, string cssClassOn, SessionParams sessionParams)
        {
            var strOut = "<script type='text/javascript'>";
            strOut += "    function callTagArticleList(propertyid) {";
            strOut += "        $('.simplisity_loader').show();";
            strOut += "        $('.rocket-tagbutton').removeClass('" + cssClassOn + "');";
            strOut += "        $('.rocket-tagbutton').addClass('" + cssClassOff + "');";
            strOut += "        if (propertyid > 0) {";
            strOut += "        $('.rocket-tagbutton' + propertyid).addClass('" + cssClassOn + "');";
            strOut += "        }";
            strOut += "        $('" + sreturn + "').getSimplisity('/Desktopmodules/dnnrocket/api/rocket/action', 'remote_publiclist', '{\"moduleref\":\"" + sessionParams.ModuleRef + "\",\"moduleid\":\"" + sessionParams.ModuleId + "\",\"tabid\":\"" + sessionParams.TabId + "\",\"catid\":\"" + sessionParams.Get("catid") + "\",\"systemkey\":\"" + systemKey + "\",\"basesystemkey\":\"rocketdirectoryapi\",\"template\":\"articlelist.cshtml\"}', '');";
            strOut += "    }";
            strOut += "</script>";
            return new RawString(strOut);
        }
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
        /// <summary>
        /// Builds the List URL.
        /// </summary>
        /// <param name="listpageid">The listpageid.</param>
        /// <param name="categoryId">The category identifier.</param>
        /// <param name="categoryName">Name of the category.</param>
        /// <returns></returns>
        public IEncodedString ListUrl(int listpageid, CategoryLimpet categoryData)
        {
            var listurl = "";
            if (categoryData != null && categoryData.CategoryId > 0)
            {
                string[] urlparams = { "catid", categoryData.CategoryId.ToString(), DNNrocketUtils.UrlFriendly(categoryData.Name)};
                listurl = DNNrocketUtils.NavigateURL(listpageid, urlparams);
            }
            else
            {
                listurl = DNNrocketUtils.NavigateURL(listpageid);
            }
            return new RawString(listurl);
        }
        /// <summary>
        /// Builds the Detail URL.
        /// </summary>
        /// <param name="detailpageid">The detailpageid.</param>
        /// <param name="title">The title.</param>
        /// <param name="eId">The row eId.</param>
        /// <returns></returns>
        public IEncodedString DetailUrl(int detailpageid, ArticleLimpet articleData, CategoryLimpet categoryData)
        {
            var detailurl = "";
            var seotitle = DNNrocketUtils.UrlFriendly(articleData.Name);
            if (categoryData != null && categoryData.CategoryId > 0)
            {
                string[] urlparams = { "articleid", articleData.ArticleId.ToString(), "catid", categoryData.CategoryId.ToString(), seotitle };
                detailurl = DNNrocketUtils.NavigateURL(detailpageid, articleData.CultureCode, urlparams);
            }
            else
            {
                string[] urlparams = { "articleid", articleData.ArticleId.ToString(), seotitle };
                detailurl = DNNrocketUtils.NavigateURL(detailpageid, articleData.CultureCode, urlparams);
            }
            return new RawString(detailurl);
        }

    }
}

