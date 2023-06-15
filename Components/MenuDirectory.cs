using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Web;
using System.Web.Razor.Parser.SyntaxTree;
using DNNrocketAPI;
using DNNrocketAPI.Components;

namespace RocketDirectoryAPI.Components
{
    public class MenuDirectory : IMenuInterface
    {
        private CategoryLimpetList _categoryDataList;
        private string _systemkey = "rocketdirectoryapi";
        public List<PageRecordData> GetMenuItems(int portalId, string cultureCode, string systemkey, string rootRef = "")
        {
            if (!String.IsNullOrEmpty(systemkey)) _systemkey = systemkey;

            var rtn = new List<PageRecordData>();
            var portalContent = new PortalCatalogLimpet(portalId, cultureCode, _systemkey);
            _categoryDataList = new CategoryLimpetList(portalId, cultureCode, _systemkey);
            var rootId = ParentId(rootRef);
            var treelist = _categoryDataList.GetCategoryTree(rootId);
            foreach (var catData in treelist)
            {
                var p = new PageRecordData();
                p.Name = catData.Name;
                p.KeyWords = catData.Keywords;
                p.Description = catData.Summary;
                p.Title = catData.Name;
                if (catData.ParentItemId == 0)
                    p.ParentPageId = 0;
                else
                    p.ParentPageId = catData.ParentItemId;
                p.PageId = catData.CategoryId;
                p.Url = PagesUtils.NavigateURL(portalContent.ListPageTabId) + "/catid/" + catData.CategoryId + "/" + DNNrocketUtils.UrlFriendly(catData.Name);
                rtn.Add(p);
            }
            return rtn;
        }


        public string TokenPrefix()
        {
            return "[CATDIR";
        }
        public int PageId(int portalId, string cultureCode)
        {
            var portalContent = new PortalCatalogLimpet(portalId, cultureCode, _systemkey);
            return portalContent.ListPageTabId;
        }
        public int ParentId(string rootRef)
        {
            var rootId = 0;
            if (rootRef == "") return rootId;
            var rootCatList = _categoryDataList.GetCategoryByRef(rootRef);
            if (rootCatList.Count > 0) rootId = rootCatList.First().CategoryId;
            return rootId;
        }
    }

}
