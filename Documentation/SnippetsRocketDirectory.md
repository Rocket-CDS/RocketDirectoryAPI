# Snippets RocketDirectory
These snippets and examples give help for building AppThemes.    

## Templates
### Admin Razor Header
```
@inherits RocketDirectoryAPI.Components.RocketDirectoryAPITokens<Simplisity.SimplisityRazor>
@using DNNrocketAPI.Components;
@using RocketDirectoryAPI.Components;
@AssigDataModel(Model)
@AddProcessDataResx(appTheme, true)
<!--inject-->
```
### ThemeSettings.cshtml
```
@inherits RocketDirectoryAPI.Components.RocketDirectoryAPITokens<Simplisity.SimplisityRazor>
@AssigDataModel(Model)
@AddProcessDataResx(appTheme, true)
<!--inject-->
@{
    var info = moduleDataInfo;
    //NOTE: xPath for module settings must use "genxml/settings/*"
}
```

### Admin Language Selector
The language selector must be added using the @RenderLanguageSelector() token.  
```
@{
    var sfieldDict = new Dictionary<string, string>();
    sfieldDict.Add("articleid", articleData.ArticleId.ToString());
    sfieldDict.Add("moduleedit", sessionParams.GetBool("moduleedit"));
    sfieldDict.Add("tabid", sessionParams.TabId.ToString());
}
@RenderLanguageSelector("articleadmin_editarticle", sfieldDict, appThemeDirectory, Model)

```
### Admin Search (AdminList.cshtml)
```
[INJECT:appthemedirectory,AdminSearch.cshtml]
```
### Admin Buttons. (AdminDetail.cshtml)
```
[INJECT:appthemedirectory,AdminDetailButtons.cshtml]
```
### AdminHeader.cshtml (mandatory)
If an AppTheme requires more header data, it can be added by creating a template called "AdminHeader.cshtml".  This template is considered mandatory, even if it contains nothing.  The system will always try and inject it and if it cannot be found it will display the INJECT token.  
```
@inherits RocketDirectoryAPI.Components.RocketDirectoryAPITokens<Simplisity.SimplisityRazor>
<!-- Placeholder template. so we do not get a error display on admin. -->
<!--inject-->
```
### moduleedit (optional)
The "moduleedit" flag is set to true if article is being edited directly from the module public list detail page.  
If it is being edited directly from the module this flag ensures the "AdminDetailButtons.cshtml" template returns to the public detail view after editing.  
It is option, but makes a more friendly UI.  

### CKEditor4 - Shared Inject (optional)
Usually not required in the AppTheme templates, this is added by the directory system in the "AdminPanelHeader.cshtml" template.  
```
[INJECT:appthemedirectory,CKEditor4.cshtml]
```
## Categories
*Admin Example*
```
<div onclick="$('#categoriestab').toggle();$('#categoriestabexpand').toggle();" class="w3-button w3-block w3-theme-l3 w3-left-align ">
    @ResourceKey("RC.categories")
    <span id="categoriestabexpand" class="material-icons w3-right" style="display:none;">
        unfold_more
    </span>
</div>
<div id="categoriestab" class='w3-row sectionname a-articlecategorylist w3-border w3-padding' style="">
    [INJECT:appthemedirectory,ArticleCategoryList.cshtml]
</div>
```
*View Example*
```
@foreach (var c in articleData.GetCategories())
{
    <span>@c.Name</span>
}
```
## Properties
*Admin Example*
```
<div onclick="$('#propertiestab').toggle();$('#propertiestabexpand').toggle();" class="w3-button w3-block w3-theme-l3 w3-left-align  w3-margin-top">
    @ResourceKey("RC.properties")
    <span id="propertiestabexpand" class="material-icons w3-right" style="display:none;">
        unfold_more
    </span>
</div>
<div id="propertiestab" class='w3-row sectionname a-articlepropertylist w3-border w3-padding' style="">
    [INJECT:appthemedirectory,ArticlePropertyList.cshtml]
</div>
```
*View Example*
```
@foreach (var p in articleData.GetProperties())
{
    <strong>@p.Name</strong>
    <br />
    @p.Summary
}
```
## Images
*Admin Example*
```
<div onclick="$('#imgs').toggle();$('#imgsexpand').toggle();" class="w3-button w3-block w3-theme-l3 w3-left-align w3-margin-top">
    @ResourceKey("DNNrocket.images")
    <span id="imgsexpand" class="material-icons w3-right" style="display:none;">
        unfold_more
    </span>
</div>
<div id="imgs" class='w3-row sectionname w3-border w3-padding' style="">
    [INJECT:appthemedirectory,Articleimages.cshtml]
</div>
```
*View Example*
```
<div>
    @foreach (var i in articleData.GetImages())
    {
        <img src="@ImageUrl(i.RelPathWebp,120,0)" alt="" />
    }
</div>
```
## Documents

*Admin Example*
```
<div onclick="$('#docs').toggle();$('#docsexpand').toggle();" class="w3-button w3-block w3-theme-l3 w3-left-align w3-margin-top">
    @ResourceKey("DNNrocket.docs")
    <span id="docsexpand" class="material-icons w3-right" style="display:none;">
        unfold_more
    </span>
</div>
<div id="docs" class='w3-row sectionname w3-border w3-padding' style="">
    [INJECT:appthemedirectory,ArticleDocuments.cshtml]
</div>
```
*View Example*
```
@foreach (var doc in articleData.GetDocs())
{
    if (!doc.Hidden)
    {
        <a target="_blank" href="/@doc.RelPath">
            @ButtonIcon(ButtonTypes.download)&nbsp;@doc.Name
        </a>                    
    }
}
```
## Links
*Admin Example*
```
<div onclick="$('#links').toggle();$('#linksexpand').toggle();" class="w3-button w3-block w3-theme-l3 w3-left-align w3-margin-top">
    @ResourceKey("DNNrocket.links")
    <span id="linksexpand" class="material-icons w3-right" style="display:none;">
        unfold_more
    </span>
</div>
<div id="links" class='w3-row sectionname w3-border w3-padding' style="">
    [INJECT:appthemedirectory,ArticleLinks.cshtml]
</div>
```
*View Example*
```
@foreach (var lk in articleData.Getlinks())
{
    if (!lk.Hidden)
    {
        <a target="_blank" href="@lk.Url" target="@lk.Target">
            @ButtonIcon(ButtonTypes.link)&nbsp;@lk.Name
        </a>
    }
}
```
## SEO Fields
*Admin Example*
```
<div onclick="$('#seotab').toggle();$('#seotabexpand').toggle();" class="w3-button w3-block w3-theme-l3 w3-left-align w3-margin-top">
    SEO
    <span id="seotabexpand" class="material-icons w3-right" style="display:none;">
        unfold_more
    </span>
</div>
[INJECT:appthemedirectory,ArticleSEO.cshtml]
```
