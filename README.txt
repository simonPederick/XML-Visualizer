Allows you to view a 3d representation of a breakdown of the sitemap data found at http://www.yellowpages.com.au/sitemap.xml.gz.
Currently the application only works as an executable and not as a webplayer due to unitys built in WWW security 
features that prevent downloading data from other sites when run as a webplayer (http://docs.unity3d.com/Documentation/Manual/SecuritySandbox.html)
Due to this, player cannot load external .html files in a web browser as this requires a javascript request.

Source files are found in Assets/Scripts/

Controls:
Click on nodes to expand
Click again on a node to collapse
When a node is expanded, use left and right arrow keys to cycle through node children on display.
You may also press a letter key and node will find and display first child beginning with that letter.

If selecting a leaf node you will be prompted to load the leaf nodes URL. If the url contains another sitemap
a new sitemap will be generated in the program (you may press the home button to return to the root sitemap)
If the leaf node contains a .html website, the website will be loaded in a browser (only in webplayer mode)

Press escape to quite the program.
