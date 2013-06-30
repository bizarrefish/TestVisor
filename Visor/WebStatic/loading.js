

var Loading_Start = function(msg) {
	if(msg === undefined) {
		msg = ""
	}
	
	$("div#loading span").text("Loading " + msg + "...")
	$("div#loading").fadeIn();
}

var Loading_Stop = function() {
	$("div#loading").hide();
}