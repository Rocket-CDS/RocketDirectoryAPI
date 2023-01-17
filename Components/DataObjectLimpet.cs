using DNNrocketAPI.Components;
using Rocket.AppThemes.Components;
using RocketPortal.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace RocketDirectoryAPI.Components
{
    public class DataObjectLimpet
    {
        private Dictionary<string, object> _dataObjects;
        private Dictionary<string, string> _passSettings;
        private string _systemKey;
        public DataObjectLimpet(int portalid, string moduleRef, SessionParams sessionParams, string systemKey, bool editMode = true)
        {
            var cultureCode = sessionParams.CultureCodeEdit;
            if (!editMode) cultureCode = sessionParams.CultureCode;
            Populate(portalid, moduleRef, cultureCode, sessionParams.ModuleId, sessionParams.TabId, systemKey);
        }
        public DataObjectLimpet(int portalid, string moduleRef, string cultureCode, int moduleId, int tabId, string systemKey)
        {
            Populate(portalid, moduleRef,  cultureCode, moduleId, tabId, systemKey);
        }
        public void Populate(int portalid, string moduleRef, string cultureCode, int moduleId, int tabId, string systemKey, bool refresh = false)
        {
            _systemKey = systemKey;
            _passSettings = new Dictionary<string, string>();
            _dataObjects = new Dictionary<string, object>();
            var portalContent = new PortalCatalogLimpet(portalid, cultureCode, SystemKey);
            var systemData = new SystemLimpet(systemKey);

            SetDataObject("appthemesystem", AppThemeUtils.AppThemeSystem(portalid, SystemKey));
            SetDataObject("appthemedirectory", AppThemeUtils.AppThemeSystem(portalid, "rocketdirectoryapi"));
            SetDataObject("portaldata", new PortalLimpet(portalid));
            SetDataObject("systemdata", systemData);
            SetDataObject("portalcontent", portalContent);
            SetDataObject("appthemeprojects", AppThemeUtils.AppThemeProjects());
            SetDataObject("defaultdata", new DefaultsLimpet());
            SetDataObject("modulesettings", new ModuleContentLimpet(portalid, moduleRef)); 
            SetDataObject("globalsettings", new SystemGlobalData());
            SetDataObject("appthemedefault", AppThemeUtils.AppThemeDefault(portalid, systemData, "Default", "1.0"));
            SetDataObject("appthemeview", AppThemeUtils.AppTheme(portalid, portalContent.AppThemeViewFolder, portalContent.AppThemeViewVersion, portalContent.ProjectNameView));
            SetDataObject("appthemeadmin", AppThemeUtils.AppTheme(portalid, portalContent.AppThemeAdminFolder, portalContent.AppThemeAdminVersion, portalContent.ProjectNameView));
            SetDataObject("appthemedatalistview", AppThemeUtils.AppThemeDataList(portalContent.ProjectNameView, SystemKey));
            SetDataObject("appthemedatalistadmin", AppThemeUtils.AppThemeDataList(portalContent.ProjectNameAdmin, SystemKey));
            SetDataObject("catalogsettings", new CatalogSettingsLimpet(portalid, cultureCode, SystemKey));
            SetDataObject("categorylist", new CategoryLimpetList(portalid, cultureCode, SystemKey, true));
            SetDataObject("propertylist", new PropertyLimpetList(portalid, cultureCode, SystemKey));
            SetDataObject("dashboard", new DashboardLimpet(portalid, cultureCode));
        }
        public void SetDataObject(String key, object value)
        {
            if (_dataObjects.ContainsKey(key)) _dataObjects.Remove(key);
            _dataObjects.Add(key, value);
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
        public string SystemKey { get { return _systemKey; } }
        public int PortalId { get { return PortalData.PortalId; } }
        public Dictionary<string, object> DataObjects { get { return _dataObjects; } }
        public AppThemeSystemLimpet AppThemeSystem { get { return (AppThemeSystemLimpet)GetDataObject("appthemesystem"); } }
        public AppThemeSystemLimpet AppThemeDirectory { get { return (AppThemeSystemLimpet)GetDataObject("appthemedirectory"); } }
        public AppThemeLimpet AppThemeDefault { get { return (AppThemeLimpet)GetDataObject("appthemedefault"); } }
        public PortalCatalogLimpet PortalContent { get { return (PortalCatalogLimpet)GetDataObject("portalcontent"); } }
        public AppThemeLimpet AppThemeView { get { return (AppThemeLimpet)GetDataObject("appthemeview"); } set { SetDataObject("appthemeview", value); } }
        public AppThemeLimpet AppThemeAdmin { get { return (AppThemeLimpet)GetDataObject("appthemeadmin"); } set { SetDataObject("appthemeadmin", value); } }
        public PortalLimpet PortalData { get { return (PortalLimpet)GetDataObject("portaldata"); } }
        public SystemLimpet SystemData { get { return (SystemLimpet)GetDataObject("systemdata"); } }
        public SystemLimpet SystemDataEcom { get { return (SystemLimpet)GetDataObject("ecomsystemdata"); } }
        public AppThemeProjectLimpet AppThemeProjects { get { return (AppThemeProjectLimpet)GetDataObject("appthemeprojects"); } }
        public Dictionary<string, string> Settings { get { return _passSettings; } }
        public CatalogSettingsLimpet CatalogSettings { get { return (CatalogSettingsLimpet)GetDataObject("catalogsettings"); } }
        public CategoryLimpetList CategoryList { get { return (CategoryLimpetList)GetDataObject("categorylist"); } }

    }
}
