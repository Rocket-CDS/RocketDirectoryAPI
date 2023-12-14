# RSS Feed
RocketDirectory and Arapper Systems for it can implment an RSS feed.  This is activated by a url with specific parameters.

Example URL:
```
/Desktopmodules/dnnrocket/api/rocket/action?cmd=rocketblogapi_rss
```
resulting RSS feed
```
<rss xmlns:blog="http://rocket-cds.org" xmlns:atom="http://www.w3.org/2005/Atom" xmlns:media="http://search.yahoo.com/mrss/" version="2.0">
    <channel>
        <title><![CDATA[Testing RSS]]></title>
        <link><![CDATA[http://test.rocketcds.site/en-us/blog]]></link>
        <description><![CDATA[Testing Description]]></description>
        <pubDate><![CDATA[Thu, 14 Dec 2023 14:19:40 GMT]]></pubDate>
        <lastBuildDate><![CDATA[Thu, 14 Dec 2023 14:19:40 GMT]]></lastBuildDate>
        <generator><![CDATA[RocketCDS Blog RSS Generator]]></generator>
        <ttl><![CDATA[30]]></ttl>
        <atom:link href="https://test.rocketcds.site/Desktopmodules/dnnrocket/api/rocket/action?cmd=rocketblogapi_rss" rel="self" type="application/rss+xml" />
        <item>
            <title><![CDATA[111111111111111111111]]></title>
            <description><![CDATA[]]></description>
            <category><![CDATA[1111111111111]]></category>
            <guid isPermaLink="true"><![CDATA[http://test.rocketcds.site/en-us/blog/articleid/716/111111111111111111111]]></guid>
            <pubDate><![CDATA[Wed, 29 Nov 2023 00:00:00 GMT]]></pubDate>
            <media:thumbnail width="160" height="160" url="https://test.rocketcds.site/DesktopModules/DNNrocket/API/DNNrocketThumb.ashx?src=/Portals/0/DNNrocket/rocketblogapi/images/716/4OG3Cir0UEC3OrJXrJVHLg.jpg&w=160&h=160&imgtype=jpg" />
            <blog:publishedon><![CDATA[Wed, 29 Nov 2023 00:00:00 GMT]]></blog:publishedon>
        </item>
    </channel>
</rss>
```
## RSS template
The RSS is built using a razor template in the AppTheme selected for the system.  The template should be call "RSS.cshtml", if no template exists in the AppTheme no RSS feed will be generatored.  

Example RSS.cshtml template:
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
        <description><![CDATA[@catalogSettings.Summary <test>jsd cjhsd>< "!£$%^&*()"]]></description>
        <pubDate><![CDATA[@DateTime.Now.ToString("r")]]></pubDate>
        <lastBuildDate><![CDATA[@DateTime.Now.ToString("r")]]></lastBuildDate>
        <generator><![CDATA[RocketCDS Blog RSS Generator]]></generator>
        <ttl><![CDATA[30]]></ttl>
        <atom:link href="@(portalData.EngineUrlWithProtocol)/Desktopmodules/dnnrocket/api/rocket/action?cmd=rocketblogapi_rss" rel="self" type="application/rss+xml" />

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
This razor template formats the selected items into a valid RSS feed format.  
*In this example there is a setting in the AppTheme settings that can chose not to include the image.*  


## Generic Data List 
By default the RocketDirectory system includes a generic RSS feed generator to select data.    
Here we will explain the generic RSS feed options.  

The RSS feed uses URL query paramaters to select data.  The default system system expects at least 1 date for selecting data.  The date reference is passed to the RSS feed by using a URL parameter call "sqldataref", if not "sqldataref" is found in the URL a defualt of the last modified date is used.  

### Category selection
Data can be filtered by categories using a "catid" parameter.  The category id number can be found on the bottom left of the category detail admin UI.
```
/Desktopmodules/dnnrocket/api/rocket/action?cmd=rocketblogapi_rss&catid=725
```
*NOTE: The RSS requires a category id, which could change during the life cycle of the website.*  

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
			<systemkey>rocketblogapi</systemkey>
			<ref>articlename</ref>
			<xpath>genxml/lang/genxml/textbox/articlename</xpath>
			<typecode>rocketblogapiART</typecode>
		</genxml>
		<genxml>
			<systemkey>rocketblogapi</systemkey>
			<ref>publisheddate</ref>
			<xpath>genxml/textbox/publisheddate</xpath>
			<typecode>rocketblogapiART</typecode>
		</genxml>
		<genxml>
			<systemkey>rocketblogapi</systemkey>
			<ref>articleref</ref>
			<xpath>genxml/textbox/articleref</xpath>
			<typecode>rocketblogapiART</typecode>
		</genxml>
	</sqlindex>
```
*NOTE: You must ensure that the "ref" used is a date.*

Example URL:
```
/Desktopmodules/dnnrocket/api/rocket/action?cmd=rocketblogapi_rss&months=12&sqlidx=publisheddate
```
If no "sqlidx" parameter is defined in the URL, the ModifiedDate of the database record is used for the selection.  

## More flexability with Data in the RSS feed.
In some cases the default data selector in RocketDirectory is not enough.  If required any datalist can be output by creating a plugin for RocketCDS.


