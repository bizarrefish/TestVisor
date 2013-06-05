/*
	Nav bar library
*/

/*
	bar is selector for nav bar
	buttonSpec is structure:
	[
		{Name: "Page 1", Id: "P1", Function: function() {} },
		{Name: "Page 2", Id: "P2", Function: function() {} }
	]
	
	current is current page name
*/

function NavBar_Init(bar, buttonSpec) {
	for(var i in buttonSpec)
	{
		var button = buttonSpec[i];
		$(bar).append('<div class="navButton" id="' + button.Id + '">' + button.Name + '</div>');
		
		(function() {
			var id = button.Id;
			var f = button.Function;
			$("div.navButton#" + id).click(function() {
				$("div.navButton").removeClass("selected");
				$(this).addClass("selected");
				f();
			});
		})();
	}
}