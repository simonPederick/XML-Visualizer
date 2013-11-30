using UnityEngine;
using System.Collections;

/// <summary>
/// Handles the overall running of the application, loading new sitemap data or websites, as well
/// as controlling the camera.
/// </summary>
public class Manager : MonoBehaviour {

	// The first url to load when the program begins
	static string ROOT_URL = "http://www.yellowpages.com.au/sitemap.xml.gz";
	
	private SiteMapViewer rootViewerNode = null;
	private Vector3 cameraFocusPoint;
	private Vector3 cameraStartPosition;
	private bool atHome = true;
	
	// Allow global access to manager
	public static Manager instance;
	
	/// <summary>
	/// Returns the root node to the current site map viewer hierachy.
	/// </summary>
	/// <value>
	/// The root node.
	/// </value>
	public SiteMapViewer RootNode {get {return rootViewerNode;}}
	
	/// <summary>
	/// Sets a new camera focus point that the camera will then smoothly focus upon.
	/// </summary>
	/// <param name='point'>
	/// The new vector for the focus point.
	/// </param>
	public void setCameraFocusPoint(Vector3 point)
	{
		cameraFocusPoint = point;
	}
	
	/// <summary>
	/// If the url contains a sitemap, a new sitemap viewer will be loaded and generated with 
	/// necessary nodes. If the url contains a simple website, a request is made to open the website.
	/// </summary>
	/// <param name='url'>
	/// The website url to load.
	/// </param>
	public void loadURL(string url)
	{
		if (SiteMapParser.urlContainsSitemap(url)) 
		{
			// Load new sitemap if present in requested url
			loadNewSitemap(url);
			
			if (url != ROOT_URL)
				atHome = false;
		}
		else
		{
			// Externally load new page if requested url links to .html website
			Application.ExternalEval("window.open("+url+");");
		}
	}
	
	// To begin, load root url 
	private void Start()
	{
		instance = this;
		loadNewSitemap(ROOT_URL);
	}
	
	// Update cameras position based on focus point
	private void Update()
	{
		transform.position = Vector3.Lerp(transform.position, cameraFocusPoint + cameraStartPosition, 0.1f);
		
		if (Input.GetKeyDown(KeyCode.Escape))
			Application.Quit();
	}
	
	/// <summary>
	/// Loads a new sitemap from a url and generates a new sitemap viewer to display information.
	/// </summary>
	/// <param name='url'>
	/// The URL to load the sitemap from
	/// </param>
	private void loadNewSitemap(string url)
	{
		if (rootViewerNode != null)
		{
			Destroy(rootViewerNode.gameObject);
			rootViewerNode = null;
		}
		
		GameObject newViewer = Instantiate(Resources.Load("siteMapViewer") as GameObject, Vector3.zero, Quaternion.identity) as GameObject;
		rootViewerNode = newViewer.GetComponent<SiteMapViewer>();
		rootViewerNode.GetComponent<TextMesh>().text = "loading";
		
		cameraStartPosition = new Vector3(0, 0, -30);
		cameraFocusPoint = Vector3.zero;
		
		Node newNode = SiteMapParser.retrieveNodeHierarchyFromURL(url);
		if (newNode != null)
			rootViewerNode.setNode(newNode);
	}
	
	// Presents home button if currently in child sitemap.
	private void OnGUI()
	{
		if (!atHome)
		{
			if (GUI.Button(new Rect(Screen.width - 100, 50, 80, 50), "Home"))
			{
				atHome = true;
				loadURL(ROOT_URL);
			}
		}
	}
}
