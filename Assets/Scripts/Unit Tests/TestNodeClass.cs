using UnityEngine;
using System.Collections.Generic;

[UTest.Module]
public class TestNodeClass : UTest.IModule {
	
	Node rootNode;
	
	public void WarmUp ()
	{
		rootNode = new Node();
		rootNode.addPath("a/b", "");
		rootNode.addPath("c", "");
		rootNode.addPath("c/e", "");
	}
	
	public void TearDown ()
	{
	}
	
	public void Dispose ()
	{
	}
	
	[UTest.Test]
	public void testNodeHierachy()
	{
		UTest.Assert.Truth(rootNode.Children[0].Name == "a", "Incorrect node hierachy generated");
		UTest.Assert.Truth(rootNode.Children[0].Children[0].Name == "b", "Incorrect node hierachy generated");
		UTest.Assert.Truth(rootNode.Children[1].Name == "c", "Incorrect node hierachy generated");
		UTest.Assert.Truth(rootNode.Children[1].Children[0].Name == "e", "Incorrect node hierachy generated");
	}
	
	[UTest.Test]
	public void testLeafNodeRetrieval()
	{
		List<Node> leafNodes = rootNode.getLeafNodes();
		UTest.Assert.Truth(leafNodes.Count == 2, "Incorrect number of leaf nodes returned");
		UTest.Assert.Truth(leafNodes[0].Name == "b" && leafNodes[1].Name == "e", "Incorrect leaf nodes returned");
	}
}
