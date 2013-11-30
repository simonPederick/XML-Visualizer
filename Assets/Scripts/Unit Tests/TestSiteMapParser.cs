using UnityEngine;
using System.Collections.Generic;


[UTest.Module]
public class TestSiteMapParser : UTest.IModule {
	string testDataURL = "file://C:/Users/Simon/Projects/Site Map Visualizer/Assets/Resources/testData.xml";
	
	public void WarmUp ()
	{
	}
	
	public void TearDown ()
	{
	}
	
	public void Dispose ()
	{
	}
	
	[UTest.Test]
	public void testRetrieveNodeHierachy()
	{
		Node rootNode = SiteMapParser.retrieveNodeHierarchyFromURL(testDataURL);
		UTest.Assert.Truth(rootNode != null, "Node hierachy not loaded");
		UTest.Assert.Truth(rootNode.Children.Count == 4, "Incorrect number of children nodes generated");
		UTest.Assert.Truth(rootNode.Children[0].Name == "map" && rootNode.Children[1].Name == "nt"
			&& rootNode.Children[2].Name == "wa" && rootNode.Children[3].Name == "website", "Incorrect children nodes generated or incorrect order");
	}
	
	[UTest.Test]
	public void testCleanURLs()
	{
		List<string> urls = new List<string>();
		urls.Add("a.html");
		urls.Add("b/sitemap.xml.gz");
		urls.Add("http://c.html");
		
		List<string> cleanList = SiteMapParser.getCleanURLList(urls);
		UTest.Assert.Truth(cleanList[0] == "a", "Did not correctly remove .html extension");
		UTest.Assert.Truth(cleanList[1] == "b", "Did not correctly remove sitemap.xml.gz extension");
		UTest.Assert.Truth(cleanList[2] == "c", "Did not correctly remove http://");
	}
}
