using DNNrocketAPI;
using Simplisity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using DNNrocketAPI.Components;
using System.Globalization;
using System.Text.RegularExpressions;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Xml.Linq;
using System.Reflection;

namespace RocketDirectoryAPI.Components
{
    public class ArticleLimpet
    {
        public const string _tableName = "RocketDirectoryAPI";
        public const string _entityTypeCodeAppendix = "ART";
        private DNNrocketController _objCtrl;
        private int _articleId;
        private List<SimplisityInfo> _propXrefList;
        private List<int> _propXrefListId;
        private List<string> _propXrefListRef;
        private List<SimplisityInfo> _catXrefList;
        private List<int> _catXrefListId;
        private List<string> _catXrefListRef;
        private string _entityTypeKey;

        /// <summary>
        /// Get ArticleLimpet data. (use RocketDirectoryAPIUtils.GetArticleData() to implement cache)
        /// </summary>
        /// <param name="portalId">portalId</param>
        /// <param name="articleId">-1 creates new record</param>
        /// <param name="cultureCode"></param>
        /// <param name="cultureCode"></param>
        public ArticleLimpet(int portalId, int articleId, string cultureCode, string systemKey)
        {
            _objCtrl = new DNNrocketController();
            _entityTypeKey = systemKey + _entityTypeCodeAppendix;
            if (articleId <= 0) articleId = -1;  // create new record.
            _articleId = articleId;
            Info = new SimplisityInfo();
            Info.ItemID = articleId;
            Info.TypeCode = _entityTypeKey;
            Info.ModuleId = -1;
            Info.UserId = -1;
            Info.PortalId = portalId;
            Populate(cultureCode, systemKey);
        }
        /// <summary>
        ///  Use to populate without accessing DB.
        /// </summary>
        /// <param name="sInfo"></param>
        public ArticleLimpet(SimplisityInfo sInfo)
        {
            _objCtrl = new DNNrocketController();
            Info = sInfo;
            CultureCode = Info.Lang;
            PortalCatalog = new PortalCatalogLimpet(PortalId, CultureCode, SystemKey);
        }
        private void Populate(string cultureCode, string systemKey)
        {
            SystemKey = systemKey;
            CultureCode = cultureCode;
            if (CultureCode == "") CultureCode = DNNrocketUtils.GetEditCulture();
            if (_articleId > 0)
            {
                var info = _objCtrl.GetInfo(_articleId, CultureCode, _tableName); // get existing record.                    
                if (Info != null && Info.ItemID > 0 && Info.TypeCode == _entityTypeKey) // ensure we have the same systemKey for detail view.
                    Info = info;
                else
                    Info.ItemID = 0; // article is a differet system, flag it as not existing.
            }
            Info.Lang = CultureCode;
            PortalCatalog = new PortalCatalogLimpet(PortalId, CultureCode, systemKey);
            SystemKey = systemKey; // after populate, so unassigned values in XML are correct. (use to identify article system for meta.ascx)
        }
        public void PopulateLists()
        {
            _propXrefList = _objCtrl.GetList(PortalId, -1, "PROPXREF", " and R1.[ParentItemId] = " + ArticleId + " ", "", "", 0, 0, 0, 0, _tableName);
            _propXrefListId = new List<int>();
            _propXrefListRef = new List<string>();
            foreach (var p in _propXrefList)
            {
                var c = new PropertyLimpet(PortalId, p.XrefItemId, CultureCode, SystemKey);
                _propXrefListId.Add(p.XrefItemId);
                _propXrefListRef.Add(c.Ref);
            }

            _catXrefList = _objCtrl.GetList(PortalId, -1, "CATXREF", " and R1.[ParentItemId] = " + ArticleId + " ", "", "", 0, 0, 0, 0, _tableName);
            _catXrefListId = new List<int>();
            _catXrefListRef = new List<string>();
            foreach (var p in _catXrefList)
            {
                var c = new CategoryLimpet(PortalId, p.XrefItemId, CultureCode, SystemKey);
                if (c.Exists)
                {
                    _catXrefListId.Add(p.XrefItemId);
                    _catXrefListRef.Add(c.Ref);
                }
                else
                    RemoveCategory(c.CategoryId);
            }
        }
        public void Delete()
        {
            _objCtrl.Delete(Info.ItemID, _tableName);
        }

        private void ReplaceInfoFields(SimplisityInfo postInfo, string xpathListSelect)
        {
            var textList = Info.XMLDoc.SelectNodes(xpathListSelect);
            if (textList != null)
            {
                foreach (XmlNode nod in textList)
                {
                    Info.RemoveXmlNode(xpathListSelect.Replace("*","") + nod.Name);
                }
            }
            textList = postInfo.XMLDoc.SelectNodes(xpathListSelect);
            if (textList != null)
            {
                foreach (XmlNode nod in textList)
                {
                    Info.SetXmlProperty(xpathListSelect.Replace("*", "") + nod.Name, nod.InnerText);
                }
            }
        }
        public int Save(SimplisityInfo postInfo)
        {
            ReplaceInfoFields(postInfo, "genxml/textbox/*");
            ReplaceInfoFields(postInfo, "genxml/lang/genxml/textbox/*");
            ReplaceInfoFields(postInfo, "genxml/money/*");
            ReplaceInfoFields(postInfo, "genxml/lang/genxml/money/*");
            ReplaceInfoFields(postInfo, "genxml/checkbox/*");
            ReplaceInfoFields(postInfo, "genxml/lang/genxml/checkbox/*");
            ReplaceInfoFields(postInfo, "genxml/select/*");
            ReplaceInfoFields(postInfo, "genxml/lang/genxml/select/*");
            ReplaceInfoFields(postInfo, "genxml/radio/*");
            ReplaceInfoFields(postInfo, "genxml/lang/genxml/radio/*");
            ReplaceInfoFields(postInfo, "genxml/data/*");
            ReplaceInfoFields(postInfo, "genxml/lang/genxml/data/*");

            var postLists = postInfo.GetLists();
            foreach (var listname in postLists)
            {
                if (listname != "modellist" && listname != "imagelist" && listname != "documentlist" && listname != "linklist" && listname != "reviewlist")
                {
                    var listData = postInfo.GetList(listname);
                    foreach (var listItem in listData)
                    {
                        Info.AddListItem(listname, listItem);
                    }
                    Info.SetXmlProperty("genxml/" + listname + "/@list", "true");
                    Info.SetXmlProperty("genxml/lang/genxml/" + listname + "/@list", "true");
                }
            }
            // Fix date for admin edit for adding reviews
            var lp = 1;
            foreach (var r in postInfo.GetList("reviewlist"))
            {
                if (r.GetXmlPropertyDate("genxml/textbox/reviewdate") < DateTime.Now.AddYears(-100))
                {
                    postInfo.SetXmlProperty("genxml/reviewlist/genxml[" + lp + "]/textbox/reviewdate", DateTime.Now.ToString("O"), TypeCode.DateTime);
                }
                else
                    break;
                lp += 1;
            }

            UpdateImages(postInfo.GetList("imagelist"));
            UpdateDocs(postInfo.GetList("documentlist"));
            UpdateLinks(postInfo.GetList("linklist"));
            UpdateReview(postInfo.GetList("reviewlist"));
            UpdateModels(postInfo.GetList("modellist"));

            //------------------------------------------------------------------------------------------
            //NOTE: THE FORMAT FIX CAN ONLY BE DONE IN THE SAVE, IT FIXES THE INVALID HUMAN INPUT.
            // Fix incorrect money value.  All money should be kept in int.

            var lp3 = 1;
            var modelList = postInfo.GetList("modellist");
            foreach (var model in modelList)
            {
                Info.SetXmlPropertyInt("genxml/modellist/genxml[" + lp3 + "]/textbox/modelprice", PortalCatalog.CurrencyConvertCents(model.GetXmlProperty("genxml/textbox/modelprice")).ToString());
                lp3 += 1;
            }

            int pmax = 0;
            int pmin = 0;
            int psmax = 0;
            int psmin = 0;
            var modelList2 = Info.GetList("modellist");
            foreach (var m in modelList2)
            {
                var mPrice = m.GetXmlPropertyInt("genxml/textbox/modelprice");
                if (mPrice < pmin || pmin == 0) pmin = mPrice;
                if (mPrice > pmax || pmax == 0) pmax = mPrice;
            }
            if (psmin == 0) psmin = psmax; // if we have a sale price, the minimum should always be set.
            Info.SetXmlPropertyInt("genxml/priceminimum", pmin.ToString());
            Info.SetXmlPropertyInt("genxml/pricemaximum", pmax.ToString());
            var bestprice = pmin;
            if (psmin != 0) bestprice = psmin;
            Info.SetXmlPropertyInt("genxml/bestprice", bestprice.ToString());

            // FIX any prices saved as single fields.
            var nodList = postInfo.XMLDoc.SelectNodes("genxml/money/*");
            if (nodList != null)
            {
                foreach (XmlNode nod in nodList)
                {
                    Info.SetXmlPropertyInt("genxml/money/" + nod.Name, PortalCatalog.CurrencyConvertCents(postInfo.GetXmlProperty("genxml/money/" + nod.Name)).ToString());
                }
            }

            return ValidateAndUpdate();
        }
        public void ClearCache()
        {
            var groupId = SystemKey + PortalId;
            CacheUtils.ClearAllCache(groupId); // clear system cache, so lists and razor views are reloaded.
        }
        public int Update()
        {
            CalculateReviewCount();
            Info = _objCtrl.SaveData(Info, _tableName);
            if (Info.GUIDKey == "")
            {
                // get unique ref
                Info.GUIDKey = GeneralUtils.GetGuidKey();
                Info = _objCtrl.SaveData(Info, _tableName);
            }
            _objCtrl.RebuildIndex(PortalId, Info.ItemID, SystemKey, _tableName);
            ClearCache();
            return Info.ItemID;
        }
        public int ValidateAndUpdate()
        {
            Validate();
            return Update();
        }
        public int Copy()
        {
            Info.ItemID = -1;
            Info.GUIDKey = GeneralUtils.GetGuidKey();
            ClearCache();
            var i = Update();
            return i;
        }

        public void AddListItem(string listname)
        {
            if (Info.ItemID < 0) Update(); // blank record, not on DB.  Create now.
            Info.AddListItem(listname);         
            Update();
        }
        public void Validate()
        {
        }

        #region "images"

        public string ImageListName { get { return "imagelist";  } }
        public void UpdateImages(List<SimplisityInfo> imageList)
        {
            Info.RemoveList(ImageListName);
            foreach (var sInfo in imageList)
            {
                var imgData = new ArticleImage(sInfo, "articleimage");
                UpdateImage(imgData);
            }
        }
        public List<SimplisityInfo> GetImageList()
        {
            return Info.GetList(ImageListName);
        }
        public ArticleImage AddImage(string uniqueName)
        {
            var articleImage = new ArticleImage(new SimplisityInfo(), "articleimage");
            if (GetImageList().Count < PortalCatalog.ArticleImageLimit)
            {
                if (Info.ItemID < 0) Update(); // blank record, not on DB.  Create now.
                articleImage.RelPath = PortalCatalog.ImageFolderRel.TrimEnd('/') + "/" + ArticleId + "/" + Path.GetFileName(uniqueName);
                Info.AddListItem(ImageListName, articleImage.Info);
                Update();
            }
            return articleImage;
        }
        public void UpdateImage(ArticleImage articleImage)
        {
            Info.RemoveListItem(ImageListName, "genxml/hidden/imagekey", articleImage.ImageKey);
            Info.AddListItem(ImageListName, articleImage.Info);
        }
        public ArticleImage GetImage(int idx)
        {
            return new ArticleImage(Info.GetListItem(ImageListName, idx), "articleimage");
        }
        public List<ArticleImage> GetImages()
        {
            var rtn = new List<ArticleImage>();
            foreach (var i in Info.GetList(ImageListName))
            {
                rtn.Add(new ArticleImage(i, "articleimage"));
            }
            return rtn;
        }
        #endregion

        #region "docs"
        public string DocumentListName { get { return "documentlist"; } }
        public void UpdateDocs(List<SimplisityInfo> docList)
        {
            Info.RemoveList(DocumentListName);
            foreach (var sInfo in docList)
            {
                var docData = new ArticleDoc(sInfo, "articledoc");
                UpdateDoc(docData);
            }
        }
        public List<SimplisityInfo> GetDocList()
        {
            return Info.GetList(DocumentListName);
        }
        public ArticleDoc AddDoc(string friendlyName, string uniqueName)
        {
            var articleDoc = new ArticleDoc(new SimplisityInfo(), "articledoc");
            if (GetDocList().Count < PortalCatalog.ArticleDocumentLimit)
            {
                if (Info.ItemID < 0) Update(); // blank record, not on DB.  Create now.
                articleDoc.RelPath = PortalCatalog.DocFolderRel.TrimEnd('/') + "/" + Path.GetFileName(uniqueName);
                articleDoc.FileName = friendlyName;
                articleDoc.Name = friendlyName;
                articleDoc.Extension = Path.GetExtension(friendlyName);                
                Info.AddListItem(DocumentListName, articleDoc.Info);
                Update();
            }
            return articleDoc;
        }
        public void UpdateDoc(ArticleDoc articleDoc)
        {
            Info.RemoveListItem(DocumentListName, "genxml/hidden/dockey", articleDoc.DocKey);
            Info.AddListItem(DocumentListName, articleDoc.Info);
        }
        public ArticleDoc GetDoc(int idx)
        {
            return new ArticleDoc(Info.GetListItem(DocumentListName, idx), "articledoc");
        }
        public ArticleDoc GetDoc(string docKey)
        {
            return new ArticleDoc(Info.GetListItem(DocumentListName, "genxml/hidden/dockey", docKey), "articledoc");
        }
        public List<ArticleDoc> GetDocs()
        {
            var rtn = new List<ArticleDoc>();
            foreach (var i in Info.GetList(DocumentListName))
            {
                rtn.Add(new ArticleDoc(i, "articledoc"));
            }
            return rtn;
        }
        #endregion

        #region "links"
        public string LinkListName { get { return "linklist"; } }
        public void UpdateLinks(List<SimplisityInfo> linkList)
        {
            Info.RemoveList(LinkListName);
            foreach (var sInfo in linkList)
            {
                var linkData = new ArticleLink(sInfo, "articlelink");
                UpdateLink(linkData);
            }
        }
        public List<SimplisityInfo> GetLinkList()
        {
            return Info.GetList(LinkListName);
        }
        public ArticleLink AddLink()
        {
            var articleLink = new ArticleLink(new SimplisityInfo(), "articlelink");
            if (Info.ItemID < 0) Update(); // blank record, not on DB.  Create now.
            Info.AddListItem(LinkListName, articleLink.Info);
            Update();
            return articleLink;
        }
        public void UpdateLink(ArticleLink articleLink)
        {
            Info.RemoveListItem(LinkListName, "genxml/hidden/linkkey", articleLink.LinkKey);
            Info.AddListItem(LinkListName, articleLink.Info);
        }
        public ArticleLink Getlink(int idx)
        {
            return new ArticleLink(Info.GetListItem(LinkListName, idx), "articlelink");
        }
        public List<ArticleLink> Getlinks()
        {
            var rtn = new List<ArticleLink>();
            foreach (var i in Info.GetList(LinkListName))
            {
                rtn.Add(new ArticleLink(i, "articlelink"));
            }
            return rtn;
        }
        #endregion

        #region "reviews"
        public string ReviewListName { get { return "reviewlist"; } }
        public void UpdateReview(List<SimplisityInfo> reviewList)
        {
            Info.RemoveList(ReviewListName);
            reviewList = reviewList.OrderBy(element => element.GetXmlPropertyDate("genxml/textbox/reviewdate")).ToList();          
            foreach (var sInfo in reviewList)
            { 
                Info.AddListItem(ReviewListName, sInfo);
            }
        }
        public List<SimplisityInfo> GetReviewList()
        {
            return Info.GetList(ReviewListName);
        }
        public SimplisityInfo AddReview()
        {
            var articleReview = new ArticleReview(new SimplisityInfo(), "articlereview");
            if (Info.ItemID < 0) Update(); // blank record, not on DB.  Create now.
            Info.AddListItem(ReviewListName, articleReview.Info);
            UpdateReview(Info.GetList("reviewlist")); // sort by date, so new is at top of list.
            Update();
            return articleReview.Info;
        }
        public SimplisityInfo AddReview(SimplisityInfo postInfo)
        {
            var articleReview = new ArticleReview(postInfo, "articlereview");
            if (Info.ItemID > 0 && (!String.IsNullOrEmpty(articleReview.Comment) || articleReview.Rating > 0))
            {
                if (!SecurityInput.CheckForSQLInjection(articleReview.Name) && !SecurityInput.CheckForSQLInjection(articleReview.Comment))
                {
                    var comment = SecurityInput.PreventSQLInjection(articleReview.Comment);
                    comment = SecurityInput.NoProfanity(comment);
                    comment = SecurityInput.RemoveScripts(comment);
                    var name = SecurityInput.PreventSQLInjection(articleReview.Name);
                    name = SecurityInput.NoProfanity(name);
                    name = SecurityInput.RemoveScripts(name);

                    articleReview.Info.XMLData = postInfo.XMLData;
                    articleReview.UserId = UserUtils.GetCurrentUserId();
                    articleReview.Name = name;
                    articleReview.Comment = comment;
                    articleReview.ReviewDate = DateTime.Now;
                    Info.AddListItem(ReviewListName, articleReview.Info);
                    Update();
                }

            }
            return articleReview.Info;
        }
        public ArticleReview GetReview(int idx)
        {
            return new ArticleReview(Info.GetListItem(ReviewListName, idx), "articlereview");
        }
        public List<ArticleReview> GetReviews()
        {
            var rtn = new List<ArticleReview>();
            foreach (var i in Info.GetList(ReviewListName))
            {
                rtn.Add(new ArticleReview(i, "articlereview"));
            }
            return rtn;
        }
        private void CalculateReviewCount()
        {
            // Get Review Counts
            var reviewList = GetReviews();
            ReviewCount = reviewList.Count();
            ReviewScore = 0;
            var scoreCount = 0;
            foreach (var r in reviewList)
            {
                if (r.Rating > 0)
                {
                    ReviewScore += r.Rating;
                    scoreCount += 1;
                }
            }
            if (scoreCount > 0 && ReviewScore > 0)
            {
                ReviewScore = ReviewScore / scoreCount;
            }
        }
        #endregion

        #region "category"
        public void AddCategory(int categoryId, bool cascade = true)
        {
            if (Info.ItemID < 0) Update(); // blank record, not on DB.  Create now.
            var catRecord = _objCtrl.GetRecord(categoryId, _tableName);
            if (catRecord != null)
            {
                var xrefGuidKey = GUIDKey + "-" + catRecord.GUIDKey;
                var catXrefRecord = _objCtrl.GetRecordByGuidKey(PortalId, -1, "CATXREF", xrefGuidKey, "", _tableName);
                if (catXrefRecord == null)
                {
                    var sRec = new SimplisityRecord();
                    sRec.ItemID = -1;
                    sRec.PortalId = PortalId;
                    sRec.ParentItemId = ArticleId;
                    sRec.XrefItemId = categoryId;
                    sRec.TypeCode = "CATXREF";
                    sRec.GUIDKey = xrefGuidKey;
                    if (_catXrefList == null) PopulateLists();
                    if (_catXrefList.Count == 0) sRec.ModuleId = 1; // use ModuleId for default.
                    _objCtrl.Update(sRec, _tableName);

                    if (cascade) AddCasCadeCategory(categoryId, 0);
                }
            }
        }
        private void AddCasCadeCategory(int categoryId, int levelCount)
        {
            var catRecord = _objCtrl.GetRecord(categoryId, _tableName);
            if (catRecord != null && catRecord.ParentItemId > 0 && levelCount < 50) // use levelCount to stop infinate loop for corrupt data
            {
                if (!IsInCategory(catRecord.ParentItemId))
                {
                    var catParentRecord = _objCtrl.GetRecord(catRecord.ParentItemId, _tableName);
                    var xrefGuidKey = GUIDKey + "-" + catParentRecord.GUIDKey;
                    var casCadeRec = _objCtrl.GetByGuidKey(PortalId, -1, "CATXREF", xrefGuidKey, "", _tableName);
                    if (casCadeRec == null)
                    {
                        var sRec = new SimplisityRecord();
                        sRec.ItemID = -1;
                        sRec.PortalId = PortalId;
                        sRec.ParentItemId = ArticleId;
                        sRec.XrefItemId = catParentRecord.ItemID;
                        sRec.TypeCode = "CATXREF";
                        sRec.GUIDKey = xrefGuidKey;
                        _objCtrl.Update(sRec, _tableName);
                    }
                }
                AddCasCadeCategory(catRecord.ParentItemId, levelCount + 1);
            }
        }

        public void RemoveCategory(int categoryId)
        {

            var filter = " and xrefitemid = " + categoryId + " and ParentItemid = " + ArticleId + " ";
            var l = _objCtrl.GetList(PortalId, -1, "CATXREF", filter, "", "", 0, 0, 0, 0, _tableName);
            foreach (var cx in l)
            {
                //RemoveCasCadeCategory(categoryId);
                _objCtrl.Delete(cx.ItemID, _tableName);
            }
            Update();
        }
        private void RemoveCasCadeCategory(int categoryId)
        {
            var catRecord = _objCtrl.GetRecord(categoryId, _tableName);
            if (catRecord != null && catRecord.ParentItemId > 0)
            {
                var catParentRecord = _objCtrl.GetRecord(catRecord.ParentItemId, _tableName);
                var xrefGuidKey = GUIDKey + "-" + catParentRecord.GUIDKey;
                var casCadeRec = _objCtrl.GetByGuidKey(PortalId, -1, "CATXREF", xrefGuidKey, "", _tableName);
                if (casCadeRec != null)
                {
                    _objCtrl.Delete(casCadeRec.ItemID, _tableName);
                }
                RemoveCasCadeCategory(catRecord.ParentItemId);
            }
        }
        public void UpdateCategorySortOrder(int categoryId, int sortOrder)
        {
            var l = _objCtrl.ExecSqlList("SELECT *  FROM {databaseOwner}[{objectQualifier}RocketDirectoryAPI] where typecode = 'CATXREF' and ParentItemId = " + ArticleId + " and XrefItemId = " + categoryId + " ");
            foreach (var catXrefRecord in l)
            {
                catXrefRecord.SortOrder = sortOrder;
                _objCtrl.Update(catXrefRecord, _tableName);
            }
        }
        public List<SimplisityInfo> GetCategoriesInfo()
        {
            if (_catXrefList == null) PopulateLists();
            return _catXrefList;
        }
        public List<CategoryLimpet> GetCategories()
        {
            var rtn = new List<CategoryLimpet>();
            if (_catXrefListId == null) PopulateLists();
            foreach (var categoryId in _catXrefListId)
            {
                var catData = new CategoryLimpet(PortalId, categoryId, CultureCode, SystemKey);
                if (catData.Exists)
                    rtn.Add(catData);
                else
                    RemoveCategory(categoryId);
            }
            return rtn;
        }
        public bool IsInCategory(int categoryId)
        {
            if (_catXrefListId == null) PopulateLists();
            if (_catXrefListId.Contains(categoryId)) return true;
            return false;
        }
        public bool IsInCategory(string propertyRef)
        {
            if (_catXrefListId == null) PopulateLists();
            if (_catXrefListRef.Contains(propertyRef)) return true;
            return false;
        }
        public int DefaultCategory()
        {
            // use ModuleId to flag default
            if (_catXrefList == null) PopulateLists();
            foreach (var catxrefInfo in _catXrefList)
            {
                if (catxrefInfo.ModuleId > 0) return catxrefInfo.XrefItemId;
            }
            return 0;
        }
        public void SetDefaultCategory(int categoryId)
        {
            // use ModuleId to flag default
            if (_catXrefList == null) PopulateLists();
            foreach (var catxrefInfo in _catXrefList)
            {
                if (catxrefInfo.XrefItemId == categoryId)
                    catxrefInfo.ModuleId = 1;
                else
                    catxrefInfo.ModuleId = 0;
                _objCtrl.Update(catxrefInfo, _tableName);
            }
        }
        #endregion

        #region "property"
        public void AddProperty(int propertyId)
        {
            if (Info.ItemID < 0) Update(); // blank record, not on DB.  Create now.
            var catRecord = _objCtrl.GetRecord(propertyId, _tableName);
            if (catRecord != null)
            {
                var key = catRecord.GUIDKey;
                if (key == "") key = GeneralUtils.GetUniqueString(2);
                var xrefGuidKey = GUIDKey + "-" + key;
                var catXrefRecord = _objCtrl.GetRecordByGuidKey(PortalId, -1, "PROPXREF", xrefGuidKey, "", _tableName);
                if (catXrefRecord == null)
                {
                    var sRec = new SimplisityRecord();
                    sRec.ItemID = -1;
                    sRec.ModuleId = -1;
                    sRec.PortalId = PortalId;
                    sRec.ParentItemId = ArticleId;
                    sRec.XrefItemId = propertyId;
                    sRec.TypeCode = "PROPXREF";
                    sRec.Lang = "";
                    sRec.XMLData = "<genxml></genxml>";
                    sRec.TextData = "";
                    sRec.GUIDKey = xrefGuidKey;
                    _objCtrl.Update(sRec, _tableName);
                }
            }
        }
        public void RemoveProperty(int propertyId)
        {
            var filter = " and xrefitemid = " + propertyId + " and ParentItemid = " + ArticleId + " ";
            var l = _objCtrl.GetList(PortalId, -1, "PROPXREF", filter, "", "", 0, 0, 0, 0, _tableName);
            foreach (var cx in l)
            {
                _objCtrl.Delete(cx.ItemID, _tableName);
            }
            Update();
        }
        public List<SimplisityInfo> GetPropertiesInfo()
        {
            if (_propXrefList == null) PopulateLists();
            return _propXrefList;
        }
        public List<PropertyLimpet> GetProperties(string groupRef = "")
        {
            if (_propXrefListId == null) PopulateLists();
            var rtn = new List<PropertyLimpet>();
            foreach (var propertyId in _propXrefListId)
            {
                var propertyData = new PropertyLimpet(PortalId, propertyId, CultureCode, SystemKey);
                if (groupRef == "" || propertyData.IsInGroup(groupRef))
                {
                    if (propertyData.Exists)
                        rtn.Add(propertyData);
                    else
                        RemoveProperty(propertyId);
                }
            }
            return rtn;
        }
        public List<ArticleLimpet> GetRelatedArticles(string groupRef = "", int numberOfArticles = 5)
        {
            var rtn = new List<ArticleLimpet>();
            var articleIdList = new List<int>();
            var properties = GetProperties(groupRef);
            
            foreach (var p in properties)
            {
                var sqlCmd = "select distinct ParentItemId from {databaseOwner}[{objectQualifier}RocketDirectoryAPI] where typecode = 'PROPXREF' and xrefitemid = " + p.PropertyId + " for xml raw";
                var sqlRtn = "<root><rows>" +  _objCtrl.GetSqlxml(sqlCmd) + "</rows></root>";
                var sRec = new SimplisityRecord();
                sRec.XMLData = sqlRtn;
                foreach (var r in sRec.GetRecordList("rows"))
                {
                    var articleId = r.GetXmlPropertyInt("row/@ParentItemId");
                    if (articleId > 0)
                    {
                        if (!articleIdList.Contains(articleId)) articleIdList.Add(articleId);
                    }
                }
            }
            Random random = new Random();
            int n = articleIdList.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                int value = articleIdList[k];
                articleIdList[k] = articleIdList[n];
                articleIdList[n] = value;
            }
            var lp = 1;
            foreach (var artId in articleIdList)
            {
                var articleData = new ArticleLimpet(PortalId, artId, CultureCode, SystemKey);
                if (articleData.Exists) rtn.Add(articleData);
                if (lp >= numberOfArticles) break;
                lp += 1;
            }
            return rtn;
        }

        public bool HasProperty(int propertyId)
        {
            if (_propXrefListId == null) PopulateLists();
            if (_propXrefListId.Contains(propertyId)) return true;
            return false;
        }
        public bool HasProperty(string propertyRef)
        {
            if (_propXrefListId == null) PopulateLists();
            if (_propXrefListRef.Contains(propertyRef)) return true;
            return false;
        }

        #endregion

        #region "models"

        public string ModelListName { get { return "modellist"; } }

        public void UpdateModels(List<SimplisityInfo> modelList)
        {
            Info.RemoveList("modellist");
            foreach (var sInfo in modelList)
            {
                var modelData = new ArticleModel(PortalCatalog, sInfo, CultureCode);
                UpdateModel(modelData);
            }
        }
        public List<SimplisityInfo> GetModelList()
        {
            return Info.GetList("modellist");
        }
        public ArticleModel AddModel()
        {
            var sInfo = new SimplisityInfo();
            var modelkey = GeneralUtils.GetUniqueString();
            sInfo.SetXmlProperty("genxml/hidden/modelkey", modelkey);
            Info.AddListItem("modellist", sInfo);
            return GetModel(modelkey);
        }
        public void UpdateModel(ArticleModel ArticleModel)
        {
            Info.RemoveListItem("modellist", "genxml/hidden/modelkey", ArticleModel.ModelKey);
            Info.AddListItem("modellist", ArticleModel.Info);
        }
        public ArticleModel GetModel(int idx)
        {
            return new ArticleModel(PortalCatalog, Info.GetListItem("modellist", idx), CultureCode);
        }
        public ArticleModel GetModel(string modelKey)
        {
            return new ArticleModel(PortalCatalog, Info.GetListItem("modellist", "genxml/hidden/modelkey", modelKey), CultureCode);
        }
        public List<ArticleModel> GetModels()
        {
            var rtn = new List<ArticleModel>();
            foreach (var i in Info.GetList("modellist"))
            {
                rtn.Add(new ArticleModel(PortalCatalog, i, CultureCode));
            }
            return rtn;
        }
        public Dictionary<string, string> GetModelDictionary(string template = "{ref} {name} {price}", string cultureCode = "")
        {
            if (cultureCode == "") cultureCode = DNNrocketUtils.GetCurrentCulture();
            var rtn = new Dictionary<string, string>();
            foreach (var modelData in GetModels())
            {
                if (!rtn.ContainsKey(modelData.ModelKey) && modelData.ModelKey != "")
                {
                    var temp = template.Replace("{ref}", modelData.Ref);
                    temp = temp.Replace("{name}", modelData.Name);
                    temp = temp.Replace("{barcode}", modelData.BarCode);
                    temp = temp.Replace("{price}", modelData.PriceDisplay(cultureCode));
                    rtn.Add(modelData.ModelKey, temp.Trim(' '));
                }
            }
            return rtn;
        }
        #endregion

        #region "properties"

        public string CultureCode { get; private set; }
        public string EntityTypeCode { get { return Info.GUIDKey; } }
        public SimplisityInfo Info { get; set; }
        public int ModuleId { get { return Info.ModuleId; } set { Info.ModuleId = value; } }
        public int XrefItemId { get { return Info.XrefItemId; } set { Info.XrefItemId = value; } }
        public int ParentItemId { get { return Info.ParentItemId; } set { Info.ParentItemId = value; } }
        public int ArticleId { get { return Info.ItemID; } set { Info.ItemID = value; } }
        public string GUIDKey { get { return Info.GUIDKey; } set { Info.GUIDKey = value; } }
        public int SortOrder { get { return Info.SortOrder; } set { Info.SortOrder = value; } }
        public string SystemKey { get { return Info.GetXmlProperty("genxml/systemkey"); } set { Info.SetXmlProperty("genxml/systemkey", value); } }
        public string ImageFolder { get; set; }
        public string DocumentFolder { get; set; }
        public string AppTheme { get; set; }
        public string AppThemeVersion { get; set; }
        public string AppThemeRelPath { get; set; }
        public PortalCatalogLimpet PortalCatalog { get; set; }
        public bool DebugMode { get; set; }
        public int PortalId { get { return Info.PortalId; } }
        public bool Exists { get {if (Info.ItemID  <= 0) { return false; } else { return true; }; } }
        public string LogoRelPath { get { var articleImage = GetImage(0); return articleImage.RelPath;} }
        public string NameUrl { get { return GeneralUtils.UrlFriendly(Name); } }
        public string Ref { get { return Info.GetXmlProperty(RefXPath); } set { Info.SetXmlProperty(RefXPath, value); } }
        public string RefXPath { get { return "genxml/textbox/articleref"; } }
        public string RichText { get { return Info.GetXmlProperty(RichTextXPath); } set { Info.SetXmlProperty(RichTextXPath, value); } }
        public string RichTextXPath { get { return "genxml/lang/genxml/textbox/articlerichtext"; } }
        public string Name { get { return Info.GetXmlProperty(NameXPath); } set { Info.SetXmlProperty(NameXPath, value); } }
        public string NameXPath { get { return "genxml/lang/genxml/textbox/articlename"; } }
        public DateTime PublishedDate { get { if (Info.GetXmlProperty(PublishedDateXPath) == "") return Info.ModifiedDate; else return Info.GetXmlPropertyDate(PublishedDateXPath); } set { Info.SetXmlProperty(PublishedDateXPath, value.ToString("O"), TypeCode.DateTime); } }
        public string PublishedDateXPath { get { return "genxml/textbox/publisheddate"; } }
        public string Summary { get { return Info.GetXmlProperty(SummaryXPath); } set { Info.SetXmlProperty(SummaryXPath, value); } }
        public string SummaryXPath { get { return "genxml/lang/genxml/textbox/articlesummary"; } }
        public bool Hidden { get { return Info.GetXmlPropertyBool(HiddenXPath); } set { Info.SetXmlProperty(HiddenXPath, value.ToString()); } }
        public string HiddenXPath { get { return "genxml/checkbox/hidden"; } }
        public bool HiddenByCulture { get { return Info.GetXmlPropertyBool(HiddenByCultureXPath); } set { Info.SetXmlProperty(HiddenByCultureXPath, value.ToString()); } }
        public string HiddenByCultureXPath { get { return "genxml/lang/genxml/checkbox/hidden"; } }
        public bool IsHidden { get { if (Hidden || HiddenByCulture) return true; else return false; } }
        public string SeoTitle
        {
            get
            {
                if (Info.GetXmlProperty(SeoTitleXPath) == "")
                    return Name;
                else
                    return Info.GetXmlProperty(SeoTitleXPath);
            }
        }

        public string SeoTitleXPath { get { return "genxml/lang/genxml/textbox/seotitle"; } }
        public string SeoDescription 
        { 
            get 
            {
                if (Info.GetXmlProperty(SeoKeyWordsXPath) == "")
                    return Summary;
                else
                    return Info.GetXmlProperty(SeoDescriptionXPath);
            }
        }
        public string SeoDescriptionXPath { get { return "genxml/lang/genxml/textbox/seodescription"; } }
        public string SeoKeyWords 
        { 
            get 
            { 
                if (Info.GetXmlProperty(SeoKeyWordsXPath) == "")
                    return SeoDescription; 
                else 
                    return Info.GetXmlProperty(SeoKeyWordsXPath); 
            } 
        }
        public string SeoKeyWordsXPath { get { return "genxml/lang/genxml/textbox/seokeyword"; } }
        public int ReviewCount { get { return Info.GetXmlPropertyInt("genxml/textbox/reviewcount"); } set { Info.SetXmlProperty("genxml/textbox/reviewcount",value.ToString(), TypeCode.Int32); } }
        public int ReviewScore { get { return Info.GetXmlPropertyInt("genxml/textbox/reviewscore"); } set { Info.SetXmlProperty("genxml/textbox/reviewscore", value.ToString(), TypeCode.Int32); } }
        public List<int> CategoryIds { get
        {
                if (_catXrefListId == null) PopulateLists();
                return _catXrefListId; 
            } 
        }
        public List<int> PropertyIds
        {
            get
            {
                if (_propXrefListId == null) PopulateLists();
                return _propXrefListId;
            }
        }
        #endregion

    }

}
