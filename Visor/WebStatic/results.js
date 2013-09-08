var lastInterval;	// Id from setInterval

V.resultsView.OnOpen(function() {
	var idctr = 0;
	var resStart = 0;
	var resLimit = 10;
	
	var selectedRunIndex = -1;
	var selectedTestKey = "";
	
	var refreshEverything = function() {}
	
	
	var artifactListState = null;
	var RefreshDetails = (function() {
		var lastArtifactNum = -1;
		var lastRunId = -1;
		var lastTestKey = -1;
		
		return function(runId, testKey, result) {
		
			var AddResultDetail = function(name, value, icon)
			{
				DivList_Add("div#testResult", "part-" + (idctr++), name, value, icon)
			}

		
			if(result === undefined) return;
			var resultDetails = "div#testResult";
			var artifacts = "div#artifactList";
			
			var resultHeading = "div#resultHeading";
			var artifactHeading = "div#artifactHeading";
			
			DivList_Clear(resultDetails);
			
			$(resultHeading).text(testKey + " Details");
			$(artifactHeading).text(testKey + " Artifacts");
			
			
			var IncludeDetail = function(prop, desc) {
				if(result.Result[prop] !== null) {
					AddResultDetail(desc, result.Result[prop]);
				}
			}
			IncludeDetail("Success", "Success");
			IncludeDetail("ExecutionTime", "Execution Time");
			IncludeDetail("StandardOutput", "Standard Output");
			IncludeDetail("StandardError", "Standard Error");
			
			var resultData = [];
			for(var i in result.Artifacts) {
				var artifactObj = result.Artifacts[i];
				var objectId = {runId: runId, testKey: testKey, index: i, Url: artifactObj.DownloadUrl};
				resultData.push({
					id: objectId,
					title: artifactObj.Name,
					description: artifactObj.FileName,
					updateToken: runId + ":" + testKey + ":" + i
				});
			}
			
			artifactListState = DivList_Update(artifactListState, artifacts, resultData, function(oId) {
				var frm = document.getElementById("hiddenForm");
				frm.action = oId.Url;
				frm.method = "GET";
				frm.submit();
			});
		
		}
	})();
	
	var resultListState = null;
	var RefreshRun = function(run) {
		var resultList = "div#testResultList";
		var data = [];
		for(var testKey in run.Results) {
			var result = run.Results[testKey];
			var success = result.Result.Success;
			var icon = success ? "tick.png" : (success !== null ? "cross.png" : "wait.gif");
			data.push({
				id: testKey,
				title: testKey,
				description: "",
				icon: icon,
				updateToken: (success !== null ? success.toString() : "?") + ":" + testKey + ":" + run.Id
			});
		}
		
		resultListState = DivList_Update(resultListState, resultList, data, function(testKey) {
			selectedTestKey = testKey;
			refreshEverything();
			return true;
		});
	}
	
	
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
				var queueing = status.CurrentTestRun !== null && runs[0].Id >= status.CurrentTestRun;
				
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
		
		SwitchRunPage(0, resLimit);
		
		if(lastInterval !== undefined)
			clearInterval(lastInterval);
		
		lastInterval = setInterval(function() {refreshFunc()}, 1000);
	})();
});