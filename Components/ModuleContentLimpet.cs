using DNNrocketAPI.Components;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace RocketDirectoryAPI.Components
{
    public class ModuleContentLimpet : ModuleBase
    {
        private PortalCatalogLimpet _portalContent; 
        public ModuleContentLimpet(int portalId, string moduleRef, string systemKey, int moduleid = -1, int tabid = -1) : base(portalId, moduleRef, moduleid, tabid)
        {
            base.SystemKey = systemKey;
            _portalContent = new PortalCatalogLimpet(PortalId, DNNrocketUtils.GetCurrentCulture(), SystemKey);
            base.AppThemeAdminFolder = _portalContent.AppThemeFolder;
            base.AppThemeAdminVersion = _portalContent.AppThemeVersion;
            base.ProjectName = _portalContent.ProjectName;

        }
        public string ApiModuleRef { get { if (String.IsNullOrEmpty(GetSetting("apimoduleref"))) return ModuleRef; else  return GetSetting("apimoduleref"); } }
        public int DefaultCategoryId { get { return GetSettingInt("defaultcategory"); } }
        public int ListPageTabId()
        {
            var rtn = GetSettingInt("listpage");
            if (rtn == 0) rtn = _portalContent.ListPageTabId;
            return rtn;
        }
        public int DetailPageTabId()
        {
            var rtn = GetSettingInt("detailpage");
            if (rtn == 0) rtn = _portalContent.DetailPageTabId;
            return rtn;
        }
        public Dictionary<string, string> GetPropertyModuleGroups(CatalogSettingsLimpet catalogSettings)
        {
            var rtn = new Dictionary<string, string>();
            foreach (var g in catalogSettings.GetPropertyGroups())
            {
                if (GetSettingBool("propertygroup-" + g.Key.ToLower()))
                {
                    rtn.Add(g.Key.ToLower(), g.Value);
                }
            }
            return rtn;
        }

    }
}
