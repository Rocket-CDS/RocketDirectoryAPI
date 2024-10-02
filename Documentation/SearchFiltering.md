# Search
Text searching works with SQL and/or the DNN search system.  

## DNN Search
The prefered method of text search is the DNN search.  It gives better performace, but less control.

## Setup of the DNN Search
- Ensure the DNN Scheduler is active.
- Ensure the "Search: Site Crawler" Scheduler Task is running.
- In RocketDirectory Admin Select a search page.
- Save the Admin Settings.
- Select a rocket directory module for the search. 


## DNN Search tips
The DNN search uses the Scheduler to populated the DNN search database, each time an article/content is edited the scheduler is told to rebuild the index for that article/content.  

If you are importing data or altering the DB without using the Admin UI you will need to run the **"Directory Admin Panel>Admin>Validate and Rebuild Search Index"** button.  

It can be difficult getting the DNN search to work on the category for the first time.  For some unknown reason it does not index instantly, the time for index is slow and if there is a problem it stops.   

- Try Restarting the AppPool
- Re-run the **"Directory Admin Panel>Admin>Validate and Rebuild Search Index"**
- Wait a period of time before testing.

**Remember:** The DNN search is controlled by DNN, there are options in Site Settings>Search.  The "Enable Part Word" option can be helpful, but it does slow the indexing.   

## Search Rules

"Category Select" and "Text Search" are mutually exclusive.  This is to make searching as simple as possible for end users.  
The "Text Search" is given priority.

*NOTE: If text searches need to be made on categories a plugin or system can be created with different rules.*  

### When a text search is activated
- The category selected will be cleared.  
- The Filters and Tags will continue to persists and be applied to the results.  

### When a Category is selected (Without a "Text Search")
- The category results will be displayed.
- The Filters and Tags will continue to persists and be applied to the results.  

### When a Category is selected (With a "Text Search")
- Any text search will **NOT** be cleared.    
- **The category selected will be ignored**, becuase the "Text Search" is active. *(See Clearing the "Text Search")*  
- The Filters and Tags will continue to persists and be applied to the results.  

### Default Category for a module
If there is a default category for a module it will be ignored if a text search is made.  The text search will be across all the articles.  

### Clearing the "Text Search"
This can be done by clearing the search session cookie.  
```
simplisity_setSessionField('searchtext', '');
```
Often we want to clear the "Text Search" when a category is selected.  This can be done by JS on the onclick event of the category menu.
```
<a href="@DNNrocketUtils.NavigateURL(sessionParams.TabId, catDict, c.Name)" onclick="simplisity_setSessionField('searchtext', '');$('.simplisity_loader').show();" catid="@c.CategoryId" class="navgocolink" level="0">@c.Name</a>
```
### Clearing the "Selected Tag"
```
simplisity_setSessionField('rocketpropertyidtag', '0');
```
or use the Razor token
```
@TagButtonClear(ResourceKey("DNNrocket.clear").ToString(), sessionParams)
```
### Clearing the "Filters"
```
$('.rocket-filtercheckbox').each(function(i, obj) {
    simplisity_setSessionField(this.id, false); 
});
```
or use the razor token
```
@FilterActionButton(ResourceKey("DNNrocket.clear").ToString(), sessionParams, false)
```

## Advanced XML SQL Search
There is a default index created that can be used or you can use other XML data.  
Sorting on XML is very slow and an index should be created, selecting on XML is quick and no real need for a special index column to be created.   


## The default index that are created.  
The sqlindex configuration is found in the "/systemrules.rules" file.
```
	  <sqlindex list="true">
		  <genxml>
			<systemkey>rocketdirectoryapi</systemkey>
			<ref>articlename</ref>
			<xpath>genxml/lang/genxml/textbox/articlename</xpath>
			<typecode>rocketdirectoryapiART</typecode>
		  </genxml>
		  <genxml>
			  <systemkey>rocketdirectoryapi</systemkey>
			  <ref>publisheddate</ref>
			  <xpath>genxml/textbox/publisheddate</xpath>
			  <typecode>rocketdirectoryapiART</typecode>
		  </genxml>
		  <genxml>
			  <systemkey>rocketdirectoryapi</systemkey>
			  <ref>articleref</ref>
			  <xpath>genxml/textbox/articleref</xpath>
			  <typecode>rocketdirectoryapiART</typecode>
		  </genxml>
	  </sqlindex>

```
*These can be altered by editing the "systemruels.rules" file and then you must do a validation of the system to rebuild the new index.*

## Defaut SQL search filter 
 
**IMPORTANT: The DNN search must be activated and you MUST also define a search page and search module in the Admin Settings of the RocketDirectory System.**  

The DNN search musts be linked to a modules, when the articles are updated a flag is placed on the module to reindex the data that has changed.  
If you do not do this link the DNN search will return nothing and hence the article list will return ALL products without any searching.  

```
{contains:searchtext}
```
## XML and index SQL search filter
```
and (
    isnull(articlename.GUIDKey,'') like '%{searchtext}%'
    or articleref.GUIDKey	    like '%{searchtext}%'
    or isnull([XMLData].value('(genxml/lang/genxml/textbox/articlekeywords)[1]','nvarchar(max)'),'') like '%{searchtext}%'
)
```
NOTE: The articlename index is used as the "articlename.GUIDKey" column.


## UserInRole

The sql filter supports testing for is a user is in a specific role.
**Token**
```
{isinrole:Manager}
```
**SQL Token test**
```
'{isinrole:ClientEditor}' = 'True'
```
This allows filtering on a role system.

### XML SQL filter exanple
```
and 
(
    (
		isnull(articlename.GUIDKey,'') like '%{searchtext}%'
		or isnull([XMLData].value('(genxml/textbox/articleref)[1]','nvarchar(max)'),'') like '%{searchtext}%'
		or isnull([XMLData].value('(genxml/lang/genxml/textbox/articlekeywords)[1]','nvarchar(max)'),'') like '%{searchtext}%'
    )
	and
	(
	    ([XMLData].value('(genxml/textbox/publisheddate)[1]','date') >= convert(date,'{searchdate1}') or '{searchdate1}' = '')
	    and
	    ([XMLData].value('(genxml/textbox/publisheddate)[1]','date') <= convert(date,'{searchdate2}') or '{searchdate2}' = '')
	)
)
and
(
    isnull([XMLData].value('(genxml/checkbox/internal)[1]','bit'),'0') = 0
    or
    (isnull([XMLData].value('(genxml/checkbox/internal)[1]','bit'),'0') = 1  and ('{isinrole:Manager}' = 'True' or '{isinrole:ClientEditor}' = 'True'))
)
```


## Date Range

The ```{searchdate1}``` and ```{searchdate2}``` are the data sources to deal with the range select.  
This example also included a ```{searchtext}```

### Razor Template
```
@inherits RocketDirectoryAPI.Components.RocketDirectoryAPITokens<Simplisity.SimplisityRazor>
@using RocketDirectoryAPI.Components;
@using DNNrocketAPI;
@using Simplisity;
@using RocketPortal.Components;
@using DNNrocketAPI.Components;
@using System.Globalization;
@using Rocket.AppThemes.Components;

@AddProcessData("resourcepath", "/DesktopModules/DNNrocket/api/App_LocalResources/")
@AddProcessData("resourcepath", "/DesktopModules/DNNrocketModules/RocketDirectoryAPI/App_LocalResources/")

@{
    var sessionParams = (SessionParams)Model.SessionParamsData;
    var portalContent = (PortalCatalogLimpet)Model.GetDataObject("portalcontent");
    var articleDataList = (ArticleLimpetList)Model.GetDataObject("articlelist");
    var categoryData = (CategoryLimpet)Model.GetDataObject("categorydata");
}

<div class="searchbar searchdata">
    <div class="searchbar-item">
        <div class="searchbar-label">Par mot-clé</div>
        <div class="searchbar-input">
            @TextBox(new SimplisityInfo(), "genxml/textbox/searchtext", " class='simplisity_sessionfield actionentrykey' autocomplete='off' ", sessionParams.Get("searchtext"))
            <span class="searchbar-btn clearsearch" style="display:none;" onclick="clearSearch();return false;">@ButtonIcon(ButtonTypes.cancel)</span>
            <span class="searchbar-btn simplisity_click dosearch" onclick="doSearchReload();return false;">@ButtonIcon(ButtonTypes.search)</span>
        </div>
    </div>
    <div class="searchbar-item">
        <div class="searchbar-label">Par date</div>
        <div class="searchbar-daterange">
            <div class="searchbar-input searchbar-item">
                <div class="searchbar-inputpre">@ResourceKey("DNNrocket.from") :</div>
                @TextBox(new SimplisityInfo(), "genxml/textbox/searchdate1", " class='simplisity_sessionfield' autocomplete='off' ", sessionParams.Get("searchdate1"), false, 0, "","date")
            </div>
            <div class="searchbar-input searchbar-item">
                <div class="searchbar-inputpre">@ResourceKey("DNNrocket.to") :</div>
                @TextBox(new SimplisityInfo(), "genxml/textbox/searchdate2", " class='simplisity_sessionfield' autocomplete='off' ", sessionParams.Get("searchdate2"), false, 0, "","date")
            </div>
        </div>
    </div>
</div>

<script>

    $(document).ready(function () {
        if ($('.actionentrykey').val() !== '') {
            $('.actionentrykey').focus();
            $('.clearsearch').show();
            $('.dosearch').hide();
            var tmpStr = $('.actionentrykey').val();
            $('.actionentrykey').val('');
            $('.actionentrykey').val(tmpStr);
            $('#searchtext').addClass('w3-theme-light');
        }
        else {
            $('#searchtext').removeClass('w3-theme-light');
            $('.clearsearch').hide();
            $('.dosearch').show();
        }

        $('.actionentrykey').unbind('keypress');
        $('.actionentrykey').on('keypress', function (e) {
            if (e.keyCode == 13) {
                doSearchReload();
                return false; // prevent the button click from happening
            }
        });

    });

    function clearSearch() {
        $('#searchtext').val('');
        $('#searchdate1').val('');
        $('#searchdate2').val('');
        doSearchReload();
    }

    function doSearchReload() {
        simplisity_setSessionField('searchtext', $('#searchtext').val());
        simplisity_setSessionField('searchdate1', $('#searchdate1').val());
        simplisity_setSessionField('searchdate2', $('#searchdate2').val());
        simplisity_setSessionField('pagesize', $('#pagesize').val());
        simplisity_setSessionField('orderbyref', $('#orderbyref').val());
        $('.simplisity_loader').show();
        @{
        var urlparams = new Dictionary<string, string>();
        urlparams.Add("page", "1");
        }
        window.location.href = '@(DNNrocketUtils.NavigateURL(sessionParams.TabId, urlparams))';
    }

</script>

```
### SQL filter settings
The SQL filter settings are edited in the "Admin Settings" for the system.
```
and 
(
    (
	    isnull(articlename.GUIDKey,'') like '%{searchtext}%'
	    or isnull([XMLData].value('(genxml/textbox/articleref)[1]','nvarchar(max)'),'') like '%{searchtext}%'
	    or isnull([XMLData].value('(genxml/lang/genxml/textbox/articlekeywords)[1]','nvarchar(max)'),'') like '%{searchtext}%'
    )
	and
	(
	    ([XMLData].value('(genxml/textbox/publisheddate)[1]','date') >= convert(date,'{searchdate1}') or '{searchdate1}' = '')
	    and
	    ([XMLData].value('(genxml/textbox/publisheddate)[1]','date') <= convert(date,'{searchdate2}') or '{searchdate2}' = '')
	)
)
```

