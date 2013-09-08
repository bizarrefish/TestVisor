
TestVisor
=========
This is a mad attempt to formalize automated testing in virtual machines via a web interface. 
It's demoable at the moment, and maybe even useful to someone, but there is a distinct lack of functionality
and polish.  

Testing
-------
If you write software that people are supposed to use
and enjoy being able to show that certain bad things probably won't happen when they do, you most likely
use automated testing.
Unit testing is probably the most commonly talked about. Unit testing is generally highly granular,
and great for checking that an algorithm isn't doing something wrong.  
That's all very well, but for some apps, unless you have complete control over the target environment,
the complexities of modern desktop computing quickly create potential for issues which unit testing can't help with.


Example
-------
You develop a program which a user installs on their home computer, and
you'd like to make sure the user's "/usr" directory doesn't get deleted when they uninstall it. 

Running the uninstaller yourself in something like the target environment
would be able to give you some degree of certainty that this wouldn't happen,
but running an uninstaller every time you commit code is not fun.


Automation
----------
Like unit tests, it would surely be beneficial to run a suite of these higher-level functional tests
on a regular basis, maybe as part of a continuous integration job. This is tricky, as this kind of testing can be
highly dependent on the state of the machine; ideally you want to run the tests in some kind of isolation. 
So what to do; reinstall the OS before each test? No, that's stupid. 
Using virtual machines and snapshotting is a far better idea.


Details
-------
Test plans are written in Javascript(executed on the server with Rhino).
They decide what tests to run, on what VM, and with what parameters. 

A test is something which can be downloaded and run on a virtual machine
and can also produce results/artifacts. 

There are pluggable hypervisor modules(currently just VirtualBox),
and pluggable test type modules(currently just Windows batch files). 


To Use
------
You should just need a modern Linux machine with Virtualbox and IKVM(ikvmc) installed. 
After cloning the repo, run the Makefile. 
Pop open Visor.sln in monodevelop.
Build VMAgent, and download the produced exe into a Windows virtual machine. 
After starting it as administrator, snapshot the VM and name the snapshot "TEST_INIT".
Now kill the VM, and run the "Visor" project. 
Browse to http://localhost:8080 and look around. 
