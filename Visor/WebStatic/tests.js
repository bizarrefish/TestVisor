V.testView.OnOpen(function() {
	var idctr = 0;
	var testList = "div#testList"
	var testTypeMapCached;
	
		
	var RefreshTestTypes = function(nextFunc) {
		Tests_GetTestTypes({}, function(testTypes) {
			
			var sel = "select#testTypeSelect";
			// Refresh the test type select
			$(sel + " > option").remove();
			
			
			testTypeMapCached = {}
			for(var i in testTypes) {
				var tt = testTypes[i];
				
				// do the extensions
				var eStr = "";
				var first = true;
				for(var j in tt.FileExtensions) {
					var ext = tt.FileExtensions[j];
					
					if(!first) eStr = eStr + ", ";
					
					eStr = eStr + ext;
					first = false;
				}
				
				$(sel).append("<option id=\"" + tt.Id + "\">" + tt.Name + " (" + eStr + ")</option>")
				
				testTypeMapCached[tt.Id] = tt;
			}
			
			
			nextFunc();
		});
	}

	
	// Parameter is test type id -> test type obj
	var RefreshTests = function(testTypeMap) {
		
		Tests_GetTests({}, function(tests) {
		
			DivList_Clear(testList);
			
			for(var i in tests) {
				var t = tests[i];
				DivList_Add(testList, "test-" + (idctr++), t.Name, testTypeMap[t.TestTypeId].Name);
			}
			
		});
	}
	
	
	// Here we go...
	var f = function(n) { n(); };
	if(testTypeMapCached === undefined) {
		f = RefreshTestTypes;
	}
	
	f(function() {
		RefreshTests(testTypeMapCached);
	});
});