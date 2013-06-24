// Autogenerated RPC code:
function GenRPC(url, methodName, argNames) {
	return function(args, success, fail) {
		var reqKey = null;

		// The initial request
		var doRequest = function() {
			ajax(url, {Args: args}, callback);
		}

		// The poll request
		var doPoll = function(withArgs) {
			ajax(url, {Key: reqKey}, callback);
		}

		// The callback to handle the responses
		var callback = function(data) {
			reqKey = data.Key;
			if(data.Status === 'SUCCESS') {
				success(data.Result);
			} else if(data.Status === 'ERROR') {
				if(fail !== undefined) fail(data.ErrorText);
			} else if(data.Status === 'BUSY') {
				setTimeout(doPoll, 1000);		// Poll again in a couple of seconds if busy.
			}
		}

		for(var i in argNames) {
			if(!args.hasOwnProperty(argNames[i])) {
				throw new Error('[RPC Error] Argument not provided:' + argNames[i]);
			}
		}

		doRequest();
	}
}

var Results_GetTestResults = GenRPC('/Ajax/Results_GetTestResults', 'Results_GetTestResults', ["TestPlanId","Skip","Limit"]);
var Plans_Start = GenRPC('/Ajax/Plans_Start', 'Plans_Start', ["TestPlanId","Arguments"]);
var Plans_SetSource = GenRPC('/Ajax/Plans_SetSource', 'Plans_SetSource', ["TestPlanId","Source"]);
var Plans_GetSource = GenRPC('/Ajax/Plans_GetSource', 'Plans_GetSource', ["TestPlanId"]);
var Plans_SetInfo = GenRPC('/Ajax/Plans_SetInfo', 'Plans_SetInfo', ["Info"]);
var Plans_Create = GenRPC('/Ajax/Plans_Create', 'Plans_Create', ["Info"]);
var Plans_GetTestPlans = GenRPC('/Ajax/Plans_GetTestPlans', 'Plans_GetTestPlans', []);
var Machines_GetMachines = GenRPC('/Ajax/Machines_GetMachines', 'Machines_GetMachines', []);