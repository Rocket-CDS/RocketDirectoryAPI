# Category Menu
A RocketDirectory module can display a category menu.  The correct module templates needs to be setup and selected from the module settings.  
A URL query parameter for the category is used to identify the category that should be displayed.  The category URL key is defined in the dependacy file of the AppTheme.  

*Example of the blog dependancy file for the category url key. (See dependacy documention)*
```
<queryparams list="true">
	<genxml>
		<queryparam>blogcatid</queryparam>
		<tablename>rocketdirectoryapi</tablename>
		<systemkey>rocketblogapi</systemkey>
		<datatype>category</datatype>
	</genxml>
</queryparams>
```

## Module Template Definition
A module template needs to be created on the AppTheme with an entry in the dependancies files.  
This enables the category menu to be selected from the module settings.  

*Example of dependancy file template option*
```
<moduletemplates list="true">
	<genxml>
		<file>Categories.cshtml</file>
		<name>Category Menu</name>
	</genxml>
</moduletemplates>
```

## Category Menu Template
In the "default" sub-folder of the AppTheme the above template needs to be created.  


