using DNNrocketAPI;
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
    public class CategoryLimpetList
    {
        public const string _tableName = "RocketDirectoryAPI";
        private List<CategoryLimpet> _categoryList;
        private DNNrocketController _objCtrl;
        private string _cacheKey;
        private string _cacheGroup;
        private string _systemKey;        

        public CategoryLimpetList(int portalId, string langRequired, string systemKey, bool populate = true)
        {
            _systemKey = systemKey;
            PortalId = portalId;
            CultureCode = langRequired;
            EntityTypeCode = _systemKey + "CAT";
            TableName = _tableName;

            if (CultureCode == "") CultureCode = DNNrocketUtils.GetCurrentCulture();
            _objCtrl = new DNNrocketController();

            _cacheKey = PortalId + "*" + EntityTypeCode + "*" + CultureCode + "*" + _tableName + "*" + systemKey;
            _cacheGroup = systemKey + PortalId;

            if (populate) Populate();
        }
        public void Populate()
        {
            DataList = (List<SimplisityInfo>)CacheUtils.GetCache(_cacheKey, _cacheGroup);
            if (DataList == null)
            {
                DataList = _objCtrl.GetList(PortalId, -1, EntityTypeCode, "", CultureCode, " order by R1.SortOrder", 0, 0, 0, 0, TableName);
                CacheUtils.SetCache(_cacheKey, DataList, _cacheGroup);
            }
            PopulateCategoryList();
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
        public int SelectedParentId { get; set; }
        public List<CategoryLimpet> GetBreadCrumbList(int parentid = -1)
        {
            if (parentid == -1) parentid = SelectedParentId;
            var cacheKey = _cacheKey + "GetBreadCrumbList" + parentid;
            var rtnList = (List<CategoryLimpet>)CacheUtils.GetCache(cacheKey, _cacheGroup);
            if (rtnList != null) return rtnList;

            rtnList = new List<CategoryLimpet>();
            var exitloop = false;
            var lp = 0; // stop any possible infinate loop caused by data coruption
            while (parentid != 0 && !exitloop)
            {
                var categoryData = new CategoryLimpet(PortalId, parentid, CultureCode, _systemKey);
                if (categoryData.Exists && lp < 100)
                {
                    rtnList.Add(categoryData);
                    parentid = categoryData.ParentItemId;
                }
                else
                {
                    exitloop = true;
                }
                lp += 1;
            }
            rtnList.Reverse();
            CacheUtils.SetCache(cacheKey, rtnList, _cacheGroup);
            return rtnList;
        }
        public List<CategoryLimpet> GetCategoryList()
        {
            return _categoryList;
        }
        public List<CategoryLimpet> GetCategoryList(int parentid)
        {
            List<CategoryLimpet> newList = _categoryList.Where(m => m.ParentItemId == parentid).ToList();
            return newList;
        }
        public List<CategoryLimpet> GetCategoryByRef(string catRef)
        {
            List<CategoryLimpet> newList = _categoryList.Where(m => m.Ref == catRef).ToList();
            return newList;
        }
        private List<CategoryLimpet> PopulateCategoryList()
        {
            _categoryList = (List<CategoryLimpet>)CacheUtils.GetCache(_cacheKey + "PopulateCategoryList", _cacheGroup);
            if (_categoryList != null) return _categoryList;

            _categoryList = new List<CategoryLimpet>();
            var sortorder = 0;
            foreach (var o in DataList)
            {
                var categoryData = new CategoryLimpet(PortalId, o.ItemID, CultureCode, _systemKey);
                if (categoryData.SortOrder != sortorder) // update of sortorder, save
                {
                    categoryData.SortOrder = sortorder;
                    categoryData.Update();
                    ClearCache();
                }
                _categoryList.Add(categoryData);
                sortorder += 5;
            }
            CacheUtils.SetCache(_cacheKey + "PopulateCategoryList", _categoryList, _cacheGroup);
            return _categoryList;
        }
        public List<CategoryLimpet> GetCategoryTree(int parentId = 0)
        {
            var treeList = new List<CategoryLimpet>();
            var rtn = (List<CategoryLimpet>)CacheUtils.GetCache(_cacheKey + "PopulateCategoryTreeList" + parentId, _cacheGroup);
            if (rtn == null)
            {
                rtn = GetCategoryRecursive(parentId, treeList, 0);
                CacheUtils.SetCache(_cacheKey + "PopulateCategoryTreeList" + parentId, rtn, _cacheGroup);
            }
            return rtn;
        }
        public List<CategoryLimpet> GetCategoryRecursive(int parentid, List<CategoryLimpet> treeList, int level)
        {
            List<CategoryLimpet> newList = _categoryList.Where(m => m.ParentItemId == parentid).ToList();
            if (newList.Count > 0)
            {
                foreach (var categoryData in newList)
                {
                    categoryData.Level = level;
                    treeList.Add(categoryData);
                    if (categoryData.CategoryId != parentid)
                    {
                        GetCategoryRecursive(categoryData.CategoryId, treeList, (level + 1));
                    }
                }
            }
            return treeList;
        }
        public void Validate()
        {
            // validate categories
            foreach (var pInfo in DataList)
            {
                var categoryData = new CategoryLimpet(PortalId, pInfo.ItemID, CultureCode, _systemKey);
                categoryData.ValidateAndUpdate();
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
        public void ClearCache()
        {
            CacheFileUtils.ClearAllCache(PortalId, _cacheGroup);
        }
    }

}
