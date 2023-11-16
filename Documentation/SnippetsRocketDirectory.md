﻿# Snippets RocketDirectory
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
[INJECT:appthemedirectory,ArticleCategoryListBlock.cshtml]
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
[INJECT:appthemedirectory,ArticlePropertyListBlock.cshtml]
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
[INJECT:appthemedirectory,ArticleImagesBlock.cshtml]
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
[INJECT:appthemedirectory,ArticleDocumentsBlock.cshtml]
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
[INJECT:appthemedirectory,ArticleLinksBlock.cshtml]
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
[INJECT:appthemedirectory,ArticleSEOBlock.cshtml]
```
## Published Date
*Admin*
```
<label>@ResourceKey("DNNrocket.date")</label>
@TextBoxDate(info, articleData.PublishedDateXPath, " class='w3-input w3-border' autocomplete='off'", DateTime.Today.ToString("O"), false, 0)
```
*View*
```
@DateOf(articleData.Info, articleData.PublishedDateXPath, false, sessionParams.CultureCode)
```
By default the DB record ModifiedDate will be returned if no published date exists.  



