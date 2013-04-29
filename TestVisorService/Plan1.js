Debug("This is Plan1");
Debug("Taking a snapshot");
var snapshot = Snapshot();
Debug("Got snapshot: " + snapshot);

Debug("Let's run BatchTest");
var result = MyBatchFileTest({}, "Succeedey");

Debug("BatchTest done. Result: " + result);

Debug("Restoring snapshot");
snapshot.Restore();

Debug("Running BatchTest again, in faily way");
var result2 = MyBatchFileTest({PLEASEFAIL: true}, "Faily");
Debug("Result: " + result2);
