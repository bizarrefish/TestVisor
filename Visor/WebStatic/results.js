var lastInterval;	// Id from setInterval

V.resultsView.OnOpen(function() {
	var idctr = 0;
	var resStart = 0;
	var resLimit = 5;
	
	var selectedRunIndex = -1;
	var selectedTestKey = "";
	
	var refreshEverything = function() {}
	
	var RefreshDetails = (function() {
		var lastArtifactNum = -1;
		var lastRunId = -1;
		var lastTestKey = -1;
		
		return function(runId, testKey, result) {
		
			var AddResultDetail = function(name, value, icon)
			{
				DivList_Add("div#testResult", "part-" + (idctr++), name, value, icon)
			}

			var AddArtifact = function(artifactIndex, obj)
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
		
			if(result === undefined) return;
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
				AddArtifact(i, artifactObj);
			}
		
		}
	})();
	
	var RefreshRun = (function() {
		
		var lastRunResultsLength = -1;
		var lastRunId;
		var lastRunStateString;
		
		return function(run) {
		
			var AddTestResult = function(testKey, result, selectedFunc) {
				var id = "result-" + (idctr++)
				var success = result.Result.Success;
				var icon = success ? "tick.png" : (success !== null ? "cross.png" : "wait.gif");
				DivList_Add("div#testResultList", id, testKey, "", icon, function() {
					DivList_Select("div#testResultList", id);
					selectedFunc();
				});
				return id;
			}

			var resultList = "div#testResultList";
			var runStateString = JSON.stringify(run);
			
			if(runStateString !== lastRunStateString) {
				// Clear out the list
				DivList_Clear(resultList);
				
				// Load up the list again
				for(var testKey in run.Results) {
					var result = run.Results[testKey];
					
					// We need to save the test key
					(function(_selectedTestKey) {
						
						var id = AddTestResult(testKey, result, function() {
							
							selectedTestKey = _selectedTestKey;
							refreshEverything();
							
						});
						
						if(selectedTestKey === _selectedTestKey) {
							DivList_Select(resultList, id);
						}
						
					})(testKey);
				}
				lastRunId = run.Id;
				lastRunStateString = runStateString;
			}
		}
	})();
	
	
	var RefreshRunList = (function() {
		var lastStart;
		var lastCurrentRunId;
		
		return function(start, limit, runs, status) {
		
			var AddTestRun = function(obj, selectedFunc, icon) {
				// Give this test run a unique id
				var id = "run-" + (idctr++)
				DivList_Add("div#testRunList", id, obj.Name, obj.desc, icon, function() {
					DivList_Select("div#testRunList", id);
					selectedFunc();
				});
			}
			
			var runList = "div#testRunList";
			var runListDesc = "div#testRunListDesc";
			
			// Refresh the list
			if(start !== lastStart || status.CurrentTestRun !== lastCurrentRunId) {
				// Clear the list
				DivList_Clear(runList);
				
				// If there are things going on, we have stuff waiting
				var queueing = status.CurrentTestRun !== null;
				
				// Set run.icon and run.desc
				for(var i in runs) {
					var run = runs[i];
					
					if(queueing) {
						if(run.Id === status.CurrentTestRun) {
							run.icon = "wait.gif";
							run.desc = status.MicroStatus + "...";
							queueing = false;
						} else {
							run.icon = "pause.png";
							run.desc = "Waiting..."
						}
					} else {
						run.icon = "tick.png";
						run.desc = "";
					}
				}
				
				// Fill with test runs
				for(var i in runs)
				{
					// We need to save runIndex for future updates, even if 'runs' changes.
					(function(runIndex) {
					
						var id = AddTestRun(runs[runIndex], function() {
							selectedRunIndex = runIndex;
							refreshEverything();
						}, runs[runIndex].icon);
						
						// If we selected this one, or one after it.
						if(runIndex === selectedRunIndex) {
							DivList_Select(runList, id);
						}
						
					})(i);
				}
				
				// Let the user know what they're seeing
				$(runListDesc).text("Showing results: " + (start+1) + " to " + (start + limit));
			
				lastStart = start;
				lastCurrentRunId = status.CurrentTestRun;
			}
		}
	})();
	
	(function() {
		var currentRuns = [];
		var currentRunId = -1;
		var status = {};
		
		refreshEverything = function() {
			if(currentRuns !== undefined)
				RefreshRunList(resStart, resLimit, currentRuns, status);
			
			if(currentRuns[selectedRunIndex] !== undefined)
				RefreshRun(currentRuns[selectedRunIndex]);
			
			if(currentRuns[selectedRunIndex] !== undefined &&
				currentRuns[selectedRunIndex].Results[selectedTestKey] !== undefined)
				RefreshDetails(currentRuns[selectedRunIndex].Id, selectedTestKey, currentRuns[selectedRunIndex].Results[selectedTestKey]);
		}
		
		var refreshFunc = function() {};
		
		// Load a run page and update the view.
		var SwitchRunPage = function(start, limit) {
			refreshFunc = function() {
				Results_GetTestResults({
					Skip: start,
					Limit: limit,
					TestPlanId: ""
				}, function(runs) {
				
					Status_GetCurrentStatus({}, function(_status) {
					
						resStart = start;
						resLimit = limit;
						currentRuns = runs;
						status = _status;
						refreshEverything()
					});
				});
			};
			
			refreshFunc();
		}
		
		
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
		
		if(lastInterval !== undefined)
			clearInterval(lastInterval);
		
		lastInterval = setInterval(function() {refreshFunc()}, 1000);
	})();
});