TestVisor
=========

Software to manage automated testing in a virtualized environment.

The Vision
----------
There will be a nice ajax web interface, offering complete control of tests, testplans and virtual machines.
We'll also be able to stream a live feed from the VM via a video tag and some kind of RDP to OGG bridge or something :)

### Test Plans
A test plan brings together a collection of tests in a sequenced hierarchy, and may be expressed thus:
> TestPlan: Testplan-001  
> {  
>   InstallerTest64 | InstallerTest32  
>   {  
>   Database1Test | Database2Test  
>   OpenDocumentsTest  
>   UninstallerTest  
>   }  
>   UninstallerTest  
> }  

The resulting sequence of actions would be:

1. Snapshot 1
2. InstallerTest64
3. Snapshot 2
4. Database1Test
5. OpenDocumentsTest
6. UninstallerTest
7. Restore 2
8. Database2Test
9. Restore 1
10. InstallerTest32
11. Snapshot 3
12. Database1Test
13. OpenDocumentsTest
14. UninstallerTest
15. Restore 3
16. Database2Test
17. OpenDocumentsTest
18. UninstallerTest
19. Restore 2
20. UninstallerTest


The Components:
--------------
- VMLib
- VMTestLib
- WebLib
- TestVisor

### VMLib
Abstraction layer for various hypervisors.

### VMTestLib
Controls hypervisor abstraction according to test plans.

### WebLib
Stuff needed for the web frontend.

### TestVisor
This pulls it all together into a nice web app.

There will surely be more components created as development proceeds.
