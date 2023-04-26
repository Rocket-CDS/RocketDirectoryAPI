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
        private string _systemkey = "rocketdirectoryapi";
        public List<PageRecordData> GetMenuItems(int portalId, string cultureCode, string rootRef = "")
        {
            var rtn = new List<PageRecordData>();
            var categoryDataList = new CategoryLimpetList(portalId, cultureCode, _systemkey);
            var treelist = categoryDataList.GetCategoryTree();
            foreach (var catData in treelist)
            {
                var p = new PageRecordData();
                p.Name = catData.Name;
                p.KeyWords = catData.Keywords;
                p.Description = catData.Summary;
                p.Title = catData.Name;
                p.ParentPageId = catData.ParentItemId;
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
    }

}
