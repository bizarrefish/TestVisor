/*
	Trying to deal with paginated views
*/

var Views_Init = function() {
	var navs = []
	
	var viewObj = {};
	
	var viewDivs = $(".view").each(function() {
		var d = $(this);
		var id = d.attr("id");
		var name = d.children(".heading").text();
		d.hide();
		
		var openFunc = function() {};
		var closeFunc = function() {};
		
		viewObj[id] = {
			OnOpen: function(handler) {
				var old = openFunc;
				openFunc = function() { old(); handler() };
			},
			
			OnClose: function(handler) {
				var old = closeFunc;
				closeFunc = function() { old(); handler(); };
			}
		};
		
		navs.push({ Name: name, Id: "nav-" + id, Function: function() {
			viewDivs.hide().promise().done(function() {
				d.fadeIn();
			});
			$("#headerText").text(name);
			
			openFunc();
		}});
	});
	
	NavBar_Init("div.navBar", navs);
	return viewObj;
}

