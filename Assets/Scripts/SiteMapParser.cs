using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Xml;

/// <summary>
/// Contains a series of static functions to handle the parsing of a XML sitemap file from the web 
/// and generate a corresponding node hierachy. 
/// </summary>
public class SiteMapParser{
	
	/// <summary>
	/// Loads an xml sitemap file from the specified url, generates a node hierachy
	/// based on the contents, then returns the root node of this hierachy.
	/// </summary>
	/// <returns>
	/// Root node of hierachy of webpages.
	/// </returns>
	/// <param name='url'>
	/// website url that contains the sitemap.
	/// </param> 
	public static Node retrieveNodeHierarchyFromURL(string url)
	{
		XmlDocument doc;
		doc = loadXmlDocument(url);
		return createNodeHierarchyFromStrings(parseXMLDocumentForURLs(doc));
	}
	
	/// <summary>
	/// Checks the url extensions to determine the contents of the url.
	/// </summary>
	/// <returns>
	/// True if the url contains sitemap data, and false if it is a generic webpage.
	/// </returns>
	/// <param name='url'>
	/// The website url to check against.
	/// </param>
	public static  bool urlContainsSitemap(string url)
	{
		string[] split = url.Split('/');
		string endString = split[split.Length-1];
		
		if (endString.EndsWith(".xml.gz"))
			return true;
		else 
			return false;
	}
	
	/// <summary>
	/// Returns a list urls that have had the beginning 'http:' and end extensions removed.
	/// (eg. http://google.com/abc.html becomes google.com/abc)
	/// </summary>
	/// <returns>
	/// List of urls with all extensions removed .
	/// </returns>
	/// <param name='urlList'>
	/// List of url strings to convert.
	/// </param>
	public static List<string> getCleanURLList(List<string> urlList)
	{
		List<string> cleanStrings = new List<string>();
		foreach (string s in urlList)
		{
			string sub;
			
			if (s.EndsWith("xml.gz"))
				sub = s.Substring(0, s.LastIndexOf('/'));
			else
				sub = s.Substring(0, s.LastIndexOf('.'));
			
			if (sub.StartsWith("http://"))
				sub = sub.Substring(7, sub.Length - 7);
			
			cleanStrings.Add(sub);
		}
		
		return cleanStrings;
	}
	
	/// <summary>
	/// Loads the website data from the URL and returns the data within an Xml document.
	/// </summary>
	/// <returns>
	/// Xml Document containing website data.
	/// </returns>
	/// <param name='url'>
	/// The website url to load the necessary data from.
	/// </param>
	private static XmlDocument loadXmlDocument(string url)
	{
		int attemptCount = 0;
		bool failedToLoad;
		XmlDocument xmlDoc = new XmlDocument(); 
		
		do
		{
			failedToLoad = false;
			
			// Attempt to load data from website
			WWW xmlSite = new WWW(url);
			while(!xmlSite.isDone) {}
			
			string error = xmlSite.error;
			if (error != null)
			{
				Debug.LogError(error);
				return null;	
			}
			
			try 
			{
				// Attempt to pass xml file from website
				xmlDoc.LoadXml(xmlSite.text);
			}
			catch (XmlException ex)
			{
				// Loading website data sometimes fails on first attempt, try a total of 10 times to load data
				Debug.LogError("Failed to pass xml file, will attempt again. " + ex.ToString() + "\n Attempt " + (attemptCount+1));
				attemptCount++;
				failedToLoad = true;
				if (attemptCount > 10)
					return null;
			}
			catch (Exception ex)
			{
				Debug.LogError("Failed to load xml document from " + url + ". " + ex.ToString());
				return null;
			}
			
		} while (failedToLoad);
		
		if (attemptCount > 0)
			Debug.Log("Xml passed on attempt " + (attemptCount+1));
		
		return xmlDoc;
	}
	
	/// <summary>
	/// Searches through the sitemap xml document and returns a list of urls from the sitemap.
	/// </summary>
	/// <returns>
	/// List of URL strings.
	/// </returns>
	/// <param name='doc'>
	/// Xml sitemap document to search through.
	/// </param>
	private static List<string> parseXMLDocumentForURLs(XmlDocument doc)
	{
		// Retrieve all urls held within the <loc> tag
		XmlNodeList sitemaps = doc.GetElementsByTagName("loc");
		List<string> urlList = new List<string>();
		
		foreach (XmlNode node in sitemaps)
		{
			urlList.Add(node.InnerText);
		}
		
		// Ensure list is alphabetically sorted
		urlList.Sort();
		return urlList;
	}
	
	/// <summary>
	/// Generates a node hierachy based off the supplied strings using the '/' delimenator to signify 
	/// a child node.
	/// </summary>
	/// <returns>
	/// The root node of the hierachy.
	/// </returns>
	/// <param name='urlList'>
	/// The url string list to generate the hierachy from.
	/// </param>
	private static Node createNodeHierarchyFromStrings(List<string> urlList)
	{
		List<string> stringList = getCleanURLList(urlList);
		Node rootNode = new Node();
		
		// Add all url directories to current node hieracahy
		for (int i = 0; i < stringList.Count; ++i)
			rootNode.addPath(stringList[i], urlList[i]);
		
		// Remove any parent nodes with only one child
		while (rootNode.Children.Count == 1)
			rootNode = rootNode.Children[0];
		
		return rootNode;
	}
}
