﻿using DNNrocketAPI;
using DNNrocketAPI.Components;
using Simplisity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;

namespace RocketDirectoryAPI.Components
{
    public class PropertyLimpetList
    {
        public const string _tableName = "RocketDirectoryAPI";
        private List<PropertyLimpet> _propertyList;
        private DNNrocketController _objCtrl;
        private string _searchText;
        private string _systemKey;
        private string _cachekey;

        public PropertyLimpetList(int portalId, string langRequired, string systemKey, string searchText = "")
        {
            _searchText = searchText;
            _systemKey = systemKey;
            PortalId = portalId;
            CultureCode = langRequired;
            EntityTypeCode = systemKey + "PROP";
            TableName = _tableName;

            _cachekey = PortalId + "*" + _systemKey + "*" + CultureCode + "*" + systemKey + "PROP" + _tableName;

            if (CultureCode == "") CultureCode = DNNrocketUtils.GetCurrentCulture();
            _objCtrl = new DNNrocketController();

            Populate();
        }
        public void Populate()
        {
            var filter = "";
            if (_searchText != "") filter = " and [XMLData].value('(genxml/lang/genxml/textbox/name)[1]','nvarchar(max)') like '%" + _searchText + "%' ";
            DataList = (List<SimplisityInfo>)CacheUtils.GetCache(_cachekey + "SimplisityInfo", _systemKey + PortalId);
            _propertyList = (List<PropertyLimpet>)CacheUtils.GetCache(_cachekey + "PropertyLimpet", _systemKey + PortalId);
            if (DataList == null || _propertyList == null || filter != "")
            {
                DataList = _objCtrl.GetList(PortalId, -1, EntityTypeCode, filter, CultureCode, " order by R1.SortOrder desc, [XMLData].value('(genxml/textbox/ref)[1]','nvarchar(max)') ", 0, 0, 0, 0, TableName);
                PopulatePropertyList();
                if (filter == "") // do not cache if we are searching.
                {
                    CacheUtils.SetCache(_cachekey + "SimplisityInfo", DataList, _systemKey + PortalId);
                    CacheUtils.SetCache(_cachekey + "PropertyLimpet", _propertyList, _systemKey + PortalId);
                }
            }
        }
        public void ClearCache()
        {
            CacheUtils.RemoveCache(_cachekey + "SimplisityInfo", _systemKey + PortalId);
            CacheUtils.RemoveCache(_cachekey + "PropertyLimpet", _systemKey + PortalId);
        }
        public void DeleteAll()
        {
            foreach (var r in DataList)
            {
                _objCtrl.Delete(r.ItemID);
            }
            ClearCache();
        }
        public List<SimplisityInfo> DataList { get; private set; }
        public int PortalId { get; set; }
        public string TableName { get; set; }
        public string EntityTypeCode { get; set; }
        public string CultureCode { get; set; }
        public List<PropertyLimpet> GetPropertyList(string groupRef = "")
        {
            if (groupRef != "")
            {
                var rtnList = new List<PropertyLimpet>();
                foreach (var p in _propertyList)
                {
                    if (p.IsInGroup(groupRef)) rtnList.Add(p);
                }
                return rtnList;
            }
            return _propertyList;
        }
        public Dictionary<string,string> GetPropertyFilterList(string groupKey)
        {
            var rtn = new Dictionary<string, string>();
            foreach (var p in GetPropertyList(groupKey))
            {
                var filterId = "checkboxfilter" + p.PropertyId + "-" + groupKey;
                rtn.Add(filterId, p.Name);
            }
            return rtn;
        }
        public Dictionary<int, string> GetPropertyTagList(string groupKey)
        {
            var rtn = new Dictionary<int, string>();
            foreach (var p in GetPropertyList(groupKey))
            {
                rtn.Add(p.PropertyId, p.Name);
            }
            return rtn;
        }
        private List<PropertyLimpet> PopulatePropertyList()
        {
            _propertyList = new List<PropertyLimpet>();
            foreach (var o in DataList)
            {
                var propertyData = new PropertyLimpet(PortalId, o.ItemID, CultureCode, _systemKey);
                _propertyList.Add(propertyData);
            }
            return _propertyList;
        }
        public void Validate()
        {
            // validate properties
            foreach (var pInfo in DataList)
            {
                var propertyData = new PropertyLimpet(PortalId, pInfo.ItemID, CultureCode, _systemKey);
                propertyData.ValidateAndUpdate();
            }
            Reload();
        }
        /// <summary>
        /// Clear cache and Reload list 
        /// </summary>
        public void Reload()
        {
            ClearCache();
            Populate();
        }
    }

}
