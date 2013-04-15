function ajax(uri, params, callback)
{
	 $.ajax({
	 	url: uri,
	 	dataType: "json",
	 	data: params
	 }).done(function(data, status) {
	 	if(status == 200) callback(data);
	 });
}

function GenRPC(methodName, argNames) {
	return function(req, success, fail) {
		var callback = function(data) {
			if(data.Status === 'SUCCESS') {
				success(data.Result);
			} else if(data.Status === 'FAILED') {
				if(fail !== undefined) fail(data.Result);
			} else if(data.Status === 'PENDING') {
				setTimeout(callback, 2000);
			}
		}
		for(var i in argNames) {
			if(req.hasOwnProperty(argNames[i])) {
				throw new Error('[RPC Error] Argument not provided:' + argNames[i]);
			}
		}
		ajax('/' + methodName, req, callback);
	}
}

var GetMachines = GenRPC('GetMachines', []);
var GetSnapshots = GenRPC('GetSnapshots', ["machineId"]);

var SessionId

function MakeJsonFunc(where)
{
	return function(f, d)
	{
		$.ajax({
			url: where,
			dataType: "json",
			data: d !== undefined ? d : {}
		}).done(function(data, status) {
			f(data)
		});
	}
}

var GetMachines = MakeJsonFunc("/GetMachines");

// {machine: machineId}
var GetSnapshots = MakeJsonFunc("/GetSnapshots");

// {machine: machineId, snapshot: snapshotId}
var StartMachine = MakeJsonFunc("/StartMachine");

// {machine: machineId}
var GetMachineStatus = MakeJsonFunc("/GetMachineStatus");
