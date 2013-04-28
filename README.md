TestVisor
=========

Software to manage automated testing in a virtualized environment.

The Vision
----------
A virtual machine hypervisor manager (or "Führer") able to run sequences of tests on a virtual machine instance,
making use of snapshotting to deal with real world automated testing in a state-controlled way.
There'll be a nice web interface to control everything from, a video feed
and support for multiple hypervisors and test types.

Project Status
--------------
Still drawing lines in the sand, some abstractions fairly complete.
Can manage batch file tests with test harness code to operate everything,
collecting results into the host's filesystem. No real work on the Web side thus far.  
There's a [Video](http://www.youtube.com/watch?v=g7Hf0I2mp88)

Concepts:
---------

### VM Driver
A backend for talking to a hypervisor.
Currently supported are:  
- Oracle VirtualBox

### VM Agent
Program to run on the guest, providing file upload/download, directory inspection and program control.
Will provide live screen once the rest of the solution is ready.

<a id="testDrivers"></a>
### Test Driver
Used to manage a number tests of a particular kind.
A test driver must know how to accept uploaded tests in some format or another,
process and store them away, download the necessary parts to the guest, invoke the test on the guest
and pick up the results.  
Currently supported are:
- Windows Batch Files

In Future:  
- TestComplete
- You name it

### Test Plan
A representation of tests to run on the target along with information to control snapshotting.
Currently undecided; will likely be exposed as an API for an embedded javascript engine before anything else.  
Can use [Blockly](http://code.google.com/p/blockly/) on the frontend :)

The Components:
--------------

### VMAgent
Executable server which runs the guest, providing a kind of combined FTP/SSH functionality over HTTP.
Also includes a library which wraps up communication with the server.
This will be extended to include a video feed at some point.

### VMLib
Abstraction layer for various hypervisors.

### VMTestLib
Controls hypervisor abstraction according to test plans.

### WebLib
Stuff needed for the web frontend.

### TestVisor
This pulls it all together into a nice web app.

There will surely be more components created as development proceeds.
