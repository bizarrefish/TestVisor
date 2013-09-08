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
				
				$(sel).append("<option value=\"" + tt.Id + "\">" + tt.Name + " (" + eStr + ")</option>")
				
				testTypeMapCached[tt.Id] = tt;
			}
			
			
			nextFunc();
		});
	}

	
	var testListState = null;
	
	// Parameter is test type id -> test type obj
	var RefreshTests = function(testTypeMap) {
		
		Tests_GetTests({}, function(tests) {
		
			DivList_Clear(testList);
			
			var data = [];
			for(var i in tests) {
				var t = tests[i];
				data.push({
					id: t.Name,
					title: t.Name,
					description: testTypeMap[t.TestTypeId].Name,
					updateToken: t.Name
				});
			}
			
			testListState = DivList_Update(testListState, testList, data);
			
		});
	}
	
	var DoTestUpload = function() {
	
		var testTypeId = $("#testTypeSelect option:selected").attr('value');
		
		var name = window.prompt("Name of this test", "");
	
		Tests_GetTestUploadUrl({
			TypeId: testTypeId,
			Name: name
		}, function(url) {
			var frm = document.getElementById("testUploadForm");
			frm.action = url
			frm.submit();
		});
	}
	
	$("#testUploadButton").unbind('click').click(function() {
		DoTestUpload();
	});
	
	
	// Here we go...
	var f = function(n) { n(); };
	if(testTypeMapCached === undefined) {
		f = RefreshTestTypes;
	}
	
	f(function() {
		RefreshTests(testTypeMapCached);
	});
});