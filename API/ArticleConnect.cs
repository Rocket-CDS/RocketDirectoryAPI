using DNNrocketAPI;
using DNNrocketAPI.Components;
using Rocket.AppThemes.Components;
using RocketDirectoryAPI.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Xml;

namespace RocketDirectoryAPI.API
{    
    public partial class StartConnect
    {
        private ArticleLimpet GetActiveArticle(int articleid, bool useCache = true)
        {
            return RocketDirectoryAPIUtils.GetArticleData(_dataObject.PortalContent.PortalId, articleid, _sessionParams.CultureCodeEdit, _dataObject.SystemKey, useCache);
        }

        public int SaveArticle()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            _dataObject.Settings.Add("saved", "true");
            var articleData = RocketDirectoryAPIUtils.GetArticleData(_dataObject.PortalContent.PortalId, articleId, _sessionParams.CultureCodeEdit, _dataObject.SystemKey);
            articleData.ModuleId = _dataObject.PortalContent.SearchModuleId ; // moduleid used as changed flag.
            var rtn = articleData.Save(_postInfo);

            // Check other langauges for data.
            var objCtrl = new DNNrocketController();
            var updLang = false;
            foreach (var l in DNNrocketUtils.GetCultureCodeList())
            {
                if (l != articleData.CultureCode)
                {
                    var sqlCmd = "SELECT [ItemId],[PortalId],[ModuleId],[TypeCode],[XMLData],[GUIDKey],[ModifiedDate],[TextData],[XrefItemId],[ParentItemId],[Lang],[UserId],[SortOrder] FROM {databaseOwner}[{objectQualifier}RocketDirectoryAPI] where ParentItemId = " + articleData.ArticleId + " and  TypeCode = '" + articleData.Info.TypeCode + "LANG' and lang = '" + l + "'";
                    var sqlRtn = objCtrl.ExecSqlList(sqlCmd);
                    if (sqlRtn.Count == 0)
                    {
                        var sRec = objCtrl.GetRecordLang(articleData.ArticleId, articleData.CultureCode, "RocketDirectoryAPI");
                        if (sRec != null)
                        {
                            sRec.ItemID = -1;
                            sRec.Lang = l;
                            objCtrl.Update(sRec, "RocketDirectoryAPI");
                            updLang = true;
                        }
                    }
                }
            }
            if (updLang) articleData.Update(); // so we rebuild IDX record.

            // clear cache
            CacheUtils.ClearAllCache(_dataObject.SystemKey + _dataObject.PortalId);

            DNNrocketUtils.SynchronizeModule(_dataObject.PortalContent.SearchModuleId); // module search

            return rtn;
        }
        public void DeleteArticle()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            var articleData = GetActiveArticle(articleId);
            var queryString = "articleid=" + articleData.ArticleId;
            DNNrocketUtils.DeleteSearchDocument(_dataObject.PortalId, queryString);
            articleData.ClearCache();
            articleData.Delete();
            CacheUtils.ClearAllCache(_dataObject.SystemKey + _dataObject.PortalId);
            _sessionParams.Set("articleid", "0"); // so we return the list
        }
        public int CopyArticle()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            var articleData = GetActiveArticle(articleId);
            var newarticleId = articleData.Copy();
            // create all languages
            var l = DNNrocketUtils.GetCultureCodeList();
            foreach (var c in l)
            {
                articleData = RocketDirectoryAPIUtils.GetArticleData(_dataObject.PortalContent.PortalId, articleId, c, _dataObject.SystemKey);
                var newarticleData = RocketDirectoryAPIUtils.GetArticleData(_dataObject.PortalContent.PortalId, newarticleId, c, _dataObject.SystemKey);
                newarticleData.Info.XMLData = articleData.Info.XMLData;
                newarticleData.Name += " - " + LocalUtils.ResourceKey("RC.copy", "Text", c);
                newarticleData.Update();
            }
            // add categories
            articleData = GetActiveArticle(articleId);
            var newarticleData2 = RocketDirectoryAPIUtils.GetArticleData(_dataObject.PortalContent.PortalId, newarticleId, _sessionParams.CultureCodeEdit, _dataObject.SystemKey);
            foreach (var c in articleData.GetCategories())
            {
                newarticleData2.AddCategory(c.CategoryId);
            }
            // add properties
            foreach (var p in articleData.GetProperties())
            {
                newarticleData2.AddProperty(p.PropertyId);
            }

            // copy images
            var imgFolder = Path.Combine(_dataObject.PortalContent.ImageFolderMapPath, newarticleData2.ArticleId.ToString());
            if (!Directory.Exists(imgFolder)) Directory.CreateDirectory(imgFolder);
            newarticleData2.Info.RemoveList("imagelist");
            foreach (var img in articleData.GetImages())
            {
                var uniqueName = Path.GetFileName(img.MapPath);
                var newImgMapPath = Path.Combine(imgFolder, uniqueName);
                try
                {
                    File.Copy(img.MapPath, newImgMapPath);
                    newarticleData2.AddImage(uniqueName);
                }
                catch (Exception)
                {
                    // ignore
                }
            }
            // copy documents
            var docFolder = Path.Combine(_dataObject.PortalContent.DocFolderMapPath, newarticleData2.ArticleId.ToString());
            if (!Directory.Exists(docFolder)) Directory.CreateDirectory(docFolder);
            newarticleData2.Info.RemoveList(articleData.DocumentListName);
            foreach (var doc in articleData.GetDocs())
            {
                var uniqueName = Path.GetFileName(doc.MapPath);
                var newDocMapPath = Path.Combine(docFolder, uniqueName);
                try
                {
                    File.Copy(doc.MapPath, newDocMapPath);
                    newarticleData2.AddDoc(doc.Name, uniqueName);
                }
                catch (Exception)
                {
                    // ignore
                }
            }

            newarticleData2.ClearCache();
            return newarticleData2.ArticleId;
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
                var imgList = RocketUtils.ImgUtils.UploadBase64Image(filenameList, filebase64List, baseFileMapPath, destDir, imgsize);
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
        public string AddArticleChatGptImageAsync(bool singleImage = false)
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            var articleData = GetActiveArticle(articleId);
            if (articleData.Exists)
            {
                if (singleImage) articleData.Info.RemoveList(articleData.ImageListName);

                //call ChatGptimage
                var prompt = GeneralUtils.DeCode(_postInfo.GetXmlProperty("genxml/hidden/chatgptimagetext"));
                if (!String.IsNullOrEmpty(prompt))
                {
                    try
                    {
                        _dataObject.PortalData.AiImageCount();
                        var chatGpt = new ChatGPT();
                        var iUrl = chatGpt.GenerateImageAsync(prompt).Result;
                        if (GeneralUtils.IsUriValid(iUrl))
                        {
                            var imgFolder = _dataObject.PortalContent.ImageFolderMapPath + "\\" + articleData.ArticleId;
                            if (!Directory.Exists(imgFolder)) Directory.CreateDirectory(imgFolder);
                            var imgFileMapPath = imgFolder + "\\" + GeneralUtils.GetGuidKey() + ".webp";
                            ImgUtils.DownloadAndSaveImage(iUrl, imgFileMapPath);

                            articleData.AddImage(Path.GetFileName(imgFileMapPath));
                            _dataObject.SetDataObject("articledata", articleData);
                        }
                        else
                        {
                            return iUrl;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogUtils.LogException(ex);
                        return ex.ToString();
                    }
                }
            }
            return GetArticle(articleData);
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

                    var destDir = _dataObject.PortalContent.ImageFolderMapPath + "\\" + articleData.ArticleId;
                    if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);
                    var imgList = RocketUtils.ImgUtils.MoveImageToFolder(UserUtils.GetCurrentUserId(), sInfo, destDir, PortalUtils.TempDirectoryMapPath());
                    foreach (var imgFileMapPath in imgList)
                    {
                        articleData.AddImage(Path.GetFileName(imgFileMapPath));
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
                var destDir = _dataObject.PortalContent.DocFolderMapPath + "\\" + articleData.ArticleId;
                if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);
                var filenameList = fileuploadlist.Split('*');
                var filebase64List = fileuploadbase64.Split('*');
                Dictionary<string,string> fileList;
                if (_dataObject.PortalContent.SecureUpload)
                    fileList = DocUtils.UploadSecureBase64file(filenameList, filebase64List, destDir);
                else
                    fileList = DocUtils.UploadBase64fileDict(filenameList, filebase64List, destDir);
                foreach (var imgFileMapPath in fileList)
                {
                    articleData.AddDoc(imgFileMapPath.Value, imgFileMapPath.Key);
                }
                _dataObject.SetDataObject("articledata", articleData);
            }

            var razorTempl = GetSystemTemplate("ArticleDocuments.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, articleData, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            if (pr.ErrorMsg != "") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public string AddArticleLink()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            if (articleId > 0)
            {
                var articleData = GetActiveArticle(articleId);
                articleData.Save(_postInfo);
                articleData.AddLink();
                _dataObject.SetDataObject("articledata", articleData);
                var razorTempl = GetSystemTemplate("ArticleLinks.cshtml");
                var pr = RenderRazorUtils.RazorProcessData(razorTempl, articleData, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
                if (pr.ErrorMsg != "") return pr.ErrorMsg;
                return pr.RenderedText;
            }
            return "ERROR: Invalid ItemId";
        }
        public string GenerateLinkImage()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            if (articleId > 0)
            {
                var linkidx = _paramInfo.GetXmlPropertyInt("genxml/hidden/linkidx");
                if (linkidx == 0) return "ERROR: Invalid listidx";

                var articleData = GetActiveArticle(articleId);
                articleData.Save(_postInfo);
                
                var linkData = articleData.Getlink(linkidx - 1);
                var requestData = new SimplisityRecord();
                requestData.SetXmlProperty("genxml/request/websiteurl", linkData.Url);
                if (linkData.Url.ToLower().EndsWith(".pdf"))
                {
                    requestData.SetXmlProperty("genxml/request/width", "816");
                    requestData.SetXmlProperty("genxml/request/height", "1056");
                }
                else
                {
                    requestData.SetXmlProperty("genxml/request/width", "1920");
                    requestData.SetXmlProperty("genxml/request/height", "1200");
                }

                var rtnXML = RocketDirectoryAPIUtils.SendServerRequest("WebsiteUrlToImage", requestData);
                if (rtnXML != "")
                {
                    var sRec = new SimplisityRecord();
                    sRec.FromXmlItem(rtnXML);
                    // Get image and add to Article.
                    var imageMapPath = sRec.GetXmlProperty("genxml/responce/imagemappath");
                    if (File.Exists(imageMapPath))
                    {
                        var uniqueName = Path.GetFileName(imageMapPath);
                        var imgNewMapPath = _dataObject.PortalContent.ImageFolderMapPath.TrimEnd('\\') + "\\" + articleData.ArticleId + "\\" + uniqueName;
                        if (!File.Exists(imgNewMapPath))
                        {
                            var imgFolder = Path.GetDirectoryName(imgNewMapPath);
                            if (!Directory.Exists(imgFolder)) Directory.CreateDirectory(imgFolder);
                            articleData.AddImage(uniqueName);
                            File.Move(imageMapPath, imgNewMapPath);
                        }
                    }

                }
                return GetArticle(articleData);
            }
            return "ERROR: Invalid ItemId";
        }
        public string GeneratePdfImage()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            if (articleId > 0)
            {
                var docidx = _paramInfo.GetXmlPropertyInt("genxml/hidden/docidx");
                if (docidx == 0) return "ERROR: Invalid listidx";

                var articleData = GetActiveArticle(articleId);
                articleData.Save(_postInfo);

                var docData = articleData.GetDoc(docidx - 1);
                var requestData = new SimplisityRecord();
                requestData.SetXmlProperty("genxml/request/pdfmappath", docData.MapPath);
                requestData.SetXmlProperty("genxml/request/width", "816");
                requestData.SetXmlProperty("genxml/request/height", "1056");

                if (docData.Extension.ToLower() == ".pdf")
                {
                    var rtnXML = RocketDirectoryAPIUtils.SendServerRequest("PdfToImage", requestData);
                    if (rtnXML != "")
                    {
                        var sRec = new SimplisityRecord();
                        sRec.FromXmlItem(rtnXML);
                        // Get image and add to Article.
                        var imageMapPath = sRec.GetXmlProperty("genxml/responce/imagemappath");
                        if (File.Exists(imageMapPath))
                        {
                            var uniqueName = Path.GetFileName(imageMapPath);
                            var imgNewMapPath = _dataObject.PortalContent.ImageFolderMapPath.TrimEnd('\\') + "\\" + articleData.ArticleId + "\\" + uniqueName;
                            if (!File.Exists(imgNewMapPath))
                            {
                                var imgFolder = Path.GetDirectoryName(imgNewMapPath);
                                if (!Directory.Exists(imgFolder)) Directory.CreateDirectory(imgFolder);
                                articleData.AddImage(uniqueName);
                                File.Move(imageMapPath, imgNewMapPath);
                            }
                        }

                    }
                }
                return GetArticle(articleData);
            }
            return "ERROR: Invalid ItemId";
        }
        public string AddArticleReview()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            if (articleId > 0)
            {
                var articleData = GetActiveArticle(articleId);
                articleData.Save(_postInfo);
                articleData.AddReview();
                _dataObject.SetDataObject("articledata", articleData);
                var razorTempl = GetSystemTemplate("ArticleReviews.cshtml");
                var pr = RenderRazorUtils.RazorProcessData(razorTempl, articleData, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
                if (pr.ErrorMsg != "") return pr.ErrorMsg;
                return pr.RenderedText;
            }
            return "ERROR: Invalid ItemId";
        }
        public string AddArticleModel()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            if (articleId > 0)
            {
                var articleData = GetActiveArticle(articleId);
                articleData.Save(_postInfo);
                articleData.AddModel();
                _dataObject.SetDataObject("articledata", articleData);
                var razorTempl = GetSystemTemplate("ArticleModelsList.cshtml");
                var pr = RenderRazorUtils.RazorProcessData(razorTempl, articleData, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
                if (pr.ErrorMsg != "") return pr.ErrorMsg;
                return pr.RenderedText;
            }
            return "ERROR: Invalid ItemId";
        }
        public string AddUserReview()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            if (articleId > 0)
            {
                if (UserUtils.IsValidUser(_dataObject.PortalId, UserUtils.GetCurrentUserId()))
                {
                    var articleData = GetActiveArticle(articleId);
                    articleData.AddReview(_postInfo);
                    _dataObject.SetDataObject("articledata", articleData);
                    return ""; // reload page from simplisity.
                }
                LogUtils.LogSystem("SECURITY: Invalid user trying to add review.");
                return "";
            }
            return "ERROR: Invalid AddUserReview()";
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

            var razorTempl = _dataObject.AppTheme.GetTemplate("admindetail.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, articleData, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            if (pr.ErrorMsg != "") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public String GetArticleCategoryList(ArticleLimpet articleData)
        {
            var razorTempl = GetSystemTemplate("ArticleCategoryList.cshtml");
            _dataObject.SetDataObject("articledata", articleData);
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, null, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            if (pr.ErrorMsg != "") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public String GetArticlePropertyList(ArticleLimpet articleData)
        {
            var razorTempl = GetSystemTemplate("ArticlePropertyList.cshtml");
            _dataObject.SetDataObject("articledata", articleData);
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, articleData, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            if (pr.ErrorMsg != "") return pr.ErrorMsg;
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
                if (_dataObject.AppTheme.AppThemeFolder == "") return "No AppTheme Defined.  Check RocketDirectoryAPI Admin Portal Settings.";
                var articleDataList = new ArticleLimpetList(_sessionParams, _dataObject.PortalContent, _sessionParams.CultureCodeEdit, true, true, _sessionParams.GetInt("defaultcategory"));
                _dataObject.SetDataObject("articlelist", articleDataList);
                var razorTempl = _dataObject.AppTheme.GetTemplate("adminlist.cshtml");
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
        private Dictionary<string, object> DownloadArticleFile()
        {
            var rtnDic = new Dictionary<string, object>();
            var strId = GeneralUtils.DeCode(_paramInfo.GetXmlProperty("genxml/urlparams/articleid"));
            var articleId = 0;
            if (GeneralUtils.IsNumeric(strId) && (UserUtils.IsAuthorised() || !_dataObject.PortalContent.SecureUpload))
            {
                articleId = Convert.ToInt32(strId);
                var articleData = GetActiveArticle(articleId);
                articleData.ClearCache(); // reset data to clear cached name.
                articleData = GetActiveArticle(articleId);

                var dockey = GeneralUtils.DeCode(_paramInfo.GetXmlProperty("genxml/urlparams/dockey"));
                var articleDoc = articleData.GetDoc(dockey);
                rtnDic.Add("filenamepath", DNNrocketUtils.MapPath(articleDoc.RelPath));
                rtnDic.Add("downloadname", articleDoc.DownloadName);
            }
            return rtnDic;
        }
        public string AssignArticleProperty()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            var propertyId = _paramInfo.GetXmlPropertyInt("genxml/hidden/propertyid");
            var articleData = GetActiveArticle(articleId);
            articleData.AddProperty(propertyId);
            articleData.ClearCache();
            articleData = GetActiveArticle(articleId); // reload
            return GetArticlePropertyList(articleData);
        }
        public string AssignArticlePropertyCheckBox()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            var articleData = GetActiveArticle(articleId);
            var nodList = _postInfo.XMLDoc.SelectNodes("genxml/ignore/*");
            if (nodList != null)
            {
                foreach (XmlNode nod in nodList)
                {
                    if (nod.InnerText.ToLower() == "true")
                    {
                        var propId = nod.Name.Replace("property", "");
                        if (GeneralUtils.IsNumeric(propId)) articleData.AddProperty(Convert.ToInt32(propId));
                    }
                }
            }
            articleData.ClearCache();
            articleData = GetActiveArticle(articleId); // reload
            return GetArticlePropertyList(articleData);
        }
        public string RemoveArticleProperty()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            var propertyId = _paramInfo.GetXmlPropertyInt("genxml/hidden/propertyid");
            var articleData = GetActiveArticle(articleId);
            articleData.RemoveProperty(propertyId);
            articleData.ClearCache();
            articleData = GetActiveArticle(articleId); // reload
            return GetArticlePropertyList(articleData);
        }
        public string AssignArticleCategory()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            var categoryId = _paramInfo.GetXmlPropertyInt("genxml/hidden/categoryid");
            var articleData = GetActiveArticle(articleId);
            articleData.AddCategory(categoryId);
            articleData.ClearCache();
            articleData = GetActiveArticle(articleId); // reload
            return GetArticleCategoryList(articleData);
        }
        public string AssignArticleCategoryCheckBox()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            var articleData = GetActiveArticle(articleId);
            var nodList = _postInfo.XMLDoc.SelectNodes("genxml/ignore/*");
            if (nodList != null)
            {
                foreach (XmlNode nod in nodList)
                {
                    if (nod.InnerText.ToLower() == "true")
                    {
                        var catId = nod.Name.Replace("category", "");
                        if (GeneralUtils.IsNumeric(catId)) articleData.AddCategory(Convert.ToInt32(catId), _postInfo.GetXmlPropertyBool("genxml/settings/cascade"));
                    }
                }
            }
            articleData.ClearCache();
            articleData = GetActiveArticle(articleId); // reload
            return GetArticleCategoryList(articleData);
        }
        public string AssignDefaultArticleCategory()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            var categoryId = _paramInfo.GetXmlPropertyInt("genxml/hidden/categoryid");
            var articleData = GetActiveArticle(articleId);
            articleData.SetDefaultCategory(categoryId);
            articleData.ClearCache();
            articleData = GetActiveArticle(articleId); // reload
            return GetArticleCategoryList(articleData);
        }
        public string RemoveArticleCategory()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            var categoryId = _paramInfo.GetXmlPropertyInt("genxml/hidden/categoryid");
            var articleData = GetActiveArticle(articleId);
            articleData.RemoveCategory(categoryId);
            articleData.ClearCache();
            articleData = GetActiveArticle(articleId); // reload
            _dataObject.Reload();
            return GetArticleCategoryList(articleData);
        }
        private string GetPublicView(string template)
        {
            var razorTempl = _dataObject.AppTheme.GetTemplate(template);
            if (razorTempl == "") return "";
            var articleid = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            if (articleid == 0) articleid = _paramInfo.GetXmlPropertyInt("genxml/urlparams/articleid");
            if (articleid == 0) articleid = _paramInfo.GetXmlPropertyInt("genxml/remote/urlparams/articleid");
            if (articleid > 0)
            {
                var articleData = RocketDirectoryAPIUtils.GetArticleData(_dataObject.PortalContent.PortalId, articleid, _sessionParams.CultureCode, _dataObject.SystemKey);
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
                var articleData = RocketDirectoryAPIUtils.GetArticleData(_dataObject.PortalContent.PortalId, articleid, _sessionParams.CultureCode, _dataObject.SystemKey);
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
            var razorTempl = _dataObject.AppTheme.GetTemplate(template);
            if (razorTempl == "") return "No Razor Template.  Check engine server. Theme: '" + _dataObject.AppTheme.AppThemeFolder;
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, null, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, _dataObject.PortalContent.DebugMode);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public String ArticleSearchPreview()
        {
            var searchmodel = new SearchPostModel();
            searchmodel.SearchInput = _sessionParams.Get("searchtext");
            if (searchmodel.SearchInput == "") return "";
            searchmodel.PageIndex = 1;
            searchmodel.PageSize = 200;

            var template = _paramInfo.GetXmlProperty("genxml/hidden/template");
            if (template == "") template = "SearchPreview.cshtml";
            var searchResults = SearchUtils.DoSearchRecord(_dataObject.PortalId, searchmodel);
            _dataObject.SetDataObject("previewsearch", searchResults);
            return RocketDirectoryAPIUtils.DisplayView(_dataObject, template);
        }
        public String GetPublicArticleList()
        {
            var template = _paramInfo.GetXmlProperty("genxml/hidden/template");
            return RocketDirectoryAPIUtils.DisplayView(_dataObject, template);
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

