Debug("This is a test plan");

Debug("Taking a snapshot");
var snapshot = Snapshot();

Debug("Got snapshot: " + snapshot);

Debug("Let's run BatchTest");

var result = BatchTest({}, "FirstTest");

Debug("BatchTest done. Result: " + result);

Debug("Restoring snapshot");
snapshot.Restore();

Debug("Running BatchTest again, telling it to fail");

var result2 = BatchTest({PLEASEFAIL: true}, "SecondTest");

Debug("Result: " + result2);
