﻿using DNNrocketAPI;
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
            var portalCatalog = new PortalCatalogLimpet(portalId, _sessionParams.CultureCodeEdit, _dataObject.SystemKey);
            portalCatalog.Reset();

            CacheFileUtils.ClearFileCache(portalId);
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
                CacheFileUtils.ClearAllCache(_dataObject.PortalId);
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
                articleDataList.Validate();
            }
            return "OK";
        }
        public string IndexCatalog()
        {
            SearchUtils.DeleteModuleDocuments(_dataObject.PortalContent.PortalId, _dataObject.PortalContent.SearchModuleId);
            DeleteIDX(_dataObject.PortalContent.PortalId, _dataObject.SystemKey);
            foreach (var l in DNNrocketUtils.GetCultureCodeList(_dataObject.PortalContent.PortalId))
            {
                var articleDataList = new ArticleLimpetList(_sessionParams, _dataObject.PortalContent, l, false);
                articleDataList.ReIndex();
            }
            return "OK";
        }
        public void DeleteIDX(int portalId, string systemKey)
        {
            var objCtrl = new DNNrocketController();
            var sqlCmd = "DELETE FROM {databaseOwner}[{objectQualifier}RocketDirectoryAPI] ";
            sqlCmd += " where portalid = " + portalId + " and typecode like 'IDX_%' ";
            sqlCmd += " and textdata = '" + systemKey + "' "; 
            objCtrl.ExecSql(sqlCmd);
        }


    }
}
