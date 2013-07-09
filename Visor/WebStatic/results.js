V.resultsView.OnOpen(function() {
	var idctr = 0;
	var resStart = 0;
	var resLimit = 5;
	
	
	// View state
	
	var AddResultDetail = function(name, value, icon)
	{
		DivList_Add("div#testResult", "part-" + (idctr++), name, value, icon)
	}
	
	var AddArtifact = function(runId, testKey, artifactIndex, obj)
	{
		DivList_Add("div#artifactList", "artifact-" + (idctr++), obj.Name, obj.FileName, undefined, function() {
			Results_GetArtifactUrl({
				RunId: runId,
				TestKey: testKey,
				ArtifactIndex: artifactIndex
			}, function(url) {
				var frm = document.getElementById("hiddenForm");
				frm.action = url;
				frm.method = "GET";
				frm.submit();
			});
		});
	}
	
	var lastArtifactNum = -1;
	var lastRunId = -1;
	var lastTestKey = -1;
	
	var RefreshDetails = function(runId, testKey, result) {

		var resultDetails = "div#testResult";
		var artifacts = "div#artifactList";
		
		var resultHeading = "div#resultHeading";
		var artifactHeading = "div#artifactHeading";
		
		DivList_Clear(resultDetails);
		DivList_Clear(artifactList);
		
		$(resultHeading).text(testKey + " Details");
		$(artifactHeading).text(testKey + " Artifacts");
		
		AddResultDetail("Success", result.Result.Success);
		AddResultDetail("Execution Time", result.Result.ExecutionTime);
		
		for(var i in result.Artifacts) {
			var artifactObj = result.Artifacts[i];
			AddArtifact(runId, testKey, i, artifactObj);
		}
	
	}
	
	
	var AddTestResult = function(testKey, result, selectedFunc) {
		var id = "result-" + (idctr++)
		var success = result.Result.Success;
		var icon = success ? "tick.png" : (success !== null ? "cross.png" : "wait.png");
		DivList_Add("div#testResultList", id, testKey, testKey, icon, function() {
			DivList_Select("div#testResultList", id);
			selectedFunc();
		});
		return id;
	}
	
	var lastRunResultsLength = -1;
	
	// Returns true if details are different from what was there previously
	var detailsNew = function(newDetails) { return true; }
	
	var refreshCurrentResults = function() { }
	
	var RefreshRun = function(run) {

		var resultList = "div#testResultList";
	
		// Clear out the list
		DivList_Clear(resultList);
		
		// Load up the list again
		for(var testKey in run.Results) {
			var result = run.Results[testKey];
			
			// We need to save the test key
			(function(selectedTestKey) {
				
				var id = AddTestResult(testKey, result, function() {
					
					refreshCurrentResults = function() {
						RefreshDetails(run.Id, selectedTestKey, run.Results[selectedTestKey]);
					};
					
					refreshCurrentResults();
				});
				
			})(testKey);
		}
		
		refreshCurrentResults();
	}
	
	var latestRunId;
	
	var AddTestRun = function(obj, selectedFunc) {
		// Give this test run a unique id
		var id = "run-" + (idctr++)
		DivList_Add("div#testRunList", id, obj.Name, obj.Description, "tick.png", function() {
			DivList_Select("div#testRunList", id);
			selectedFunc();
		});
	}

	var refreshTestRun = function() {}
	
	var refreshCurrentRun = function() { }
	
	var RefreshRunList = function(start, limit, runs) {
		var runList = "div#testRunList";
		var runListDesc = "div#testRunListDesc";
		
		// Clear the list
		DivList_Clear(runList);
		
		// Fill with test runs
		for(var i in runs)
		{
			// We need to save runIndex for future updates, even if 'runs' changes.
			(function(runIndex) {
			
				AddTestRun(runs[runIndex], function() {
					refreshCurrentRun = function() {
						RefreshRun(runs[runIndex]);
					}
					
					refreshCurrentRun();
				});
				
			})(i);
		}
		
		
		// Let the user know what they're seeing
		$(runListDesc).text("Showing results: " + (start+1) + " to " + (start + limit));
		
		refreshCurrentRun();
	}
	
	
	var refreshFunc;
	
	// Load a run page and update the view.
	var SwitchRunPage = function(start, limit) {
		refreshFunc = function() {
			Results_GetTestResults({
				Skip: start,
				Limit: limit,
				TestPlanId: ""
			}, function(runs) {
				selectedRunIndex = 0;
				RefreshRunList(start, limit, runs);
			});
		};
		
		refreshFunc();
	}
	
	var resStart = 0;
	var resLimit = 5;
	
	
	$("input#nextResults").unbind("click").click(function() {
		resStart = resStart + resLimit;
		SwitchRunPage(resStart, resLimit);
	});
	
	$("input#prevResults").unbind("click").click(function() {
		var newStart = resStart - resLimit;
		if(newStart < 0) newStart = 0;
		SwitchRunPage(newStart, resLimit);
	});
	
	SwitchRunPage(0,5);
	
	//setInterval(function() {refreshFunc()}, 3000);
	$("header > h1 > img").click(function() {
		refreshFunc();
	});
});