using DNNrocketAPI.Components;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace RocketDirectoryAPI.Components
{
    public class ModuleContentLimpet : ModuleBase
    {
        public ModuleContentLimpet(int portalId, string moduleRef, string systemKey, int moduleid = -1, int tabid = -1) : base(portalId, moduleRef, moduleid, tabid)
        {
            base.SystemKey = systemKey;
        }
        public int DefaultCategoryId { get { return GetSettingInt("defaultcategory"); } }
        public int ListPageTabId()
        {
            var rtn = GetSettingInt("listpage");
            if (rtn == 0)
            {
                var p = new PortalCatalogLimpet(PortalId, DNNrocketUtils.GetCurrentCulture(), SystemKey);
                rtn = p.ListPageTabId;
            }
            return rtn;
        }
        public int DetailPageTabId()
        {
            var rtn = GetSettingInt("detailpage");
            if (rtn == 0)
            {
                var p = new PortalCatalogLimpet(PortalId, DNNrocketUtils.GetCurrentCulture(), SystemKey);
                rtn = p.DetailPageTabId;
            }
            return rtn;
        }
        public Dictionary<string, string> GetPropertyModuleGroups(CatalogSettingsLimpet catalogSettings)
        {
            var rtn = new Dictionary<string, string>();
            foreach (var g in catalogSettings.GetPropertyGroups())
            {
                if (GetSettingBool("propertygroup-" + g.Key))
                {
                    rtn.Add(g.Key, g.Value);
                }
            }
            return rtn;
        }

    }
}
