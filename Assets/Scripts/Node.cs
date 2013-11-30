using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Implements a basic tree structure where each node maintains a list of its children nodes. Each node
/// also holds its name and if it is a leaf node, a full URL. Class exposes methods to generate a node 
/// heirachy based off a list of paths using the '/' delimenator.
/// </summary>
[System.Serializable]
public class Node{
	private string name;
	private string fullURL;
	private List<Node> children = new List<Node>();
	
	/// <summary>
	/// Returns name of node
	/// </summary>
	/// <value>
	/// The name.
	/// </value>
	public string Name {get {return name;} set {name = value;}}
	
	/// <summary>
	/// If leaf node, returns full URL for node, otherwise returns null.
	/// </summary>
	/// <value>
	/// The full URL.
	/// </value>
	public string URL {get {return fullURL;} set {fullURL = value;}}
	
	/// <summary>
	/// Returns current children for node
	/// </summary>
	/// <value>
	/// The children.
	/// </value>
	public List<Node> Children {get {return children;}}
	
	/// <summary>
	/// Adds one or multiple child nodes based off the given path to the parent node using the '/' delimenator.
	/// For example, a/b/c generates three nodes with 'c' being a child of 'b' being a child of 'a'.
	/// </summary>
	/// <param name='path'>
	/// The path to generate a  set of nodes from
	/// </param>
	/// <param name='url'>
	/// The full url to give to the final child node of the path.
	/// </param>
	public void addPath(string path, string url)
	{
		Node currentNode = this;
		string[] pieces = path.Split('/');
		
		foreach (string piece in pieces)
		{
			Node child = currentNode.findNameInChildren(piece);
			
			// If child with name does not exist, create it
			if (child == null)
			{
				child = new Node();
				child.name = piece;
				child.fullURL = url;
				currentNode.children.Add(child);
				currentNode.fullURL = null;
			}
			
			// Set current node to child to add further child nodes as we propogate through the string
			currentNode = child;
		}
	}
	
	/// <summary>
	/// Returns a list of leaf nodes that can be reached from the current node.
	/// </summary>
	/// <returns>
	/// List of leaf nodes.
	/// </returns>
	public List<Node> getLeafNodes()
	{
		List<Node> leafNodes = new List<Node>();
		if (children.Count > 0)
		{
			foreach (Node child in children)
				leafNodes.AddRange(child.getLeafNodes());
		}
		else
		{
			leafNodes.Add(this);
		}
		
		return leafNodes;
	}
			
	/// <summary>
	/// Finds child node in immediate children with given name.
	/// </summary>
	/// <returns>
	/// Child node with given name or null if child does not exist.
	/// </returns>
	/// <param name='name'>
	/// Name of child node.
	/// </param>
	private Node findNameInChildren(string name)
	{
		foreach (Node n in children)
		{
			if (n.name == name)
				return n;
		}
		
		return null;
	}
}
