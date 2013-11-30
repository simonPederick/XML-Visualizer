using UnityEngine;
using System.Collections.Generic;


/// <summary>
/// Component to handle a site map viewer object. This object takes a single node object to use as its name and 
/// displays it within the world. When the site map object is clicked, it will generate new site map viewer children 
/// based off the node objects children. It will also load a website or new sitemap if leaf node is selected.
/// </summary>
[RequireComponent (typeof (TextMesh))]
[RequireComponent (typeof (BoxCollider))]
[RequireComponent (typeof (LineRenderer))]
public class SiteMapViewer : MonoBehaviour {
	
	// The maximun number of child nodes it can display at one time on the screen
	static int MAX_CHILD_NODES = 9;
	
	public GUIStyle style;
	
	private Node node;
	private List<GameObject> children = new List<GameObject>();
	private TextMesh textMesh;
	private int itemsOffest;
	private bool isActive = false;
	private SiteMapViewer parent = null;
	private bool displayQuestionBox = false;
	
	/// <summary>
	/// Sets the node that this viewer will use for its display text as well as generating children.
	/// </summary>
	/// <param name='n'>
	/// The node to set.
	/// </param>
	public void setNode(Node n)
	{
		node = n;
		textMesh.text = n.Name;
		GetComponent<BoxCollider>().size = new Vector3(3f * textMesh.text.Length, 7f, 1f);
	}
	
	/// <summary>
	/// Recursive function that will propogate through the viewer node tree to set all node's active state
	/// to false while setting only the specified node's active state to true. Should be called on the root node
	/// to ensure all nodes in tree are checked. Parent node can be retrieved from Manager object.
	/// </summary>
	/// <returns>
	/// Returns true if node is active or child node is active, otherwise returns false.
	/// </returns>
	/// <param name='node'>
	/// The reference of the node that will be set to active.
	/// </param>
	public bool setNodeAsActive(SiteMapViewer node)
	{
		if (this != node) // If not specified node, check set active state to false and check children for return value
		{
			bool childIsActive = false;
			setActive(false);
			// Check to see if any children return as being active, if so, return true
			foreach (GameObject child in children)
			{
				if (child.GetComponent<SiteMapViewer>().setNodeAsActive(node))
					childIsActive = true;
			}
			
			if (!childIsActive)
			{
				OnDestroy();
				return false;
			}
		}
		else // If specified node, set active state to true
			setActive(true);
		
		return true;
	}
	
	/// <summary>
	/// Update this instance. 
	/// Checks for left or right arrow input to cycle through visible child nodes.
	/// </summary>
	private void Update()
	{
		if (isActive && children.Count > 0)
		{
			// Check for left and right arrows for cycling
			if (Input.GetKeyDown(KeyCode.LeftArrow))
			{
				itemsOffest -= MAX_CHILD_NODES;
				if (itemsOffest < 0)
					itemsOffest = node.Children.Count-1;
				updateVisibleChildren();
			}
			else if (Input.GetKeyDown(KeyCode.RightArrow))
			{
				itemsOffest += MAX_CHILD_NODES;
				if (itemsOffest > node.Children.Count)
					itemsOffest = 0;
				updateVisibleChildren();
			}
			else // Check keyboard input
			{
				foreach (char c in Input.inputString)
				{
					if (char.IsLetter(c))
					{
						itemsOffest = findNextChildWithLetter(c);
						updateVisibleChildren();
					}
				}
			}
		}
		
		
	}
	
	/// <summary>
	/// Raises the mouse down event.
	/// Sets the node as active when clicked. Node will also display its children when clicked or 
	/// prompt to load a new url.
	/// </summary>
	private void OnMouseDown()
	{
		if (children.Count > 0)
		{
			if (isActive)
			{
				// Collapse children after being selected a second time
				OnDestroy();
				if (parent != null)
				{
					// Set parent as active node after collapsing
					Manager.instance.RootNode.setNodeAsActive(parent);
					setActive(false);	
				}
			}
			else
				Manager.instance.RootNode.setNodeAsActive(this);
		}
		else
		{
			if (node.Children.Count > 0)
			{
				// Create children when selected for first time
				createChildren();
			}
			else
			{
				// If leaf node, show prompt question box for loading url
				displayQuestionBox = true;
			}
			
			Manager.instance.RootNode.setNodeAsActive(this);
		}
	}
	
	private void OnMouseOver()
	{
		textMesh.color = Color.red;
	}
	
	private void OnMouseExit()
	{
		if (isActive)
			textMesh.color = Color.white;
		else
			textMesh.color = Color.gray;
	}
	
	private void Awake() 
	{
		textMesh = GetComponent<TextMesh>();
		textMesh.text = "Site Map Viewer";
		setActive(false);
	}
	
	// Initilise line renderer to point to parent
	private void Start()
	{
		LineRenderer lineRenderer = GetComponent<LineRenderer>();
		
		if (parent != null)
		{
			lineRenderer.SetColors(Color.white, Color.black);
			lineRenderer.SetVertexCount(2);
			lineRenderer.SetWidth(0.5f, 0.1f);
			lineRenderer.SetPosition(0, transform.position);
			lineRenderer.SetPosition(1, parent.transform.position);
		}
		else
			lineRenderer.enabled = false;
	}
	
	private void OnDestroy()
	{
		if (children.Count > 0)
		{
			foreach (GameObject child in children)
			{
				Destroy(child);
			}
			
			children.Clear();
		}
	}
	
	/// <summary>
	/// Updates children to reflect new parameters such as item offset (when cycling through items with arrow keys)
	/// </summary>
	private void updateVisibleChildren()
	{
		OnDestroy();
		createChildren();
	}
	
	/// <summary>
	/// Creates new siteMapViewer nodes based off the children of the node this siteMapViewer manages.
	/// Generates MAX_CHILD_NODES around the parent node. Other nodes can be reached by cycling with the
	/// keyboard keys
	/// </summary>
	private void createChildren()
	{
		int count;
		
		// If number of child nodes is greater then MAX_CHILD_NODES, specify count with item offset 
		if (node.Children.Count > MAX_CHILD_NODES)
			count = MAX_CHILD_NODES + itemsOffest;
		else
			count = node.Children.Count;
		
		for (int i = itemsOffest; i < count; ++i) // Display certain number of children, beginnning at itemsOffset value
		{
			// Create new nodes evenly in a circle around parent node
			float angle = (360f / MAX_CHILD_NODES) * (i - itemsOffest);
			angle *= Mathf.Deg2Rad;
			
			GameObject newChild = Instantiate(Resources.Load("SiteMapViewer") as GameObject) as GameObject;
			newChild.transform.parent = transform;
			newChild.transform.localPosition = new Vector3(Mathf.Cos(angle) * (40f + textMesh.text.Length/2f), Mathf.Sin(angle) * (40f + textMesh.text.Length/2f), -30f);
			newChild.GetComponent<TextMesh>().color = Color.white;
			newChild.GetComponent<SiteMapViewer>().parent = this;
			
			// Re-map index if it overflows or underflows due to the itemsOffset
			int index = i;
			if (index >= node.Children.Count)
				index -= node.Children.Count;
			else if (index < 0)
				index += node.Children.Count;
			
			newChild.GetComponent<SiteMapViewer>().setNode(node.Children[index]);
			
			children.Add(newChild);
		}
	}
	
	/// <summary>
	/// Sets the nodes active state. If active the camera will focus upon it and it will change to a white colour.
	/// </summary>
	/// <param name='active'>
	/// new active state
	/// </param>
	private void setActive(bool active)
	{
		this.isActive = active;
		if (isActive)
		{
			textMesh.color = Color.white;
			Manager.instance.setCameraFocusPoint(transform.position);
		}
		else
		{
			textMesh.color = Color.gray;
			displayQuestionBox = false;
		}
	}
	
	// Draws question prompt if leaf node has been selected.
	private void OnGUI()
	{
		if (displayQuestionBox)
		{
			style.font.material.color = Color.white;
			style.normal.textColor = Color.white;
			GUI.Box(new Rect(50, 50, 200, 100), "Do you want to load the url: " + node.URL, style);
			
			if (GUI.Button(new Rect(60, 160, 30, 15), "Yes", style))
				Manager.instance.loadURL(node.URL);
		
			if (GUI.Button(new Rect(100, 160, 30, 15), "No", style))
				displayQuestionBox = false;
		}
	}
	
	/// <summary>
	/// Searches the nodes children for the first node that begins with the specified letter then updates
	/// its current children with this entry
	/// </summary>
	/// <param name='letter'>
	/// Input letter to search for
	/// </param>
	/// <returns>
	/// Returns the new item offset of the found entry or zero if it could not be found
	/// </returns>
	private int findNextChildWithLetter(char letter)
	{
		int counter = 0;
		foreach (Node child in node.Children)
		{
			if (child.Name[0] == letter)
				return counter;
			
			counter++;
		}
		
		return 0;
	}
}
