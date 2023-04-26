using DNNrocketAPI;
using DNNrocketAPI.Components;
using HandlebarsDotNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketDirectoryAPI.Components
{
    public class HandlebarsEngineRC : HandlebarsEngine
    {
        private const string _systemKey = "rocketdirectoryapi";
        public string ExecuteRC(string source, object model)
        {
            try
            {
                var hbs = HandlebarsDotNet.Handlebars.Create();
                RegisterHelpers(hbs);
                RegisterRCHelpers(hbs);

                var systemData = new SystemLimpet(_systemKey);
                foreach (var p in systemData.HandleBarsList)
                {
                    var assembly = p.Assembly;
                    var nameSpaceClass = p.NameSpaceClass;
                    var cacheKeyhbs = assembly + "," + nameSpaceClass;
                    var hbsprov = (HandleBarsInterface)CacheUtilsDNN.GetCache(cacheKeyhbs);
                    if (hbsprov == null)
                    {
                        hbsprov = HandleBarsInterface.Instance(assembly, nameSpaceClass);
                        CacheUtilsDNN.SetCache(cacheKeyhbs, hbsprov);
                    }
                    hbsprov.RegisterHelpers(ref hbs, _systemKey);
                }

                return CompileTemplate(hbs, source, model);
            }
            catch (Exception ex)
            {
                LogUtils.LogException(ex);
                throw new TemplateException("Failed to render Handlebar template : " + ex.Message, ex, model, source);
            }
        }

        public static void RegisterRCHelpers(IHandlebars hbs)
        {
            RegisterArticle(hbs);
            RegisterArticleTest(hbs);
            RegisterImageShow(hbs);
            RegisterImage(hbs);
            RegisterDocumentShow(hbs);
            RegisterDocument(hbs);
            RegisterLinkShow(hbs);
            RegisterLink(hbs);
            RegisterEngineUrl(hbs);
            RegisterCultureCodeEdit(hbs);
            RegisterCultureCode(hbs);
            RegisterIsInCategory(hbs);
            RegisterHasProperty(hbs);
        }

        private static ArticleLimpet GetArticleData(JObject o)
        {
            var articleid = (string)o.SelectToken("genxml.data.genxml.column.itemid") ?? "";
            var portalid = (string)o.SelectToken("genxml.data.genxml.column.portalid") ?? "-1";
            var cultureCode = (string)o.SelectToken("genxml.sessionparams.r.culturecode") ?? "";
            var systemKey = (string)o.SelectToken("genxml.sessionparams.r.systemkey") ?? "";
            if (!GeneralUtils.IsNumeric(articleid)) return new ArticleLimpet(systemKey);

            var cacheKey = portalid + "*" + articleid + "*" + cultureCode;
            var articleData = (ArticleLimpet)CacheUtils.GetCache(cacheKey, "article");
            if (articleData == null)
            {
                articleData = new ArticleLimpet(Convert.ToInt32(articleid), cultureCode, systemKey);
                CacheUtils.SetCache(cacheKey, articleData, "article");
            }
            return articleData;
        }

        private static void RegisterEngineUrl(IHandlebars hbs)
        {
            hbs.RegisterHelper("engineurl", (writer, context, arguments) =>
            {
                var dataValue = "";
                if (arguments[0] != null)
                {
                    var o = (JObject)arguments[0];
                    dataValue = (string)o.SelectToken("genxml.sessionparams.r.engineurl") ?? "";
                }
                writer.WriteSafeString(dataValue);
            });
        }
        private static void RegisterCultureCode(IHandlebars hbs)
        {
            hbs.RegisterHelper("culturecode", (writer, context, arguments) =>
            {
                var dataValue = "";
                if (arguments[0] != null)
                {
                    var o = (JObject)arguments[0];
                    dataValue = (string)o.SelectToken("genxml.sessionparams.r.culturecode") ?? "";
                }
                writer.WriteSafeString(dataValue);
            });
        }
        private static void RegisterCultureCodeEdit(IHandlebars hbs)
        {
            hbs.RegisterHelper("culturecodeedit", (writer, context, arguments) =>
            {
                var dataValue = "";
                if (arguments[0] != null)
                {
                    var o = (JObject)arguments[0];
                    dataValue = (string)o.SelectToken("genxml.sessionparams.r.culturecodeedit") ?? "";
                }
                writer.WriteSafeString(dataValue);
            });
        }
        // Get Image Data
        private static void RegisterImageShow(IHandlebars hbs)
        {
            hbs.RegisterHelper("imagetest", (writer, options, context, arguments) =>
            {
                var o = (JObject)arguments[0];
                var articleData = GetArticleData(o);

                var imgidx = 0;
                if (arguments.Length >= 3) imgidx = Convert.ToInt32(arguments[2]);
                var img = articleData.GetImage(imgidx);
                var cmd = arguments[1].ToString();

                if (cmd == "isshown")
                {
                    if (!img.Hidden)
                        options.Template(writer, (object)context);
                    else
                        options.Inverse(writer, (object)context);
                }
                if (cmd == "ishidden")
                {
                    if (img.Hidden)
                        options.Template(writer, (object)context);
                    else
                        options.Inverse(writer, (object)context);
                }
                if (cmd == "hasheading")
                {
                    if (img.Alt != "")
                        options.Template(writer, (object)context);
                    else
                        options.Inverse(writer, (object)context);
                }
                if (cmd == "hasimage")
                {
                    if (img.RelPath != "")
                        options.Template(writer, (object)context);
                    else
                        options.Inverse(writer, (object)context);
                }
                if (cmd == "haslink")
                {
                    if (img.UrlText != "")
                        options.Template(writer, (object)context);
                    else
                        options.Inverse(writer, (object)context);
                }
                if (cmd == "hassummary")
                {
                    if (!String.IsNullOrWhiteSpace(img.Summary))
                        options.Template(writer, (object)context);
                    else
                        options.Inverse(writer, (object)context);
                }


            });
        }
        private static void RegisterImage(IHandlebars hbs)
        {
            hbs.RegisterHelper("image", (writer, context, arguments) =>
            {
                var dataValue = "";
                if (arguments.Length >= 2)
                {
                    var o = (JObject)arguments[0];
                    var articleData = GetArticleData(o);

                    var imgidx = 0;
                    if (arguments.Length >= 3) imgidx = Convert.ToInt32(arguments[2]);
                    var img = articleData.GetImage(imgidx);
                    var cmd = arguments[1].ToString();

                    switch (cmd)
                    {
                        case "alt":
                            dataValue = img.Alt;
                            break;
                        case "relpath":
                            dataValue = img.RelPath;
                            break;
                        case "height":
                            dataValue = img.Height.ToString();
                            break;
                        case "width":
                            dataValue = img.Width.ToString();
                            break;
                        case "count":
                            dataValue = articleData.GetImageList().Count.ToString();
                            break;
                        case "summary":
                            dataValue = img.Summary;
                            break;
                        case "url":
                            dataValue = img.Url;
                            break;
                        case "urltext":
                            dataValue = img.UrlText;
                            break;
                        case "hidden":
                            dataValue = img.Hidden.ToString();
                            break;
                        case "thumburl":
                            var width = Convert.ToInt32(arguments[3]);
                            var height = Convert.ToInt32(arguments[4]);
                            var enginrUrl = (string)o.SelectToken("genxml.sessionparams.r.engineurl") ?? "";
                            dataValue = enginrUrl.TrimEnd('/') + "/DesktopModules/DNNrocket/API/DNNrocketThumb.ashx?src=" + img.RelPath + "&w=" + width + "&h=" + height;
                            break;
                        case "fieldid":
                            dataValue = img.FieldId;
                            break;
                        default:
                            dataValue = img.Info.GetXmlProperty(cmd);
                            if (dataValue == "") dataValue = img.Info.GetXmlProperty("genxml/lang/" + cmd);
                            break;

                    }
                }

                writer.WriteSafeString(dataValue);
            });
        }

        private static void RegisterDocumentShow(IHandlebars hbs)
        {
            hbs.RegisterHelper("doctest", (writer, options, context, arguments) =>
            {
                var o = (JObject)arguments[0];
                var articleData = GetArticleData(o);

                var cmd = arguments[1].ToString();
                var idx = 0;
                if (arguments.Length >= 3) idx = (int)arguments[2];
                var doc = articleData.GetDoc(idx);

                if (cmd == "isshown")
                {
                    if (!doc.Hidden)
                        options.Template(writer, (object)context);
                    else
                        options.Inverse(writer, (object)context);
                }
                if (cmd == "ishidden")
                {
                    if (doc.Hidden)
                        options.Template(writer, (object)context);
                    else
                        options.Inverse(writer, (object)context);
                }

            });
        }
        private static void RegisterDocument(IHandlebars hbs)
        {
            hbs.RegisterHelper("document", (writer, context, arguments) =>
            {
                var dataValue = "";
                if (arguments.Length >= 2)
                {
                    var o = (JObject)arguments[0];
                    var articleData = GetArticleData(o);

                    var cmd = arguments[1].ToString();
                    var idx = 0;
                    if (arguments.Length >= 3) idx = (int)arguments[2];
                    var doc = articleData.GetDoc(idx);

                    switch (cmd)
                    {
                        case "key":
                            dataValue = doc.DocKey;
                            break;
                        case "name":
                            dataValue = doc.Name;
                            break;
                        case "hidden":
                            dataValue = doc.Hidden.ToString().ToLower();
                            break;
                        case "fieldid":
                            dataValue = doc.FieldId;
                            break;
                        case "relpath":
                            dataValue = doc.RelPath;
                            break;
                        case "count":
                            dataValue = articleData.GetDocList().Count.ToString();
                            break;
                        case "url":
                            var enginrUrl = (string)o.SelectToken("genxml.sessionparams.r.engineurl") ?? "";
                            dataValue = enginrUrl.TrimEnd('/') + "/DesktopModules/DNNrocket/API/DNNrocketThumb.ashx?src=" + doc.RelPath;
                            break;
                        default:
                            dataValue = doc.Info.GetXmlProperty(cmd);
                            if (dataValue == "") dataValue = doc.Info.GetXmlProperty("genxml/lang/" + cmd);
                            break;
                    }
                }
                writer.WriteSafeString(dataValue);
            });
        }
        private static void RegisterLinkShow(IHandlebars hbs)
        {
            hbs.RegisterHelper("linktest", (writer, options, context, arguments) =>
            {
                var o = (JObject)arguments[0];
                var articleData = GetArticleData(o);
                var cmd = arguments[1].ToString();
                var idx = 0;
                if (arguments.Length >= 3) idx = (int)arguments[2];
                var lnk = articleData.Getlink(idx);

                if (cmd == "isshown")
                {
                    if (!lnk.Hidden)
                        options.Template(writer, (object)context);
                    else
                        options.Inverse(writer, (object)context);
                }
                if (cmd == "ishidden")
                {
                    if (lnk.Hidden)
                        options.Template(writer, (object)context);
                    else
                        options.Inverse(writer, (object)context);
                }

            });
        }
        private static void RegisterLink(IHandlebars hbs)
        {
            hbs.RegisterHelper("link", (writer, context, arguments) =>
            {
                var dataValue = "";
                if (arguments.Length >= 2)
                {
                    var o = (JObject)arguments[0];
                    var articleData = GetArticleData(o);
                    var cmd = arguments[1].ToString();
                    var idx = 0;
                    if (arguments.Length >= 3) idx = (int)arguments[2];
                    var lnk = articleData.Getlink(idx);

                    switch (cmd)
                    {
                        case "key":
                            dataValue = lnk.LinkKey;
                            break;
                        case "name":
                            dataValue = lnk.Name;
                            break;
                        case "hidden":
                            dataValue = lnk.Hidden.ToString().ToLower();
                            break;
                        case "fieldid":
                            dataValue = lnk.FieldId;
                            break;
                        case "count":
                            dataValue = articleData.GetLinkList().Count.ToString();
                            break;
                        case "ref":
                            dataValue = lnk.Ref;
                            break;
                        case "type":
                            dataValue = lnk.LinkType.ToString();
                            break;
                        case "target":
                            dataValue = lnk.Target;
                            break;
                        case "anchor":
                            dataValue = lnk.Anchor;
                            break;
                        case "url":
                            dataValue = lnk.Url;
                            break;
                        default:
                            dataValue = lnk.Info.GetXmlProperty(cmd);
                            if (dataValue == "") dataValue = lnk.Info.GetXmlProperty("genxml/lang/" + cmd);
                            break;
                    }
                }

                writer.WriteSafeString(dataValue);
            });
        }

        private static void RegisterArticleTest(IHandlebars hbs)
        {
            hbs.RegisterHelper("articletest", (writer, options, context, arguments) =>
            {
                var o = (JObject)arguments[0];
                var articleData = GetArticleData(o);
                var cmd = arguments[1].ToString();

                if (cmd == "haslinks")
                {
                    if (articleData.GetLinkList().Count > 0)
                        options.Template(writer, (object)context);
                    else
                        options.Inverse(writer, (object)context);
                }
                if (cmd == "hasdocs")
                {
                    if (articleData.GetDocList().Count > 0)
                        options.Template(writer, (object)context);
                    else
                        options.Inverse(writer, (object)context);
                }
                if (cmd == "hasimages")
                {
                    if (articleData.GetImageList().Count > 0)
                        options.Template(writer, (object)context);
                    else
                        options.Inverse(writer, (object)context);
                }
                if (cmd == "haslist")
                {
                    var listname = arguments[2].ToString();
                    if (articleData.Info.GetList(listname).Count > 0)
                        options.Template(writer, (object)context);
                    else
                        options.Inverse(writer, (object)context);
                }
                if (cmd == "ishidden")
                {
                    if (!articleData.IsHidden)
                        options.Template(writer, (object)context);
                    else
                        options.Inverse(writer, (object)context);
                }

            });
        }
        private static void RegisterArticle(IHandlebars hbs)
        {
            hbs.RegisterHelper("article", (writer, context, arguments) =>
            {
                var dataValue = "";
                if (arguments[0] != null)
                {
                    var o = (JObject)arguments[0];
                    var cmd = arguments[1].ToString();
                    var articleData = GetArticleData(o);
                    var cultureCode = (string)o.SelectToken("genxml.sessionparams.r.culturecode");

                    switch (cmd)
                    {
                        case "ref":
                            dataValue = (string)o.SelectToken("genxml.data.genxml.textbox.articleref") ?? "";
                            break;
                        case "name":
                            dataValue = (string)o.SelectToken("genxml.data.genxml.lang.genxml.textbox.articlename") ?? "";
                            break;
                        case "summary":
                            dataValue = (string)o.SelectToken("genxml.data.genxml.lang.genxml.textbox.articlesummary");
                            if (dataValue == null) dataValue = "";
                            dataValue = BreakOf(dataValue);
                            break;
                        case "detailurl":
                            var url = (string)o.SelectToken("genxml.remotemodule.genxml.remote.detailpageurl" + cultureCode);
                            if (String.IsNullOrEmpty(url)) url = (string)o.SelectToken("genxml.sessionparams.r.pagedetailurl");                            
                            url = PagesUtils.GetPageURL(articleData.PortalCatalog.DetailPageTabId);
                            dataValue = url;
                            break;
                        case "defaultcategory":
                            if (articleData.GetCategories().Count > 0) dataValue = articleData.GetCategories().First().Name;
                            break;
                        case "defaultproperty":
                            if (articleData.GetProperties().Count > 0) dataValue = articleData.GetProperties().First().Name;
                            break;
                        default:

                            var displaytype = "";
                            if (arguments.Length >= 3) displaytype = arguments[2].ToString();
                            var displayculturecode = articleData.CultureCode;
                            if (arguments.Length >= 4) displayculturecode = arguments[3].ToString();

                            dataValue = articleData.Info.GetXmlProperty(cmd);
                            if (dataValue == "") dataValue = articleData.Info.GetXmlProperty("genxml/lang/" + cmd);
                            if (displaytype == "breakof") dataValue = BreakOf(dataValue);
                            if (displaytype == "money") dataValue = CurrencyUtils.CurrencyDisplay(dataValue, displayculturecode);
                            break;
                    }
                }

                writer.WriteSafeString(dataValue);
            });
        }

        private static void RegisterIsInCategory(HandlebarsDotNet.IHandlebars hbs)
        {
            hbs.RegisterHelper("IsInCategory", (writer, options, context, parameters) =>
            {
                try
                {
                    if (parameters != null && parameters.Length == 2)
                    {
                        var o = (JObject)parameters[0];
                        var articleData = GetArticleData(o);
                        string categoryRef = parameters[1].ToString();
                        if (articleData.Exists)
                        {
                            if (articleData.IsInCategory(categoryRef))
                                options.Template(writer, (object)context);
                            else
                                options.Inverse(writer, (object)context);
                        }
                        else
                        {
                            options.Inverse(writer, (object)context);
                        }
                    }
                    else
                    {
                        writer.WriteSafeString("INCORRECT ARGS: {{#IsInCategoryById this ArticleId CategoryId }}");
                    }
                }
                catch (Exception ex)
                {
                    writer.WriteSafeString(ex.ToString());
                }
            });
        }
        private static void RegisterHasProperty(HandlebarsDotNet.IHandlebars hbs)
        {
            hbs.RegisterHelper("HasProperty", (writer, options, context, parameters) =>
            {
                try
                {
                    if (parameters != null && parameters.Length == 2)
                    {
                        var o = (JObject)parameters[0];
                        var articleData = GetArticleData(o);
                        string propRef = parameters[1].ToString();
                        if (articleData.Exists)
                        {
                            if (articleData.HasProperty(propRef))
                                options.Template(writer, (object)context);
                            else
                                options.Inverse(writer, (object)context);
                        }
                        else
                        {
                            options.Inverse(writer, (object)context);
                        }
                    }
                    else
                    {
                        writer.WriteSafeString("INCORRECT ARGS: {{#IsInCategoryById this ArticleId CategoryId }}");
                    }
                }
                catch (Exception ex)
                {
                    writer.WriteSafeString(ex.ToString());
                }
            });
        }


        private static string BreakOf(String strIn)
        {
            var strOut = System.Web.HttpUtility.HtmlEncode(strIn);
            strOut = strOut.Replace(Environment.NewLine, "<br/>");
            strOut = strOut.Replace("\n", "<br/>");
            strOut = strOut.Replace("\t", "&nbsp;&nbsp;&nbsp;");
            strOut = strOut.Replace("'", "&apos;");
            return strOut;
        }

    }
}
