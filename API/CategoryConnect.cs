using DNNrocketAPI.Components;
using RocketDirectoryAPI.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace RocketDirectoryAPI.API
{    
    public partial class StartConnect
    {
        private CategoryLimpet GetActiveCategory(int categoryid)
        {
            return new CategoryLimpet(_dataObject.PortalContent.PortalId, categoryid, _sessionParams.CultureCodeEdit, _dataObject.SystemKey);
        }
        public String GetCategory(int categoryId)
        {
            var razorTempl = GetSystemTemplate("categorydetail.cshtml");
            var categoryData = GetActiveCategory(categoryId);
            _dataObject.SetDataObject("categorydata", categoryData);
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, null, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            if (pr.ErrorMsg != "") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public string MoveCategory()
        {
            var parentid = 0;
            var sourceid = _paramInfo.GetXmlPropertyInt("genxml/hidden/sourceid");
            if (sourceid > 0)
            {
                var sourceData = new CategoryLimpet(_dataObject.PortalContent.PortalId, sourceid, _sessionParams.CultureCodeEdit, _dataObject.SystemKey);
                if (sourceData.Exists)
                {
                    parentid = sourceData.ParentItemId;
                    var destparentid = _paramInfo.GetXmlPropertyInt("genxml/hidden/destid");
                    if (destparentid > 0)
                    {
                        var destData = new CategoryLimpet(_dataObject.PortalContent.PortalId, destparentid, _sessionParams.CultureCodeEdit, _dataObject.SystemKey);
                        sourceData.SortOrder = destData.SortOrder + 1;
                    }
                    else
                    {
                        sourceData.SortOrder = -1;  // must be top record.
                    }
                    sourceData.Update();
                    SortCategoryList(sourceData.ParentItemId);
                }
            }
            return GetCategoryList(parentid);
        }
        private void SortCategoryList(int parentid)
        {
            var categoryDataList = new CategoryLimpetList(PortalUtils.GetCurrentPortalId(), _sessionParams.CultureCodeEdit, _dataObject.SystemKey, false);
            categoryDataList.Reload();
            var l = categoryDataList.GetCategoryList(parentid);
            var lp = 1;
            foreach (var c in l)
            {
                c.SortOrder = (lp * 5);
                c.Update();
                lp += 1;
            }
            categoryDataList.Validate(); // clear cache
        }
        public string GetCategoryList(int categoryid)
        {
            _dataObject.CategoryList.SelectedParentId = categoryid;
            var razorTempl = GetSystemTemplate("CategoryList.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, null, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            if (pr.ErrorMsg != "") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public string GetCategoryList()
        {
            return GetCategoryList(_paramInfo.GetXmlPropertyInt("genxml/hidden/categoryid"));
        }
        public String AddCategory()
        {
            var parentid = _paramInfo.GetXmlPropertyInt("genxml/hidden/parentid");
            var razorTempl = GetSystemTemplate("CategoryDetail.cshtml");
            var categoryData = GetActiveCategory(-1);            
            var catcount = _dataObject.CategoryList.GetCategoryList(parentid).Count;

            categoryData.ParentItemId = parentid;
            categoryData.SortOrder = (5 * catcount);
            categoryData.ValidateAndUpdate();

            _dataObject.SetDataObject("categorydata", categoryData);

            _dataObject.CategoryList.Validate();  // clear cache

            var pr = RenderRazorUtils.RazorProcessData(razorTempl, categoryData, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            if (pr.ErrorMsg != "") return pr.ErrorMsg;
            return pr.RenderedText;
        }

        public int SaveCategory()
        {
            var categoryId = _paramInfo.GetXmlPropertyInt("genxml/hidden/categoryid");
            _dataObject.Settings.Add("saved", "true");
            var categoryData = GetActiveCategory(categoryId);

            // clear the category List cache when category saved.
            _dataObject.CategoryList.Reload();

            return categoryData.Save(_postInfo);
        }
        public string DeleteCategory()
        {
            var categoryid = _paramInfo.GetXmlPropertyInt("genxml/hidden/categoryid");
            if (categoryid > 0)
            {
                var categoryData = GetActiveCategory(categoryid);
                categoryData.Delete();
                _dataObject.CategoryList.Reload();
            }
            return GetCategoryList(0);
        }
        public string AddCategoryImage()
        {
            var categoryid = _paramInfo.GetXmlPropertyInt("genxml/hidden/categoryid");
            if (categoryid > 0)
            {
                var categoryData = GetActiveCategory(categoryid);
                categoryData.Save(_postInfo);
                var imgList = ImgUtils.MoveImageToFolder(_postInfo, _dataObject.PortalContent.ImageFolderMapPath);
                categoryData.RemoveImageList();
                foreach (var nam in imgList)
                {
                    categoryData.AddImage(_dataObject.PortalContent.ImageFolderRel, nam);
                }
                _dataObject.CategoryList.Reload();
                return GetCategory(categoryData.CategoryId);
            }
            return "ERROR: Invalid ItemId";
        }
        public string AssignParentCategory()
        {
            var parentid = _paramInfo.GetXmlPropertyInt("genxml/hidden/parentid");
            var categoryid = _paramInfo.GetXmlPropertyInt("genxml/hidden/categoryid");
            if (categoryid > 0 && parentid != categoryid) // check we don't move to itself
            {
                var sourceData = new CategoryLimpet(_dataObject.PortalContent.PortalId, categoryid, _sessionParams.CultureCodeEdit, _dataObject.SystemKey);
                if (sourceData.Exists)                
                {
                    if (!sourceData.HasChild(parentid))  // check we don't move to a child category
                    {
                        sourceData.ParentItemId = parentid;
                        sourceData.SortOrder = -1;  // must be top record.
                        sourceData.Update();
                        SortCategoryList(sourceData.ParentItemId);
                    }
                }
            }
            return GetCategory(categoryid);
        }
        public string AssignDefaultCategory()
        {
            var categoryid = _paramInfo.GetXmlPropertyInt("genxml/hidden/categoryid");
            _dataObject.CatalogSettings.DefaultCategoryId = categoryid;
            _dataObject.CatalogSettings.Update();
            return GetCatalogSettings();
        }        
        public string RemoveCategoryArticle()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            var categoryId = _paramInfo.GetXmlPropertyInt("genxml/hidden/categoryid");
            var articleData = GetActiveArticle(articleId);
            articleData.RemoveCategory(categoryId);
            return GetCategory(categoryId);
        }

    }
}

