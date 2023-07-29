using DNNrocketAPI.Components;
using Rocket.AppThemes.Components;
using RocketDirectoryAPI.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using System.Xml;

namespace RocketDirectoryAPI.API
{    
    public partial class StartConnect
    {
        private ArticleLimpet GetActiveArticle(int articleid)
        {
            return new ArticleLimpet(_dataObject.PortalContent.PortalId, articleid, _sessionParams.CultureCodeEdit, _dataObject.SystemKey);
        }

        public int SaveArticle()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            _dataObject.Settings.Add("saved", "true");
            var articleData = new ArticleLimpet(_dataObject.PortalContent.PortalId, articleId, _sessionParams.CultureCodeEdit, _dataObject.SystemKey);
            var rtn = articleData.Save(_postInfo);
            CacheUtils.ClearAllCache("article"); // hbs cache
            return rtn;
        }
        public void DeleteArticle()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            GetActiveArticle(articleId).Delete();
        }
        public void CopyArticle()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            var articleData = GetActiveArticle(articleId);
            var newarticleId = articleData.Copy();
            // create all languages
            var l = DNNrocketUtils.GetCultureCodeList();
            foreach (var c in l)
            {
                articleData = new ArticleLimpet(_dataObject.PortalContent.PortalId, articleId, c, _dataObject.SystemKey);
                var newarticleData = new ArticleLimpet(_dataObject.PortalContent.PortalId, newarticleId, c, _dataObject.SystemKey);
                newarticleData.Info.XMLData = articleData.Info.XMLData;
                newarticleData.Name += " - " + LocalUtils.ResourceKey("RC.copy", "Text", c);
                newarticleData.Update();
            }
            // add categories
            articleData = GetActiveArticle(articleId);
            var newarticleData2 = new ArticleLimpet(_dataObject.PortalContent.PortalId, newarticleId, _sessionParams.CultureCodeEdit, _dataObject.SystemKey);
            foreach (var c in articleData.GetCategories())
            {
                newarticleData2.AddCategory(c.CategoryId);
            }
            // add properties
            foreach (var p in articleData.GetProperties())
            {
                newarticleData2.AddProperty(p.PropertyId);
            }
            newarticleData2.ClearCache();

        }
        public string AddArticleImage()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            var articleData = GetActiveArticle(articleId);
            articleData.Save(_postInfo);

            // Add new image if found in postInfo
            var fileuploadlist = _postInfo.GetXmlProperty("genxml/hidden/fileuploadlist");
            var fileuploadbase64 = _postInfo.GetXmlProperty("genxml/hidden/fileuploadbase64");
            if (fileuploadbase64 != "")
            {
                var filenameList = fileuploadlist.Split('*');
                var filebase64List = fileuploadbase64.Split('*');
                var baseFileMapPath = PortalUtils.TempDirectoryMapPath() + "\\" + GeneralUtils.GetGuidKey();
                var imgsize = _postInfo.GetXmlPropertyInt("genxml/hidden/imageresize");
                if (imgsize == 0) imgsize = _dataObject.PortalContent.ImageResize;
                var destDir = _dataObject.PortalContent.ImageFolderMapPath + "\\" + articleData.ArticleId;
                if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);
                var imgList = ImgUtils.UploadBase64Image(filenameList, filebase64List, baseFileMapPath, destDir, imgsize);
                foreach (var imgFileMapPath in imgList)
                {
                    articleData.AddImage(Path.GetFileName(imgFileMapPath));
                }
                _dataObject.SetDataObject("articledata", articleData);
            }

            var razorTempl = GetSystemTemplate("Articleimages.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, articleData, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            if (pr.ErrorMsg != "") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public string AddArticleImage64()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            if (articleId > 0)
            {
                var articleData = GetActiveArticle(articleId);
                articleData.Save(_postInfo);

                var userfilename = UserUtils.GetCurrentUserId() + "_base64image.jpg";
                var baseFileMapPath = PortalUtils.TempDirectoryMapPath() + "\\" + userfilename;

                var base64image = _postInfo.GetXmlProperty("genxml/base64image");
                var baseArray = base64image.Split(',');
                if (baseArray.Length >= 2)
                {
                    base64image = baseArray[1];
                    var sInfo = new SimplisityInfo();
                    sInfo.SetXmlProperty("genxml/hidden/fileuploadlist", "base64image.jpg");

                    var bytes = Convert.FromBase64String(base64image);
                    using (var imageFile = new FileStream(baseFileMapPath, FileMode.Create))
                    {
                        imageFile.Write(bytes, 0, bytes.Length);
                        imageFile.Flush();
                    }

                    var imgList = ImgUtils.MoveImageToFolder(sInfo, articleData.PortalCatalog.ImageFolderMapPath);
                    foreach (var nam in imgList)
                    {
                        articleData.AddImage(nam);
                    }
                    return GetArticle(articleData);
                }
            }
            return "ERROR: Invalid ItemId or base64 string";
        }
        public string AddArticleDoc()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            var articleData = GetActiveArticle(articleId);
            articleData.Save(_postInfo);

            // Add new image if found in postInfo
            var fileuploadlist = _postInfo.GetXmlProperty("genxml/hidden/fileuploadlist");
            var fileuploadbase64 = _postInfo.GetXmlProperty("genxml/hidden/fileuploadbase64");
            if (fileuploadbase64 != "")
            {
                var filenameList = fileuploadlist.Split('*');
                var filebase64List = fileuploadbase64.Split('*');
                var fileList = DocUtils.UploadBase64file(filenameList, filebase64List, _dataObject.PortalContent.DocFolderMapPath);
                foreach (var imgFileMapPath in fileList)
                {
                    articleData.AddDoc(Path.GetFileName(imgFileMapPath));
                }
                _dataObject.SetDataObject("articledata", articleData);
            }

            var razorTempl = GetSystemTemplate("ArticleDocuments.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, articleData, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            if (pr.ErrorMsg != "") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        private List<string> MoveDocumentToFolder(SimplisityInfo postInfo, string destinationFolder, int maxDocuments = 50)
        {
            destinationFolder = destinationFolder.TrimEnd('\\');
            var rtn = new List<string>();
            var userid = UserUtils.GetCurrentUserId(); // prefix to filename on upload.
            if (!Directory.Exists(destinationFolder)) Directory.CreateDirectory(destinationFolder);
            var fileuploadlist = postInfo.GetXmlProperty("genxml/hidden/fileuploadlist");
            if (fileuploadlist != "")
            {
                var docCount = 1;
                foreach (var f in fileuploadlist.Split(';'))
                {
                    if (f != "")
                    {
                        var friendlyname = GeneralUtils.DeCode(f);
                        var userfilename = userid + "_" + friendlyname;
                        if (docCount <= maxDocuments)
                        {
                            var unqName = DNNrocketUtils.GetUniqueFileName(friendlyname.Replace(" ", "_"), destinationFolder);
                            var fname = destinationFolder + "\\" + unqName;
                            File.Move(PortalUtils.TempDirectoryMapPath() + "\\" + userfilename, fname);
                            if (File.Exists(fname))
                            {
                                rtn.Add(unqName);
                                docCount += 1;
                            }
                        }
                        File.Delete(PortalUtils.TempDirectoryMapPath() + "\\" + userfilename);
                    }
                }
            }
            return rtn;
        }
        public string AddArticleLink()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            if (articleId > 0)
            {
                var articleData = GetActiveArticle(articleId);
                articleData.Save(_postInfo);
                articleData.AddLink();
                var razorTempl = GetSystemTemplate("ArticleLinks.cshtml");
                var pr = RenderRazorUtils.RazorProcessData(razorTempl, articleData, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
                if (pr.ErrorMsg != "") return pr.ErrorMsg;
                return pr.RenderedText;
            }
            return "ERROR: Invalid ItemId";
        }

        public String AddArticle()
        {
            if (_dataObject.PortalContent.ArticleCount < _dataObject.PortalContent.MaxArticles)
            {
                return GetArticle(-1);
            }
            return "ERROR: Article Limit";
        }
        public String GetArticle(int articleId)
        {
            var articleData = GetActiveArticle(articleId);
            return GetArticle(articleData);
        }
        public String GetArticle(ArticleLimpet articleData)
        {
            if (!articleData.Exists)
            {
                // create article, so we dont; get mislinked categories and images, etc.
                var articleId = articleData.Update();
                articleData = GetActiveArticle(articleId);
            }

            _dataObject.SetDataObject("articledata", articleData);

            var razorTempl = _dataObject.AppThemeAdmin.GetTemplate("admindetail.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, articleData, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            if (pr.ErrorMsg != "") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public String GetArticleCategoryList(ArticleLimpet articleData)
        {
            var razorTempl = GetSystemTemplate("ArticleCategoryList.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, articleData, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            return pr.RenderedText;
        }
        public String GetArticlePropertyList(ArticleLimpet articleData)
        {
            var razorTempl = GetSystemTemplate("ArticlePropertyList.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, articleData, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            return pr.RenderedText;
        }
        public String GetArticleList()
        {
            if (_sessionParams.GetInt("articleid") > 0)
            {
                return GetArticle(_sessionParams.GetInt("articleid"));
            }
            else
            {
                if (_dataObject.AppThemeAdmin.AppThemeFolder == "") return "No AppTheme Defined.  Check RocketDirectoryAPI Admin Portal Settings.";
                var articleDataList = new ArticleLimpetList(_sessionParams, _dataObject.PortalContent, _sessionParams.CultureCodeEdit, true, true, 0);
                _dataObject.SetDataObject("articlelist", articleDataList);
                var razorTempl = _dataObject.AppThemeAdmin.GetTemplate("adminlist.cshtml");
                var pr = RenderRazorUtils.RazorProcessData(razorTempl, null, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
                if (pr.ErrorMsg != "") return pr.ErrorMsg;
                return pr.RenderedText;
            }
        }
        public string ArticleDocumentList()
        {
            var articleid = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            var docList = new List<object>();
            foreach (var i in DNNrocketUtils.GetFiles(DNNrocketUtils.MapPath(_dataObject.PortalContent.DocFolderRel)))
            {
                var sInfo = new SimplisityInfo();
                sInfo.SetXmlProperty("genxml/name", i.Name);
                sInfo.SetXmlProperty("genxml/relname", _dataObject.PortalContent.DocFolderRel + "/" + i.Name);
                sInfo.SetXmlProperty("genxml/fullname", i.FullName);
                sInfo.SetXmlProperty("genxml/extension", i.Extension);
                sInfo.SetXmlProperty("genxml/directoryname", i.DirectoryName);
                sInfo.SetXmlProperty("genxml/lastwritetime", i.LastWriteTime.ToShortDateString());
                docList.Add(sInfo);
            }

            _dataObject.Settings.Add("uploadcmd", "articleadmin_docupload");
            _dataObject.Settings.Add("deletecmd", "articleadmin_docdelete");
            _dataObject.Settings.Add("articleid", articleid.ToString());

            var razorTempl = GetSystemTemplate("DocumentSelect.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, docList, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            return pr.RenderedText;
        }
        public void ArticleDocumentUploadToFolder()
        {
            var userid = UserUtils.GetCurrentUserId(); // prefix to filename on upload.
            if (!Directory.Exists(_dataObject.PortalContent.DocFolderMapPath)) Directory.CreateDirectory(_dataObject.PortalContent.DocFolderMapPath);
            var fileuploadlist = _paramInfo.GetXmlProperty("genxml/hidden/fileuploadlist");
            if (fileuploadlist != "")
            {
                foreach (var f in fileuploadlist.Split(';'))
                {
                    if (f != "")
                    {
                        var friendlyname = GeneralUtils.DeCode(f);
                        var userfilename = userid + "_" + friendlyname;
                        File.Copy(PortalUtils.TempDirectoryMapPath() + "\\" + userfilename, _dataObject.PortalContent.DocFolderMapPath + "\\" + friendlyname, true);
                        File.Delete(PortalUtils.TempDirectoryMapPath() + "\\" + userfilename);
                    }
                }

            }
        }
        public void ArticleDeleteDocument()
        {
            var docfolder = _postInfo.GetXmlProperty("genxml/hidden/documentfolder");
            if (docfolder == "") docfolder = "docs";
            var docDirectory = PortalUtils.HomeDNNrocketDirectoryMapPath() + "\\" + docfolder;
            var docList = _postInfo.GetXmlProperty("genxml/hidden/dnnrocket-documentlist").Split(';');
            foreach (var i in docList)
            {
                if (i != "")
                {
                    var documentname = GeneralUtils.DeCode(i);
                    var docFile = docDirectory + "\\" + documentname;
                    if (File.Exists(docFile))
                    {
                        File.Delete(docFile);
                    }
                }
            }

        }
        public string AssignArticleProperty()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            var propertyId = _paramInfo.GetXmlPropertyInt("genxml/hidden/propertyid");
            var articleData = GetActiveArticle(articleId);
            articleData.AddProperty(propertyId);

            return GetArticlePropertyList(articleData);
        }
        public string RemoveArticleProperty()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            var propertyId = _paramInfo.GetXmlPropertyInt("genxml/hidden/propertyid");
            var articleData = GetActiveArticle(articleId);
            articleData.RemoveProperty(propertyId);

            return GetArticlePropertyList(articleData);
        }
        public string AssignArticleCategory()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            var categoryId = _paramInfo.GetXmlPropertyInt("genxml/hidden/categoryid");
            var articleData = GetActiveArticle(articleId);
            articleData.AddCategory(categoryId);
            articleData = GetActiveArticle(articleId); // reload
            return GetArticleCategoryList(articleData);
        }
        public string AssignDefaultArticleCategory()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            var categoryId = _paramInfo.GetXmlPropertyInt("genxml/hidden/categoryid");
            var articleData = GetActiveArticle(articleId);
            articleData.SetDefaultCategory(categoryId);
            articleData = GetActiveArticle(articleId); // reload
            return GetArticleCategoryList(articleData);
        }
        public string RemoveArticleCategory()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            var categoryId = _paramInfo.GetXmlPropertyInt("genxml/hidden/categoryid");
            var articleData = GetActiveArticle(articleId);
            articleData.RemoveCategory(categoryId);

            return GetArticleCategoryList(articleData);
        }
        private string GetPublicView(string template)
        {
            var razorTempl = _dataObject.AppThemeView.GetTemplate(template);
            if (razorTempl == "") return "";
            var articleid = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            if (articleid == 0) articleid = _paramInfo.GetXmlPropertyInt("genxml/urlparams/articleid");
            if (articleid == 0) articleid = _paramInfo.GetXmlPropertyInt("genxml/remote/urlparams/articleid");
            if (articleid > 0)
            {
                var articleData = new ArticleLimpet(_dataObject.PortalContent.PortalId, articleid, _sessionParams.CultureCode, _dataObject.SystemKey);
                _dataObject.DataObjects.Remove("articledata");
                _dataObject.DataObjects.Add("articledata", articleData);  
            }
            if (!_dataObject.DataObjects.ContainsKey("paraminfo")) _dataObject.DataObjects.Add("paraminfo", _paramInfo); // we need this so we can check if a detail key has been passed.  if so, we need to do the SEO for the detail.            
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, _dataObject.PortalContent, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, _dataObject.PortalContent.DebugMode);
            return pr.RenderedText;
        }
        public string GetPublicArticleHeader()
        {
            var template = _paramInfo.GetXmlProperty("genxml/hidden/template");
            if (template == "") template = "ViewHeader.cshtml";
            return GetPublicView(template);
        }

        public string GetPublicArticleBeforeHeader()
        {
            var template = _paramInfo.GetXmlProperty("genxml/hidden/template");
            if (template == "") template = "ViewBeforeHeader.cshtml";
            return GetPublicView(template);
        }

        public String GetPublicArticleSEO()
        {
            // check if we are looking for a detail page. 
            var sRec = new SimplisityRecord();
            var articleid = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            if (articleid == 0) articleid = _paramInfo.GetXmlPropertyInt("genxml/urlparams/articleid");
            if (articleid == 0) articleid = _paramInfo.GetXmlPropertyInt("genxml/remote/urlparams/articleid");
            if (articleid == 0) articleid = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            if (articleid > 0)
            {
                // do detail
                var articleData = new ArticleLimpet(_dataObject.PortalContent.PortalId, articleid, _sessionParams.CultureCode, _dataObject.SystemKey);
                var seotitle = articleData.SeoTitle;
                if (seotitle == "") seotitle = articleData.Name;
                var seodesc = articleData.SeoDescription;
                if (seodesc == "") seodesc = articleData.Summary;

                sRec.SetXmlProperty("genxml/title", seotitle);
                sRec.SetXmlProperty("genxml/description", seodesc);
                sRec.SetXmlProperty("genxml/keywords", articleData.SeoKeyWords);
            }
            return sRec.ToXmlItem();
        }
        public String GetPublicBase()
        {
            // Do product list
            var template = _paramInfo.GetXmlProperty("genxml/hidden/template");
            if (template == "") template = "View.cshtml";
            var razorTempl = _dataObject.AppThemeView.GetTemplate(template);
            if (razorTempl == "") return "No Razor Template.  Check engine server. Theme: '" + _dataObject.AppThemeView.AppThemeFolder;
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, null, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, _dataObject.PortalContent.DebugMode);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }

        public String GetPublicArticleList()
        {
            if (_dataObject.PortalContent.DebugMode) LogUtils.LogSystem(_storeParamCmd + " START - GetPublicArticleList: " + DateTime.Now.ToString("hh:mm:ss.fff"));

            // assume we want a product page, if we have a productid
            var productid = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            if (productid == 0) productid = _paramInfo.GetXmlPropertyInt("genxml/remote/urlparams/articleid");
            if (productid > 0 && !_paramInfo.GetXmlPropertyBool("genxml/hidden/staticlist"))
            {
                return GetPublicProductDetail();
            }

            // Do product list
            var template = _paramInfo.GetXmlProperty("genxml/hidden/template");
            if (template == "") template = "View.cshtml";
            var razorTempl = _dataObject.AppThemeView.GetTemplate(template);
            if (razorTempl == "") razorTempl = _dataObject.AppThemeAdmin.GetTemplate(template);
            if (razorTempl == "") return "No Razor Template.  Check engine server. Theme: '" + _dataObject.AppThemeView.AppThemeFolder;

            // add the default static catid to the url data.
            if (_paramInfo.GetXmlPropertyBool("genxml/hidden/staticlist"))
            {
                _paramInfo.SetXmlProperty("genxml/urlparams/catid", _paramInfo.GetXmlPropertyInt("genxml/remote/categoryid").ToString());
            }

            var articleDataList = new ArticleLimpetList(_sessionParams, _dataObject.PortalContent, _sessionParams.CultureCode, true, false, _dataObject.CatalogSettings.DefaultCategoryId);
            _dataObject.SetDataObject("articlelist", articleDataList);
            var categoryData = new CategoryLimpet(_dataObject.PortalContent.PortalId, articleDataList.CategoryId, _sessionParams.CultureCode, _dataObject.SystemKey);
            _dataObject.SetDataObject("categorydata", categoryData);

            var pr = RenderRazorUtils.RazorProcessData(razorTempl, null, _dataObject.DataObjects, _dataObject.Settings, articleDataList.SessionParamData, _dataObject.PortalContent.DebugMode);

            if (_dataObject.PortalContent.DebugMode) LogUtils.LogSystem(_storeParamCmd + " END - GetPublicArticleList: " + DateTime.Now.ToString("hh:mm:ss.fff"));

            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public String GetPublicProductDetail()
        {
            if (_dataObject.PortalContent.DebugMode) LogUtils.LogSystem(_storeParamCmd + " START - GetPublicProductDetail: " + DateTime.Now.ToString("hh:mm:ss.fff"));

            var productid = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            if (productid == 0) productid = _paramInfo.GetXmlPropertyInt("genxml/remote/urlparams/articleid");
            var articleData = new ArticleLimpet(_dataObject.PortalContent.PortalId, productid, _sessionParams.CultureCode, _dataObject.SystemKey);

            if (!articleData.Exists) return "404";

            var template = _paramInfo.GetXmlProperty("genxml/hidden/template");
            if (template == "") template = "View.cshtml";
            var razorTempl = _dataObject.AppThemeView.GetTemplate(template);
            if (razorTempl == "") return "No Razor Template.  Check engine server. Theme: '" + _dataObject.AppThemeView.AppThemeFolder;

            //var articleDataList = new ArticleLimpetList(_paramInfo, _dataObject.PortalContent, _sessionParams.CultureCode, true, false, _dataObject.CatalogSettings.DefaultCategoryId);
            var categoryData = new CategoryLimpet(_dataObject.PortalContent.PortalId, articleData.DefaultCategory(), _sessionParams.CultureCode, _dataObject.SystemKey);

            _dataObject.SetDataObject("articledata", articleData);
            //_dataObject.SetDataObject("articlelist", articleDataList);
            _dataObject.SetDataObject("categorydata", categoryData);

            var pr = RenderRazorUtils.RazorProcessData(razorTempl, null, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, _dataObject.PortalContent.DebugMode);

            if (_dataObject.PortalContent.DebugMode) LogUtils.LogSystem(_storeParamCmd + " END - GetPublicProductDetail: " + DateTime.Now.ToString("hh:mm:ss.fff"));

            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        private string AddArticleListItem()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            var articleData = GetActiveArticle(articleId);
            articleData.Save(_postInfo);
            var listName = _paramInfo.GetXmlProperty("genxml/hidden/listname");
            articleData.Info.AddListItem(listName, new SimplisityInfo());
            return GetArticle(articleData);
        }


    }
}

