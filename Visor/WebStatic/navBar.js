/*
	Nav bar library
*/

/*
	bar is selector for nav bar
	buttonSpec is structure:
	[
		{Name: "Page 1", Url: "Page1.html"},
		{Name: "Page 2", Url: "Page2.html"}
	]
	
	current is current page name
*/
function NavBar_Init(bar, buttonSpec, current) {
	for(var i in buttonSpec)
	{
		var button = buttonSpec[i];
		$(bar).append('<a href="' + button.Url + '"><div class="navButton">' + button.Name + '</div></a>');
	}
}

function NavBar_SetPage(bar, pageName)
{
	$(bar).find("div.navButton").each(function() {
		var t = $(this)
		if(t.text() == pageName) t.addClass('selected');
		else t.removeClass('selected');
	});
}
