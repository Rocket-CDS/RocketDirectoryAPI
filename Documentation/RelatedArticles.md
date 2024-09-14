# Related Articles
Related articles are articles that are related in some way to the product being viewed.  

RocketDirectory uses an auto association method to make the link between articles.  Properties are used and any article with a matching property is automatically asscoiated.  

The display of related properties can be controlled by using property groups.  

## Related Property Display

*Get a list of 4 related articles in **RANDOM** order.*  
```
var relatedArticles = articleData.GetRelatedArticles("",4);
```
*Get a list of articles that are only in the "related" property group*
```
var relatedArticles = articleData.GetRelatedArticles("related",4);
```
*Example of displaying related particles*
```
<div class="w3-xlarge w3-margin-top">@ResourceKey("ProductsFood.relatedarticles")</div>
<div class="w3-row">
    @foreach (var relatedArticleData in relatedArticles)
    {
        var urlparams = new Dictionary<string, string>();
        urlparams.Add("articleid", relatedArticleData.ArticleId.ToString());

        <div class="w3-col s12 m6 l3 w3-padding w3-center">
            <a href="@(DNNrocketUtils.NavigateURL(sessionParams.TabId, urlparams, relatedArticleData.Name))" title="@relatedArticleData.Name" onclick="$('.simplisity_loader').show()">
                <img src="@ImageUrl(relatedArticleData.GetImage(0).RelPath,200,200)" style="width:100%;" alt="@relatedArticleData.Name">
            </a>
            <div class="" style="height:100px;overflow:hidden;">
                <h3>@relatedArticleData.Name</h3>
                <p>@relatedArticleData.Summary</p>
            </div>
        </div>
    }
</div>
```
