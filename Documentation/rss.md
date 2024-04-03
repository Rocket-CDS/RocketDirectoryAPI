# RSS Feed
RocketDirectory and Wrapper Systems can implment an RSS feed.  This is activated by a url with specific parameters.

Example URL:
```
/Desktopmodules/dnnrocket/api/rocket/action?cmd=rocketdirectoryapi_rss
```
```
/Desktopmodules/dnnrocket/api/rocket/action?cmd=rocketblogapi_rss
```
```
/Desktopmodules/dnnrocket/api/rocket/action?cmd=rocketnewsapi_rss
```
## RSS URL Token
To make creating the RSS URL easier, you can use a razor token.
```
RssUrl(int portalId, string cmd, int numberOfMonths = 1, string sqlidx = "", int catid = 0)
```
```
RssUrl(int portalId, string cmd, int yearDate, int monthDate, int numberOfMonths = 1, string sqlidx = "", int catid = 0)
```
*portalId = PortalId*  
*cmd = RSS command (rocketdirectoryapi_rss)*  
*yearDate = Starting Year (Default to current year)*  
*monthDate = Starting Month (Default to current year, 1st of the month is always used)*  
*numberOfMonths = number of months to search (Optional)*  
*sqlidx = SQL index key (Optional)*  
*catid = categoryID to search (Optional)*  

Example:
```
@RssUrl(portalData.PortalId,"rocketblogapi_rss")
```
```
@RssUrl(portalData.PortalId, "rocketblogapi_rss", DateTime.Now.Year, DateTime.Now.Month)
```

NOTE: Some systems may have their own RSS feed token, like RocketEventsAPI.  
*ONLY use in RocketEventsAPI*  
```
@RssEventUrl(portalData.PortalId,sessionParams.Get("cmd"),sessionParams.GetInt("month"),sessionParams.GetInt("year"))
```

Example code
```
<a href="@RssUrl(portalData.PortalId,"rocketblogapi_rss",1,"publisheddate",sessionParams.GetInt("blogcatid"))" target="_blank">
    <span class="material-icons">
    rss_feed
    </span>
</a>

```
*NOTE: The RssUrl() token forces https:// protocol.*  

### Build your own Rss Url without using the razor token
```
<a href="@(portalData.EngineUrlWithProtocol)/Desktopmodules/dnnrocket/api/rocket/action?cmd=rocketblogapi_rss&months=2" target="_blank">
    <span class="material-icons">
    rss_feed
    </span>
</a>
```

## RSS template
The RSS is built using a razor template in the AppTheme selected for the system.  The template should be call "RSS.cshtml", if no template exists in the AppTheme no RSS feed will be generatored.  

Example RSS.cshtml template:  (CDATA should be used)
```
@inherits RocketDirectoryAPI.Components.RocketDirectoryAPITokens<Simplisity.SimplisityRazor>
@using DNNrocketAPI.Components;
@using RocketDirectoryAPI.Components;
@AssigDataModel(Model)
@{
    var rssList = (List<ArticleLimpet>)Model.GetDataObject("rsslist");
}

<rss xmlns:blog="http://rocket-cds.org" xmlns:atom="http://www.w3.org/2005/Atom" xmlns:media="http://search.yahoo.com/mrss/" version="2.0">

    <channel>
        
        <title><![CDATA[@catalogSettings.CatalogName]]></title>
        <link><![CDATA[@(DNNrocketUtils.NavigateURL(portalContent.ListPageTabId))]]></link>
        <description><![CDATA[@catalogSettings.Summary]]></description>
        <pubDate><![CDATA[@DateTime.Now.ToString("r")]]></pubDate>
        <lastBuildDate><![CDATA[@DateTime.Now.ToString("r")]]></lastBuildDate>
        <generator><![CDATA[RocketCDS RSS Generator]]></generator>
        <ttl><![CDATA[30]]></ttl>
        <atom:link href="@RssUrl(portalData.PortalId,sessionParams.Get("cmd"),sessionParams.GetInt("year"),sessionParams.GetInt("month"),sessionParams.GetInt("months"),sessionParams.Get("sqlidx"))" rel="self" type="application/rss+xml" />

@foreach (ArticleLimpet articleData in rssList)
{
    var urlparams = new Dictionary<string, string>();
    urlparams.Add("articleid", articleData.ArticleId.ToString());
    var date = articleData.Info.GetXmlPropertyDate("genxml/textbox/publisheddate");
    var dateFormat = date.ToShortDateString();

        <item>
            <title><![CDATA[@articleData.Name]]></title>
            <description><![CDATA[@articleData.Summary]]></description>
            @foreach (var c in articleData.GetCategories())
            {
            <category><![CDATA[@c.Name]]></category>
            }
            <guid isPermaLink="true"><![CDATA[@DetailUrl(moduleData.DetailPageTabId(), articleData, categoryData)]]></guid>
            <pubDate><![CDATA[@articleData.Info.GetXmlPropertyDate("genxml/textbox/publisheddate").ToString("r")]]></pubDate>
            @if (catalogSettings.Info.GetXmlPropertyBool("genxml/checkbox/rssimage"))
            {
                var imagewidthrss = catalogSettings.Info.GetXmlPropertyInt("genxml/textbox/imagewidthrss");
                var imageheightrss = catalogSettings.Info.GetXmlPropertyInt("genxml/textbox/imageheightrss");
                <media:thumbnail width="@(imagewidthrss)" height="@(imageheightrss)" url="@(portalData.EngineUrlWithProtocol)@ImageUrl(articleData.GetImage(0).RelPath, imagewidthrss, imageheightrss)" />
            } 
            <blog:publishedon><![CDATA[@articleData.Info.GetXmlPropertyDate("genxml/textbox/publisheddate").ToString("r")]]></blog:publishedon>
        </item>
}

    </channel>
    </rss>
```

The razor template formats the selected items into a valid RSS feed format.  
*In this example there is a setting in the AppTheme settings that can chose not to include the image.*  

## Generic Data List 
By default the RocketDirectory system includes a generic RSS feed generator to select data.    
Here we will explain the generic RSS feed options.  

### Number of Months
By default only 1 month of changes are returned.  This can be altered by adding the "months" parameter.  
```
/Desktopmodules/dnnrocket/api/rocket/action?cmd=rocketblogapi_rss&months=12
```
### sqlidx parameter for data
The "sqlidx" parameter is used to identify what date in the data Article should be used to do the selection.  
This "ref" is taken from the "sqlindex" section of the system.rules file.
```
<sqlindex list="true">
	<genxml>
        <systemkey>rocketdirectoryapi</systemkey>
        <ref>publisheddate</ref>
        <xpath>genxml/textbox/publisheddate</xpath>
        <typecode>rocketdirectoryapiART</typecode>
	</genxml>
</sqlindex>
```
*NOTE: You must ensure that the "ref" used is a date.*  

*rocketdirectorysapi = publisheddate*
*rocketblogapi = publisheddate*
*rocketnewsapi = publisheddate*

Example URL:
```
/Desktopmodules/dnnrocket/api/rocket/action?cmd=rocketblogapi_rss&months=12&sqlidx=publisheddate
```
If no "sqlidx" parameter is defined in the URL, the ModifiedDate of the database record is used for the selection.  

## More flexability with Data in the RSS feed.
In some cases the default data selector in RocketDirectory is not enough.  If required any datalist can be output by creating a plugin for RocketCDS.


