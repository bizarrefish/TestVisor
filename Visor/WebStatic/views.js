/*
	Trying to deal with paginated views
*/

// Initialize views and navbar
// ({ ViewName: [moduleId], ... })
var Views_Init;

// Register a function to call when ViewName is opened
// (ViewName, func)
var Views_OpenFunction;

(function() {
	var idctr = 0;
	var openFunctions = {};
	
	Views_Init = function(views) {
		var navs = []
		for(var name in views) {
			
			(function() {
				var modList = views[name];
				var modName = name;
				
			
				navs.push({ Name: modName, Id: "nav-" + (idctr++), Function: function() {
					Module_SetDisplayed(modList);
					$("span#headerText").text(modName);
					if(openFunctions.hasOwnProperty(modName)) openFunctions[modName]();
				}});
			})();
		}
		
		NavBar_Init("div.navBar", navs);
	}
	
	Views_OpenFunction = function(viewName, func) {
		openFunctions[viewName] = func;
	};
})();

