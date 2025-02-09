using DNNrocketAPI.Components;
using Rocket.AppThemes.Components;
using RocketPortal.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Xml;

namespace RocketDirectoryAPI.Components
{
    public class DataObjectLimpet
    {
        private Dictionary<string, object> _dataObjects;
        private Dictionary<string, string> _passSettings;
        private string _systemKey;
        private SessionParams _sessionParams;
        private UserParams _userParams;
        public DataObjectLimpet(int portalid, string moduleRef, SessionParams sessionParams, string systemKey, bool editMode = true)
        {
            _sessionParams = sessionParams;
            var cultureCode = sessionParams.CultureCodeEdit;
            if (!editMode) cultureCode = sessionParams.CultureCode;
            Populate(portalid, moduleRef, cultureCode, sessionParams.ModuleId, sessionParams.TabId, systemKey);
        }
        public void Reload()
        {
            Populate(PortalId, ModuleRef, CultureCode, ModuleId, TabId, SystemKey);
        }
        public void Populate(int portalid, string moduleRef, string cultureCode, int moduleId, int tabId, string systemKey, bool refresh = false)
        {
            _systemKey = systemKey;
            _passSettings = new Dictionary<string, string>();
            _dataObjects = new Dictionary<string, object>();
            var portalContent = new PortalCatalogLimpet(portalid, cultureCode, systemKey);
            var systemData = SystemSingleton.Instance(_systemKey);
            _userParams = new UserParams(UserUtils.GetCurrentUserId());
            var moduleUserParams = new UserParams("ModuleID:" + moduleId, true);

            SetDataObject("appthemesystem", AppThemeUtils.AppThemeSystem(portalid, systemKey));
            SetDataObject("appthemedirectory", AppThemeUtils.AppThemeSystem(portalid, "rocketdirectoryapi"));
            SetDataObject("appthemedirectorydefault", AppThemeUtils.AppThemeDefault(portalid, new SystemLimpet("rocketdirectoryapi"), "Default", "1.0"));
            SetDataObject("appthemerocketapi", AppThemeUtils.AppThemeRocketApi(portalid));
            SetDataObject("portaldata", new PortalLimpet(portalid));
            SetDataObject("systemdata", systemData);
            var systemDirectoryData = SystemSingleton.Instance("rocketdirectoryapi");
            SetDataObject("systemdirectorydata", systemDirectoryData);
            SetDataObject("portalcontent", portalContent);
            SetDataObject("appthemeprojects", AppThemeUtils.AppThemeProjects());
            SetDataObject("modulesettings", new ModuleContentLimpet(portalid, moduleRef, systemKey, moduleId, tabId)); 
            SetDataObject("globalsettings", new SystemGlobalData());
            SetDataObject("appthemedefault", AppThemeUtils.AppThemeDefault(portalid, systemData, "Default", "1.0"));
            SetDataObject("apptheme", AppThemeUtils.AppTheme(portalid, portalContent.AppThemeFolder, portalContent.AppThemeVersion, portalContent.ProjectName));
            SetDataObject("appthemedatalist", AppThemeUtils.AppThemeDataList(portalContent.PortalId, portalContent.ProjectName, SystemKey));
            SetDataObject("catalogsettings", new CatalogSettingsLimpet(portalid, cultureCode, SystemKey));
            SetDataObject("categorylist", new CategoryLimpetList(portalid, cultureCode, SystemKey, true));
            SetDataObject("propertylist", new PropertyLimpetList(portalid, cultureCode, SystemKey));
            SetDataObject("dashboard", new DashboardLimpet(portalid, cultureCode));
            SetDataObject("userparams", _userParams);
            SetDataObject("moduleuserparams", moduleUserParams); // communication with DNN RocketDirectoryMod

            ProceesSessionParams();
        }
        /// <summary>
        /// Pre-Process SessionParams, so they are the same across modules.  
        /// This is needed so we can adjust for options selected in the system
        /// </summary>
        /// <param name="sessionParams"></param>
        public void ProceesSessionParams()
        {
            //RULE: Search text will search across ALL categories. 
            if (_sessionParams.SearchText != "") _sessionParams.Set(UrlQueryCategoryKey(), "0");

            // RULE: Clear search on category select.
            if (_sessionParams.GetInt(UrlQueryCategoryKey()) > 0) _sessionParams.SearchText = ""; 
        }
        private bool HasPropertyFilterSelected()
        {
            var nodList = _sessionParams.Info.XMLDoc.SelectNodes("r/*[starts-with(name(), 'checkboxfilter')]");
            if (nodList != null)
            {
                foreach (XmlNode nod in nodList)
                {
                    if (_sessionParams.Info.GetXmlPropertyBool("r/" + nod.Name)) return true;
                }
            }
            return false;
        }
        private void ClearPropertyFilters()
        {
            var nodList = _sessionParams.Info.XMLDoc.SelectNodes("r/*[starts-with(name(), 'checkboxfilter')]");
            if (nodList != null)
            {
                foreach (XmlNode nod in nodList)
                {
                    _sessionParams.Info.SetXmlProperty("r/" + nod.Name, "false");
                }
            }
        }

        public void SetDataObject(String key, object value)
        {
            if (_dataObjects.ContainsKey(key)) _dataObjects.Remove(key);
            _dataObjects.Add(key, value);

            if (key == "modulesettings") // load appTheme if we has settings in ModuleSettings
            {
                if (ModuleSettings.HasProject)
                {
                    SetDataObject("appthemedatalist", new AppThemeDataList(ModuleSettings.PortalId, ModuleSettings.ProjectName, SystemKey));
                    if (ModuleSettings.HasAppThemeAdmin)
                    {
                        var appTheme = new AppThemeLimpet(ModuleSettings.PortalId, ModuleSettings.AppThemeAdminFolder, ModuleSettings.AppThemeAdminVersion, ModuleSettings.ProjectName);
                        SetDataObject("apptheme", appTheme);

                        //appthemeview & appthemeadmin is deprecated
                        SetDataObject("appthemeadmin", appTheme);
                        SetDataObject("appthemeview", appTheme);
                    }
                }
            }

        }
        public object GetDataObject(String key)
        {
            if (_dataObjects != null && _dataObjects.ContainsKey(key)) return _dataObjects[key];
            return null;
        }
        public void SetSetting(string key, string value)
        {
            if (_passSettings.ContainsKey(key)) _passSettings.Remove(key);
            _passSettings.Add(key, value);
        }
        public string GetSetting(string key)
        {
            if (!_passSettings.ContainsKey(key)) return "";
            return _passSettings[key];
        }
        public List<SimplisityRecord> GetAppThemeProjects()
        {
            return AppThemeProjects.List;
        }
        public string UrlQueryCategoryKey()
        {
            return RocketDirectoryAPIUtils.UrlQueryCategoryKey(PortalData.PortalId, _systemKey);
        }
        public int SessionCatId()
        {
            return SessionParamsData.GetInt(RocketDirectoryAPIUtils.UrlQueryCategoryKey(PortalData.PortalId, _systemKey));
        }
        public string UrlQueryArticleKey()
        {
            return RocketDirectoryAPIUtils.UrlQueryArticleKey(PortalData.PortalId, _systemKey);
        }
        public int SessionArticleId()
        {
            return SessionParamsData.GetInt(RocketDirectoryAPIUtils.UrlQueryCategoryKey(PortalData.PortalId, _systemKey));
        }
        public string SystemKey { get { return _systemKey; } }
        public int PortalId { get { return PortalData.PortalId; } }
        public string ModuleRef { get { return ModuleSettings.ModuleRef; } }
        public int ModuleId { get { return ModuleSettings.ModuleId; } }
        public int TabId { get { return ModuleSettings.TabId; } }
        public string CultureCode { get { return PortalContent.CultureCode; } }
        public Dictionary<string, object> DataObjects { get { return _dataObjects; } }
        public ModuleContentLimpet ModuleSettings { get { return (ModuleContentLimpet)GetDataObject("modulesettings"); } }
        public AppThemeSystemLimpet AppThemeSystem { get { return (AppThemeSystemLimpet)GetDataObject("appthemesystem"); } }
        public AppThemeSystemLimpet AppThemeDirectory { get { return (AppThemeSystemLimpet)GetDataObject("appthemedirectory"); } }
        public AppThemeLimpet AppThemeDefault { get { return (AppThemeLimpet)GetDataObject("appthemedefault"); } }
        public PortalCatalogLimpet PortalContent { get { return (PortalCatalogLimpet)GetDataObject("portalcontent"); } }
        public AppThemeLimpet AppTheme { get { return (AppThemeLimpet)GetDataObject("apptheme"); } set { SetDataObject("apptheme", value); } }
        public PortalLimpet PortalData { get { return (PortalLimpet)GetDataObject("portaldata"); } }
        public SystemLimpet SystemData { get { return (SystemLimpet)GetDataObject("systemdata"); } }
        public SystemLimpet SystemDataEcom { get { return (SystemLimpet)GetDataObject("ecomsystemdata"); } }
        public AppThemeProjectLimpet AppThemeProjects { get { return (AppThemeProjectLimpet)GetDataObject("appthemeprojects"); } }
        public Dictionary<string, string> Settings { get { return _passSettings; } }
        public CatalogSettingsLimpet CatalogSettings { get { return (CatalogSettingsLimpet)GetDataObject("catalogsettings"); } }
        public CategoryLimpetList CategoryList { get { return (CategoryLimpetList)GetDataObject("categorylist"); } }
        public PropertyLimpetList PropertyList { get { return (PropertyLimpetList)GetDataObject("propertylist"); } }
        public SessionParams SessionParamsData { get { return _sessionParams; } }
        public UserParams UserParams { get { return _userParams; } }

    }
}
