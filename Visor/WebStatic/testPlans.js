V.testPlanView.OnOpen(function() {
	var idctr = 0;
	
	var currentTestPlanId;
	
	function LoadTestPlanSource(id) {
		Plans_GetSource({TestPlanId: id}, function(source) {
			editAreaLoader.setValue("testPlanEditorArea", source)
			currentTestPlanId = id;
		});
		
		DivList_Select("div#testPlanList", "plan-" + id);
	}
	
	function SaveTestPlanSource() {
		Loading_Start();
		Plans_SetSource({
			TestPlanId: currentTestPlanId,
			Source: editAreaLoader.getValue("testPlanEditorArea")
		}, function() {
			//alert("Saved");
			Loading_Stop();
		});
	}
	
	$("#testPlanDeleteButton").unbind('click').click(function() {
		Plans_Delete({
			TestPlanId: currentTestPlanId
		}, function() {
			RefreshTestPlanList();
		})
	});
	
	$("#testPlanSaveButton").unbind('click').click(SaveTestPlanSource);
	
	$("#testPlanRunButton").unbind('click').click(function() {
		Plans_Start({
			TestPlanId: currentTestPlanId,
			Arguments: {}
		}, function() {
		
		});
	});
	
	function AddPlan(testPlan) {
		DivList_Add("div#testPlanList", "plan-" + testPlan.Id, testPlan.Name, testPlan.Description, "visor.png", function() {
			$("#testPlanEditor > .heading").text(testPlan.Name);
			LoadTestPlanSource(testPlan.Id);
		});
	}
	
	function NewTestPlan() {
		var name = window.prompt("Name of new test plan", "");
		if(name !== "") {
			Plans_Create({
				Info: { Name: name, Description: name }
			}, function() {
				RefreshTestPlanList();
			});
		}
	}
	
	editAreaLoader.init({
		id : "testPlanEditorArea"		// textarea id
		,syntax: "js"			// syntax to be uses for highgliting
		,start_highlight: true		// to display with highlight mode on start-up
	})
	
	function RefreshTestPlanList() {
	
		DivList_Clear("div#testPlanList");
		Plans_GetTestPlans({}, function(testPlans) {
			for(var i in testPlans) {
				var obj = testPlans[i];
				AddPlan(obj);
			}
			DivList_Add("div#testPlanList", "addnewplan", "Add New...", "Create a new test plan", "plus.png", NewTestPlan);
		});
	}
	
	RefreshTestPlanList();
});