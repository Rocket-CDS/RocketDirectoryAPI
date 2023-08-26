using DNNrocketAPI.Components;
using RocketDirectoryAPI.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Xml;

namespace RocketDirectoryAPI.API
{
    public partial class StartConnect
    {
        private void ResetPortalCatalog(int portalId)
        {
            var defaultFileMapPath = DNNrocketUtils.MapPath(_rocketInterface.TemplateRelPath).TrimEnd('\\') + "\\default_portalcatalog.xml";
            var defaultxml = FileUtils.ReadFile(defaultFileMapPath);
            var portalCatalog = new PortalCatalogLimpet(portalId, _sessionParams.CultureCodeEdit, _dataObject.SystemKey);

            var sitekey = portalCatalog.SiteKey;

            if (defaultxml != "")
            {
                var tempInfo = new SimplisityInfo();
                tempInfo.FromXmlItem(defaultxml);
                portalCatalog.Record.XMLData = tempInfo.XMLData;
            }

            portalCatalog.SiteKey = sitekey;

            portalCatalog.Update();

            CacheUtils.ClearAllCache();
            CacheUtilsDNN.ClearAllCache();
            DNNrocketUtils.ClearAllCache();
        }

        private string SaveDashboard()
        {
            var portalId = _paramInfo.GetXmlPropertyInt("genxml/hidden/portalid"); // we may have passed selection
            if (portalId >= 0)
            {
                var portalDashboard = new DashboardLimpet(portalId, _sessionParams.CultureCodeEdit);
                if (portalDashboard.PortalId >= 0)
                {
                    portalDashboard.Save(_postInfo);
                }
                return GetDashboard(); 
            }
            return "Invalid Portal SiteKey";
        }
        private string SavePortalCatalog()
        {
            var portalId = _paramInfo.GetXmlPropertyInt("genxml/hidden/portalid"); 
            if (portalId >= 0)
            {
                _dataObject.PortalContent.Save(_postInfo);
                _dataObject.PortalData.Record.SetXmlProperty("genxml/systems/" + _dataObject.SystemKey + "setup", "True");
                _dataObject.PortalData.Record.SetXmlProperty("genxml/systems/" + _dataObject.SystemKey, "True");
                _dataObject.PortalData.Update();
                return "OK";
            }
            return "Invalid Portal SiteKey";
        }
        private void DeletePortalCatalog()
        {
            var portalId = _paramInfo.GetXmlPropertyInt("genxml/hidden/portalid"); // we may have passed selection
            if (portalId >= 0)
            {
                PortalUtils.DeletePortal(portalId); // Delete base portal will crash install.
                DNNrocketUtils.RecycleApplicationPool();
                _dataObject.PortalContent.Delete();
                _userParams.TrackClear(_dataObject.SystemKey);
            }
        }
        public string ValidateCatalog()
        {
            foreach (var l in DNNrocketUtils.GetCultureCodeList(_dataObject.PortalContent.PortalId))
            {
                var articleDataList = new ArticleLimpetList(_sessionParams, _dataObject.PortalContent, l, false);
                articleDataList.Validate(); // Will also reindex.
            }
            return "OK";
        }


    }
}
