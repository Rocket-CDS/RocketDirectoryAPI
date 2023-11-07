using DNNrocketAPI.Components;
using Newtonsoft.Json.Linq;
using RazorEngine.Text;
using Rocket.AppThemes.Components;
using RocketPortal.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

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

    }
}

