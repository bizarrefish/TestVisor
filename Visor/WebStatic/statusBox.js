
/*
box: jQuery path
statusMap: { statusName: statusObject, statusName: statusObject}
statusObject: { icon: url, desc: description }
*/
function StatusBox_Init(box, statusMap) {
	var objectNodes = [];
	
	var firstStatus = null;
	for(var statusName in statusMap) {
		var statusObject = statusMap[statusName];
		if(firstStatus === null) firstStatus = statusName;
		
		$(box).append('<div class="statusObject ' + statusName + '"><div class="statusDesc">' + statusObject.desc +
			'</div><img class="icon" src="' + statusObject.icon + '"></div>');
	}
	
	StatusBox_Update(box, firstStatus);
}

function StatusBox_Update(box, statusName) {
	$(box).children("div.statusObject:not(." + statusName + ")").fadeOut(function() {;
		$(box).children("div.statusObject." + statusName).fadeIn();});
}
