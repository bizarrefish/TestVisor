/*
	Trying to deal with paginated views
*/

// Initialize views and navbar
// ({ ViewName: viewId, ... })
var Views_Init;

// Register a function to call when ViewName is opened
// (ViewName, func)
var Views_OpenFunction;

(function() {
	var idctr = 0;
	var openFunctions = {};
	
	Views_Init = function() {
		var navs = []
		var viewDivs = $(".view").each(function() {
			var d = $(this);
			var id = d.attr("id");
			var name = d.children(".heading").text();
			d.hide();
			navs.push({ Name: name, Id: "nav-" + id, Function: function() {
				viewDivs.hide().promise().done(function() {
					d.fadeIn();
				});
				$("#headerText").text(name);
				if(openFunctions.hasOwnProperty(id)) openFunctions[id]();
			}});
		});
		
		NavBar_Init("div.navBar", navs);
	}
	
	Views_OpenFunction = function(viewName, func) {
		openFunctions[viewName] = func;
	};
})();

