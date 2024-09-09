# Building a Directory Webiste

Building a directory website can be more complex than building a normal websites.  We need to deal with all the pages, plus all the directory items and categories.  
Here is a quick list of what needs to be done.  More information is available in the documentation.  

## DNN or RocketCDS Installation
Create an installation (AppService) of DNN or RocketCDS.  

**DNN**  
[https://docs.dnncommunity.org/content/getting-started/setup/requirements/index.html](https://docs.dnncommunity.org/content/getting-started/setup/requirements/index.html)

**RocketCDS**  
[https://docs.rocket-cds.org/installation](https://docs.rocket-cds.org/installation)  

## Module Installation
**Skip this step if you have used RocketCDS.**  *(RocketCDS automatically installs all the required modules.)*  
Install all the RocketModules needed.  Here is a minimal list.  
```
0_DNNrocket_*.*.*_Install.zip
RocketContentAPI_*.*.*_Install.zip
RocketContentMod_*.*.*_Install.zip
RocketDirectoryAPI_*.*.*_Install.zip
RocketDirectoryMod_*.*.*_Install.zip
RocketWebpUtils_*.*.*_Install.zip
xRocketEndInstall.zip
```
## Page Creation
The pages you need in the wesbite depends on what you need.  For this example we will only use a "Home" page and a "List" page.  

- Create a page call "Catalog".  
- The "Home" page, should alreay be in the menu.

More Info: [https://docs.rocket-cds.org/integration/rocketdirectory/menumanipulator](https://docs.rocket-cds.org/integration/rocketdirectory/menumanipulator)

## Setup Modules
- Download AppThemes.  
AppThemes are sets of templates that create the design and admin UI for the website.  
*Usually this is done automatically, but it is good practise to ensure you have the correct templates.*  

[https://docs.rocket-cds.org/integration/installing-appthemes](https://docs.rocket-cds.org/integration/installing-appthemes)

- Add RocketContent to Home Page and pick an AppTheme you want to use.
- Add RocketDirectory to the [CATDIR1] page.  

## Setup Directory System
- Hover over the RocketDirectory Module and click the admin icon.  
 
![](data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAPgAAAAsCAYAAABIfnUyAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsQAAA7EAZUrDhsAAAdpSURBVHhe7Z1ZaBRJGMe/xCh4bBZMjBGD97GJ1y5ZxMQrETxAjQ+6PkSIgiLircQHBUF92Pii4q344IHuk2hEV1ZBo/HKwwbxvogHjhpPliAJuya6+VdXO90zPU6i3W2q+vtBZaq+7hn4p+dfR1dXTcKnRohhGC1JlK8Mw2iIYwteVVVFNTU19PHjRxlRm8TEREpOTqYuXbpQu3btZDRMbW0tvXjxIlCagdu66+rqqG3btrL0feBrbdccZfCrV69SRkYGdezYkZL+eCOjalNfmErv3r2jUChE/fv3t/0TcMHv3bv3WXOrVq3kEbVpaGiIqRkEUXcQNdsMjpYbNUHaX3rUbJG8mpAoau7evXvLSFhzp06dZEQvXr9+HaUZBFF3EDXbxuA4iJpNV6ANGq0EUTPga60fTpptBseYRJduuRPQFjnuQlmXrpoT0OY01gyi7iBq5rvoDKMxbHCG0Rg2OMNoDBucYTSGDc4wGsMGZxiNYYP7xPXr12nNmjW2dOPGDXmUYbzBU4OXJlfQT7/nERWlf1XKLMmnP1Mq5aepzdGjR2nt2rW2dPjwYXmUYbzBU4MvWLCA7t69K0vN586dO7Ro0SJZUpsHDx7IXBinmIocO3aMMjMz6dy5czJClJeXRwkJCU2OIWVlZdHJkyflEcYNPDX4s2fPZO7refjwocypQ2lpKc2cOZMqKipE+fjx43T69GmRt4LYiRMnRB7nzpo1S7xXNcyKHMMOEzPf1BhQtUK/f/8+jRkzht68if8UKM4ZNWqUb99rHoO7DFYs4Qt/4MABysnJoR49elBBQYFYCBDJ27dvafLkyWJxAM7dv3+/eC8+QyWw/BKcP3/+c+uMlnn06NFNjpmoWKHPnTuXysrKhHFfvnwpo9FUV1fTyJEj6cKFCzR79mwZ9RZfDY6Fa9ZkEiuuIuvWraPnz5/LEtGTJ09kLjbWLzXei89QCes1a2qL7RRTlUOHDlH37t1FDyQ3N9fR5IgNHz5c9HRw7sGDB+URb+EW3EUePXpEGzZskKVoME5FC42EdbuxwGc8fvxYltQCrbM5ps7Pz29WTFW6du1Kly5dol69eonKGia3Dk+RRwzHcA7OxXv8gA3uIthZIy0tTZbsFBcX082bN2nbtm0iIb9w4UJ51E5qaqrMqYHqvS43gGEvX7782eRorWFsJORNc+Mcv8wN2OAugq7XtWvXaNy4cTJiMGTIEFq/fr2oAEySkpJo48aN1KdPHxkxwHsxP46xu2pYh1nfklSlc+fOonVG7wxDM4y3YW7k0XuDuXGOn/hqcLNLZiaTWHEVQeu7adMmWTKYOnWq4zrk1q1b0/Tp02XJAO9VrQVX/Zq5SXp6Ol28eFGYHEM2mBv58vJy380NuAX3gPr6epkz+FKr1KZNG5kzcNqcoaXjRqsL3ZhabN++PZ09e1ZG9QC9te+Frwa3dsOsX4pYcRXBPOeKFStkyeDMmTMyF82pU6dkzgBjdacpNd15+vSp2CwQU4Rz5syRUfXA3XJ0zbG5I4ZZSLdu3Yo7heYV3IK7CJ5MGzBgQNRDLeierVq1ytY6f/jwgZYvX05XrlyREQMYftCgQWKDwKCAWYVJkybRypUradmyZeLZABWBgXG33JwKQ1cdKd4UmpewwV0EN9Gwfa0TJSUlNHDgQFq8eDHNmzdP5CPH6iZ4AAbj8yAAc+/YsUPMKkycOJGWLl1KmzdvlkfVIdZUWLwpNK9hg7sInkhbsmSJLEWDWnzr1q20e/du8XhjLNBN79atmyzpi2luE3RjVdQdbyos1hSaH/hq8Fh3y2PFVQRPZVn33Ub3LB49e/aUORK/TrF69WpZ0pdIc8+fP5+2b98uS2pRWFgYdyoMMRwzp9BmzJghj3gLt+Au06FDB9q5c6e4I4zxNZ5IwwKSlJQUeUYYVARYbIJaHRe/qKiIdu3aFfUrJC0ds9WNrKgjk4lO5gZ79uwRXe94U2E4hjH5iBEjaO/evTLqLZ4a3I3ulooPfGDee9++fTRs2DBRnjJlCo0dO1bkrYwfP16MO4G52AQLU1Rjy5YtX3z0NhKdzA369esnxtlNeX4B52CxibXX5iWeGry5Fz6SwYMHK3/xTfr27StzYb7lf9OSQAWGO8eR052RKRIdzN3Ssf02WWVlJf16O0OW9OTvrBBlZ2fLkqHZWvYKbNl05MgRWTKYNm2auJvuNU4a/dL9PYnUGETNPAb3CfRGcAPOmvwwNxNs2OAMozFscIbRGDY4w2gMG5xhNIYNzjAaE2yD15fJDMPoic3gWA1VX6jWbiLNAdqs2yZRUj41NDSIpCvQZtMsQSxougOpWb4KkpOTYy531AFog0aDavF36NChAdIcJljX2iCImm0Gx0qmUChErybo1ZJDCzRBGzQapMtXY7tj7KKiU+0OLdBk1xzGvNZB0h1EzQnZBcWfCn/LpV9+zqHMRk9jp5H37/+hutr/SO3Nk8IkNHZbfmys2SA+1kqt8ttV9MO/NUruieYEumqozb+kGdsj4VdJamqCoztYmon+B4WYaquCVThxAAAAAElFTkSuQmCC)

- Go into "Admin", select AppTheme "Products" and save. *(The system will keep asking for the admin settings if you do not do this.)*
- Add some products.
- Add some categories.
 
