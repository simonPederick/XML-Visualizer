using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class UTestEditor : EditorWindow
{
	UTest.TestsService m_service = UTest.TestsService.GetTestService( typeof(UTestEditor).Assembly,typeof(UTestAssemblyMarquer).Assembly );
	Fold m_folds;
	Vector2 m_scroll;
	InformationCollectorReporter m_lastCollectorReporters;
	const string m_prefsFileName = "Assets/UTest/.utestdata";
	
	public UTestEditor()
	{
		Dictionary<string,StateCacheLine> stateCache = new Dictionary<string, StateCacheLine>();
		
		if(System.IO.File.Exists(m_prefsFileName))
		{
			string[] values = System.IO.File.ReadAllLines(m_prefsFileName);
			
			foreach(string val in values)
			{
				var splited = val.Split('|');
				stateCache.Add(splited[0],new StateCacheLine(splited[1]));
			}
		}
		
		m_folds = new Fold(m_service,ReporterCallback,stateCache,null);
	}

	
	~UTestEditor()
	{
		SaveTofile();
	}
	
	
	void SaveTofile ()
	{
		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		m_folds.GetRepresentation(sb);
		var str = sb.ToString();
		
		System.IO.File.WriteAllText(m_prefsFileName,str);
	}
	
	void OnDestroy()
	{
		SaveTofile ();
	}
	
	[MenuItem ("Window/UTest/Run all Tests %#t")]
	static void RunAllTests()
	{
		
		UTest.TestsService service = UTest.TestsService.GetTestService(typeof(UTestEditor).Assembly, typeof(UTestAssemblyMarquer).Assembly);
		UTest.ITestResult finalResult = UTest.TestResultFactory.Accumulate( service, false );
		
		finalResult.Report( new ConsoleReporter() );
		Debug.Log( string.Format( "TestsResult: Successes={1}, Failures={0}", finalResult.Failures, finalResult.Successes ) );
	}
	
	[MenuItem ("Window/UTest/Open View %t")]
	static void Open()
	{
		EditorWindow.GetWindow( typeof(UTestEditor) );
	}

	void OnGUI()
	{
		//m_folds.SearchString = GUILayout.TextField(m_folds.SearchString);
		
		DrawLeftPanel();
		
		
		if(m_lastCollectorReporters!=null)
		{
			if(m_lastCollectorReporters.FailureReport!=null)
			{
				ShowFailureReport(m_lastCollectorReporters.FailureReport);
				
				if( GUILayout.Button( "Open Offender" ) )
				{
					OpenAsset( m_lastCollectorReporters.FailureReport.File, m_lastCollectorReporters.FailureReport.Line );
				}
			}
			else
			{
				var normalColor = GUI.color;
				GUI.color = Color.green;
				GUILayout.Label( "Successful tests: " + m_lastCollectorReporters.m_success);
				GUI.color = normalColor;
			}
								
		}
		
		if(GUILayout.Button("Run Selection"))
		{
			var result = m_folds.RunSelecteds();
			m_lastCollectorReporters = new InformationCollectorReporter();
			
			result.Report(m_lastCollectorReporters);
		}
		
		if(GUILayout.Button("Clear Selection"))
		{
			m_folds.ClearSelections();
		}
	}
	
	void Update()
	{
		Repaint();
	}
	
	void DrawLeftPanel()
	{
		//Si
		m_scroll = GUILayout.BeginScrollView(m_scroll,GUILayout.MaxHeight(maxSize.y/2));
		m_folds.Draw(false);
		GUILayout.EndScrollView();
	}

	void ReporterCallback(InformationCollectorReporter reporter)
	{
		m_lastCollectorReporters = reporter;
	}		
	
	void OpenAsset( string assetPath, int line )
	{
		assetPath = assetPath.Replace( "\\", "/" );
		assetPath = assetPath.Replace( Application.dataPath, "Assets" );
		Object file = AssetDatabase.LoadMainAssetAtPath( assetPath );
		AssetDatabase.OpenAsset( file, line );
	}
	
	void ShowFailureReport( UTest.FailureReport report )
	{
		var normalColor = GUI.color;
		GUI.color = Color.red;

		string show = string.Format( "A test has failed!\n TestModule: {0}\n TestName: {1}\n At line {2} of file {3}\n\n{4}"
		                            	, report.ModuleName, report.TestName, report.Line, report.File,report.ToString() );

		if( report is UTest.AssertFailureReport )
			show = string.Format( "A test has failed!\n TestModule: {0}\n TestName: {1}\n AssertType: {2} \n AssertMessage: {3}\n\n At line {4} of file {5}", report.ModuleName, report.TestName,
			                     ((UTest.AssertFailureReport)report).AssertType, ((UTest.AssertFailureReport)report).AssertMsg, report.Line, report.File );
		GUILayout.TextArea( show );
		GUI.color = normalColor;
	
	}
}

class StateCacheLine
{
	public StateCacheLine(string serialized)
	{
		string[] vals = serialized.Split(':');
		
		Fold = vals[0] == "True";
		Selected = vals[1] == "True";
	}
	
	public bool Fold{get; private set;}
	public bool Selected{get; private set;}
}

class Fold
{
	List<Fold> m_folds = new List<Fold>();
	UTest.ITestable m_testable;
	bool m_fold = false;
	System.Action<InformationCollectorReporter> m_reporterCallback;
	
	GUIContent m_content;
	bool m_selected = false;
	Fold m_parent;
	
	public Fold(UTest.ITestable testables,System.Action<InformationCollectorReporter> callback,Dictionary<string,StateCacheLine> stateCache,Fold parent)
	{
		m_reporterCallback = callback;
		m_parent = parent;
		GenerateTree(testables, stateCache);
		
		StateCacheLine cacheLine;
		
		if(stateCache.TryGetValue(SHA1Hash(), out cacheLine))
		{
			m_fold = cacheLine.Fold;
			m_selected = cacheLine.Selected;
		}
	}
	
	void GenerateTree(UTest.ITestable testable,Dictionary<string,StateCacheLine> stateCache)
	{
		IEnumerable<UTest.ITestable> enumerator = testable;
		m_testable = testable;
		string desc = string.IsNullOrEmpty(m_testable.Description)?"No Description provided":m_testable.Description;
		
		desc = m_testable.Name+"\n"+desc;
			
		m_content = new GUIContent(m_testable.Name,desc);
		
		foreach(UTest.ITestable test in enumerator)
		{
			m_folds.Add(new Fold(test,m_reporterCallback,stateCache,this));
		}
	}
	
	public UTest.ITestResult RunSelecteds()
	{
		if(m_selected)
		{
			UTest.ITestResult finalResult = UTest.TestResultFactory.Accumulate( m_testable, false );
			
			return finalResult;
		}
		else
		{
			UTest.ITestResult finalResult =  UTest.TestResultFactory.GetNeutral();
			
			foreach(Fold fold in m_folds)
			{
				finalResult = finalResult.Accumulate(fold.RunSelecteds());
			}
			
			return finalResult;
		}
	}
	
	string SHA1Hash()
	{
		string input = (m_parent!=null)?m_testable.Name+m_parent.m_testable.Name:m_testable.Name;
		
		var sha = new System.Security.Cryptography.SHA1CryptoServiceProvider();
		byte[] data = System.Text.Encoding.ASCII.GetBytes(input);
		byte[] hash = sha.ComputeHash(data);
		
		var sb = new System.Text.StringBuilder();
		for (int i = 0; i < hash.Length; i++)
		{
		  sb.Append(hash[i].ToString("X2"));
		}
		
		return sb.ToString();
	}
	
	public void GetRepresentation(System.Text.StringBuilder sb)
	{
		string sha1 = SHA1Hash();
		
		sb.AppendLine(sha1+"|"+m_fold+":"+m_selected);
		
		foreach(Fold fold in m_folds)
		{
			fold.GetRepresentation(sb);
		}
	}
	
	public void ClearSelections()
	{
		m_selected = false;
		
		foreach(Fold fold in m_folds)
		{
			fold.ClearSelections();
		}
	}
	
	public void Draw(bool parentIsSelected)
	{
		//bool inSearch = string.IsNullOrEmpty(SearchString)?false: m_testable.Name.Contains(SearchString) || m_testable.Description.Contains(SearchString);
		
		EditorGUILayout.BeginHorizontal();
				
			if(GUILayout.Button("Run",GUILayout.MaxWidth(50f)))
			{
				UTest.ITestResult finalResult = UTest.TestResultFactory.Accumulate( m_testable, false );
				
				InformationCollectorReporter reporter = new InformationCollectorReporter();
			
				finalResult.Report(reporter);
				m_reporterCallback(reporter);
			}
			
			if(parentIsSelected)
				GUILayout.Toggle(true,"",GUILayout.ExpandWidth(false));
			else
				m_selected = GUILayout.Toggle(m_selected,"",GUILayout.ExpandWidth(false));
		
			m_fold = EditorGUILayout.Foldout(m_fold, m_content);	
		
		
		EditorGUILayout.EndHorizontal();
		
		
		
		if(m_fold)
		{
			EditorGUI.indentLevel ++;
			foreach(Fold fold in m_folds)
			{
				fold.Draw(m_selected || parentIsSelected);
			}
			EditorGUI.indentLevel --;
		}
		
	}	
}

class InformationCollectorReporter: UTest.IReporter
{
	public UTest.FailureReport FailureReport{ get; set; }
	public int m_success;
	#region IReporter implementation
	bool UTest.IReporter.ReportSuccess( string moduleName, string TestName )
	{
		++m_success;
		return true;
	}

	bool UTest.IReporter.ReportFailure( UTest.AssertFailureReport report )
	{
		FailureReport = report;
		return false;
	}

	public bool ReportFailure( UTest.UnhandledExceptionFailureReport report )
	{
		FailureReport = report;
		return false;
	}
	#endregion
	
}

class ConsoleReporter: UTest.IReporter
{

	bool UTest.IReporter.ReportSuccess( string moduleName, string TestName )
	{
		return true;
	}

	bool UTest.IReporter.ReportFailure( UTest.AssertFailureReport report )
	{
		Debug.Log( "An assert failure! Module: " + report.ModuleName + " Test: " + report.TestName + " line: " + report.Line + " file: " + report.File );
		return true;
	}

	public bool ReportFailure( UTest.UnhandledExceptionFailureReport report )
	{
		Debug.Log( "An unhandled exception failure!" );
		return true;
	}

	
}