/*
	Page Modules
*/

// Takes module id
var Module_Add;

// Takes list of module ids
var Module_SetDisplayed;

(function() {
	var modules = $();
	
	Module_Add = function(modId) {
		modules = modules.add("#" + modId);
	}
	
	Module_SetDisplayed = function(moduleList) {
		modules.fadeOut().promise().done(function() {
			for(var i in moduleList) {
				var id = moduleList[i];
				
				$("#" + id).fadeIn();
			}
		});
	}
})();
